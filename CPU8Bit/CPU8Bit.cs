using FSCPU.Cycles;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace FSCPU;

/// <summary>
/// Émulateur de processeur 8 bits avec registres, ALU et unité de contrôle
/// Version avec timing réaliste basé sur les cycles d'horloge + appels système
/// </summary>
/// <remarks>
/// Sur l'ordinateur virtuel simulé ici (style Amstrad CPC et microprocesseur 8 bits), les valeurs 16 bits sont organisées en little-endian :
/// >  Low byte (octet de poids faible) est stocké à l'adresse la plus basse.
/// >  High byte (octet de poids fort) est stocké à l'adresse suivante (adresse + 1).
/// Exemple : Si on écrit la valeur 0x1234 à l'adresse 0x1000 :
/// >  0x1000 contient 0x34 (Low byte)
/// >  0x1001 contient 0x12 (High byte)
/// Cette organisation est utilisée dans les méthodes :
/// >  ReadWord(ushort address) : lit d'abord le Low byte, puis le High byte.
/// >  WriteWord(ushort address, ushort value) : écrit d'abord le Low byte, puis le High byte.
/// C'est le format standard sur la plupart des processeurs 8 bits et sur l'Amstrad CPC.
/// </remarks>
public class CPU8Bit
{
    // Registres généraux (8 bits chacun) - utilisation de champs au lieu de propriétés pour permettre ref
    private byte _regA;
    private byte _regB;
    private byte _regC;
    private byte _regD;
    private byte _regE;
    private byte _regF;

    public byte A
    {
        get => _regA;
        set => _regA = value;
    }

    public byte B
    {
        get => _regB;
        set => _regB = value;
    }

    public byte C
    {
        get => _regC;
        set => _regC = value;
    }

    public byte D
    {
        get => _regD;
        set => _regD = value;
    }

    public byte E
    {
        get => _regE;
        set => _regE = value;
    }

    public byte F
    {
        get => _regF;
        set => _regF = value;
    }

    // Registres généraux (16 bits chacun) - utilisation de champs au lieu de propriétés pour permettre ref
    private ushort _regDA;
    private ushort _regDB;

    public ushort DA
    {
        get => _regDA;
        set => _regDA = value;
    }

    public ushort DB
    {
        get => _regDB;
        set => _regDB = value;
    }

    // Registres d'index (16 bits chacun) - utilisation de champs au lieu de propriétés pour permettre ref
    private ushort _regIDX;
    private ushort _regIDY;

    public ushort IDX
    {
        get => _regIDX;
        set => _regIDX = value;
    }

    public ushort IDY
    {
        get => _regIDY;
        set => _regIDY = value;
    }

    // Registres spéciaux (16 bits)
    public ushort PC { get; set; }  // Program Counter
    public ushort SP { get; set; }  // Stack Pointer

    // Status Register / Flags (8 bits)
    public StatusRegister SR { get; set; }

    // Référence vers la mémoire et les bus
    public Memory Memory { get; private set; }
    public ALU ALU { get; private set; }

    // Gestionnaire d'horloge pour le timing réaliste
    public ClockManager Clock { get; private set; }

    // Gestionnaire d'appels système
    public SystemCallManager? SystemCalls { get; set; }

    // État du processeur
    public bool IsRunning { get; private set; }

    // Statistiques d'exécution
    public long TotalCyclesExecuted { get; private set; }
    public long TotalInstructionsExecuted { get; private set; }

    public CPU8Bit(Memory memory)
    {
        Memory = memory ?? throw new ArgumentNullException(nameof(memory));
        ALU = new ALU(this);
        SR = new StatusRegister();
        Clock = new ClockManager(this);

        // Le SystemCallManager sera injecté depuis FormMain
        // pour éviter les dépendances circulaires

        Reset();
    }

    /// <summary>
    /// Remet le processeur à zéro et démarre sur la ROM BOOT
    /// </summary>
    public void Reset()
    {
        _regA = _regB = _regC = _regD = _regE = _regF = 0;
        _regDA = _regDB = 0;
        _regIDX = _regIDY = 0; // Reset index registers

        // Démarrer sur la ROM au lieu de 0x0000
        PC = RomManager.ROM_BOOT_START; // 0xF400 - Début de la ROM BOOT

        SP = 0xFFFF;  // La pile commence en haut de la mémoire
        SR.Reset();
        IsRunning = false;
        TotalCyclesExecuted = 0;
        TotalInstructionsExecuted = 0;

        // S'assurer que la mémoire est correctement initialisée avec la ROM
        Memory.Reboot();
    }

    /// <summary>
    /// Effectue un démarrage à froid (cold boot)
    /// </summary>
    public void ColdBoot()
    {
        // Effacer complètement la mémoire et recharger la ROM
        Memory.Reboot();

        // Reset complet du CPU
        Reset();

        // Démarrer automatiquement l'exécution de la ROM avec timing réaliste
        Start();
    }

    /// <summary>
    /// Effectue un redémarrage à chaud (warm boot)
    /// </summary>
    public void WarmBoot()
    {
        // Reset du CPU seulement, garder la RAM
        Reset();

        // Redémarrer sur la ROM avec timing réaliste
        Start();
    }

    /// <summary>
    /// Démarre l'exécution du processeur avec horloge réaliste
    /// </summary>
    /// <param name="startClock">If true, also start the process clock.</param>
    public void Start(bool startClock = true)
    {
        IsRunning = true;
        if (startClock)
        {
            Clock.Start();
        }
    }

    /// <summary>
    /// Arrête l'exécution du processeur et l'horloge
    /// </summary>
    public void Stop()
    {
        IsRunning = false;
        Clock.Stop();
    }

    /// <summary>
    /// Exécute un cycle d'instruction : Fetch -> Decode -> Execute
    /// VERSION LEGACY pour compatibilité avec les tests
    /// </summary>
    public void ExecuteCycle()
    {
        if (!IsRunning) return;

        // Fetch : Lire l'instruction à l'adresse PC
        byte opcode = Memory.ReadByte(PC);
        PC++;

        // Decode & Execute : Décoder et exécuter l'instruction
        ExecuteInstruction(opcode);

        // Incrémenter les statistiques
        TotalInstructionsExecuted++;
        TotalCyclesExecuted += InstructionCycles.GetCycles(opcode);
    }

    /// <summary>
    /// Exécute un cycle d'instruction avec timing réaliste
    /// NOUVELLE VERSION avec gestion des cycles d'horloge
    /// </summary>
    public void ExecuteCycleWithTiming()
    {
        if (!IsRunning) return;

        // Fetch : Lire l'instruction à l'adresse PC
        byte opcode = Memory.ReadByte(PC);
        PC++;

        // Obtenir le nombre de cycles pour cette instruction
        int cycles = InstructionCycles.GetCycles(opcode);

        // Decode & Execute : Décoder et exécuter l'instruction
        ExecuteInstruction(opcode);

        // Ajouter les cycles d'attente pour cette instruction
        Clock.AddInstructionCycles(cycles - 1); // -1 car le cycle actuel est déjà compté

        // Incrémenter les statistiques
        TotalInstructionsExecuted++;
        TotalCyclesExecuted += cycles;
    }

    /// <summary>
    /// Décode et exécute une instruction basée sur l'opcode
    /// </summary>
    private void ExecuteInstruction(byte opcode)
    {
        switch (opcode)
        {
            case 0x00: // NOP - No Operation
                break;

            case 0x01: // HALT - Stop the processor
                Stop();
                break;

            // === 8-BIT LOAD INSTRUCTIONS ===
            case 0x10: // LDA #imm - Load A with immediate value
                _regA = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regA);
                break;

            case 0x11: // LDB #imm - Load B with immediate value
                _regB = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regB);
                break;

            case 0x12: // LDC #imm - Load C with immediate value
                _regC = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regC);
                break;

            case 0x13: // LDD #imm - Load D with immediate value
                _regD = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regD);
                break;

            case 0x14: // LDE #imm - Load E with immediate value
                _regE = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regE);
                break;

            case 0x15: // LDF #imm - Load F with immediate value
                _regF = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regF);
                break;

            // === 16-BIT LOAD INSTRUCTIONS ===
            case 0x16: // LDDA #imm16 - Load DA with 16-bit immediate value
                _regDA = Memory.ReadWord(PC);
                PC = (ushort)(PC + 2);
                SR.UpdateZeroFlag(_regDA);
                break;

            case 0x17: // LDDB #imm16 - Load DB with 16-bit immediate value
                _regDB = Memory.ReadWord(PC);
                PC = (ushort)(PC + 2);
                SR.UpdateZeroFlag(_regDB);
                break;

            case 0x18: // LDDA addr - Load DA from memory address
                ushort addr = Memory.ReadWord(PC);
                PC = (ushort)(PC + 2);
                _regDA = Memory.ReadWord(addr);
                SR.UpdateZeroFlag(_regDA);
                break;

            case 0x19: // LDDB addr - Load DB from memory address
                addr = Memory.ReadWord(PC);
                PC = (ushort)(PC + 2);
                _regDB = Memory.ReadWord(addr);
                SR.UpdateZeroFlag(_regDB);
                break;

            case 0x1A: // LDIDX #imm16 - Load IDX with 16-bit immediate value
                _regIDX = Memory.ReadWord(PC);
                PC = (ushort)(PC + 2);
                SR.UpdateZeroFlag(_regIDX);
                break;

            case 0x1B: // LDIDY #imm16 - Load IDY with 16-bit immediate value
                _regIDY = Memory.ReadWord(PC);
                PC = (ushort)(PC + 2);
                SR.UpdateZeroFlag(_regIDY);
                break;

            case 0x1C: // LDDC addr - Load C from memory address
                _regC = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regC);
                break;

            case 0x1D: // LDDD addr - Load D from memory address
                _regC = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regC);
                break;

            case 0x1E: // LDDE addr - Load E from memory address
                _regE = Memory.ReadByte(PC++);
                SR.UpdateZeroFlag(_regE);
                break;

            // === 8-BIT ARITHMETIC INSTRUCTIONS ===
            case 0x20: // ADD A,B - Add B to A
                ALU.Add(ref _regA, _regB);
                break;

            case 0x21: // SUB A,B - Subtract B from A
                ALU.Subtract(ref _regA, _regB);
                break;

            // === 16-BIT ARITHMETIC INSTRUCTIONS ===
            case 0x22: // ADD16 DA,DB - Add DB to DA (16-bit)
                ALU.Add16(ref _regDA, _regDB);
                break;

            case 0x23: // SUB16 DA,DB - Subtract DB from DA (16-bit)
                ALU.Subtract16(ref _regDA, _regDB);
                break;

            case 0x24: // INC16 DA - Increment DA
                ALU.Increment16(ref _regDA);
                break;

            case 0x25: // DEC16 DA - Decrement DA
                ALU.Decrement16(ref _regDA);
                break;

            case 0x26: // INC16 DB - Increment DB
                ALU.Increment16(ref _regDB);
                break;

            case 0x27: // DEC16 DB - Decrement DB
                ALU.Decrement16(ref _regDB);
                break;

            // === 8-BIT INCREMENT/DECREMENT/COMPARE INSTRUCTIONS ===
            case 0x28: // INC A - Increment A
                ALU.Increment(ref _regA);
                break;

            case 0x29: // DEC A - Decrement A
                ALU.Decrement(ref _regA);
                break;

            case 0x2A: // INC B - Increment B
                ALU.Increment(ref _regB);
                break;

            case 0x2B: // DEC B - Decrement B
                ALU.Decrement(ref _regB);
                break;

            case 0x2C: // CMP A,B - Compare A with B
                ALU.Compare(_regA, _regB);
                break;

            case 0x2D: // INC C - Increment C
                ALU.Increment(ref _regC);
                break;

            case 0x2E: // DEC C - Decrement C
                ALU.Decrement(ref _regC);
                break;

            case 0x2F: // CMP A,C - Compare A with C
                ALU.Compare(_regA, _regC);
                break;

            // === LOGICAL INSTRUCTIONS ===
            case 0x30: // AND A,B - Logical AND
                ALU.And(ref _regA, _regB);
                break;

            case 0x31: // OR A,B - Logical OR
                ALU.Or(ref _regA, _regB);
                break;

            case 0x32: // XOR A,B - Logical XOR
                ALU.Xor(ref _regA, _regB);
                break;

            case 0x33: // NOT A - Logical NOT
                _regA = (byte)~_regA;
                SR.UpdateZeroFlag(_regA);
                SR.UpdateNegativeFlag(_regA);
                break;

            case 0x34: // SHL A - Shift Left A
                ALU.ShiftLeft(ref _regA);
                break;

            case 0x35: // SHR A - Shift Right A
                ALU.ShiftRight(ref _regA);
                break;

            case 0x36: // AND A,C - Logical AND A with C
                ALU.And(ref _regA, _regC);
                break;

            case 0x37: // OR A,C - Logical OR A with C
                ALU.Or(ref _regA, _regC);
                break;

            case 0x38: // XOR A,C - Logical XOR A with C
                ALU.Xor(ref _regA, _regC);
                break;

            case 0x39: // SHL B - Shift Left B
                ALU.ShiftLeft(ref _regB);
                break;

            // === JUMP INSTRUCTIONS ===
            case 0x40: // JMP addr - Jump to address
                ushort address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = address;
                break;

            case 0x41: // JZ addr - Jump if Zero
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                if (SR.Zero) PC = address;
                break;

            case 0x42: // JNZ addr - Jump if Not Zero
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                if (!SR.Zero) PC = address;
                break;

            case 0x43: // JC addr - Jump if Carry
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                if (SR.Carry) PC = address;
                break;

            case 0x44: // JNC addr - Jump if No Carry
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                if (!SR.Carry) PC = address;
                break;

            case 0x45: // JN addr - Jump if Negative
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                if (SR.Negative) PC = address;
                break;

            case 0x46: // JNN addr - Jump if Not Negative
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                if (!SR.Negative) PC = address;
                break;

            // === STORE INSTRUCTIONS ===
            case 0x50: // STA addr - Store A at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteByte(address, _regA);
                break;

            case 0x51: // STDA addr - Store DA at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteWord(address, _regDA);
                break;

            case 0x52: // STDB addr - Store DB at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteWord(address, _regDB);
                break;

            case 0x53: // STB addr - Store B at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteByte(address, _regB);
                break;

            case 0x54: // STC addr - Store C at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteByte(address, _regC);
                break;

            case 0x55: // STD addr - Store D at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteByte(address, _regD);
                break;

            case 0x56: // STE addr - Store E at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteByte(address, _regE);
                break;

            case 0x57: // STF addr - Store F at address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                Memory.WriteByte(address, _regF);
                break;

            // === SUBROUTINE INSTRUCTIONS ===
            case 0x60: // CALL addr - Call subroutine
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                // Push return address (PC) onto stack (SP)
                SP = (ushort)(SP - 1);
                Memory.WriteByte(SP, (byte)((PC >> 8) & 0xFF)); // High byte
                SP = (ushort)(SP - 1);
                Memory.WriteByte(SP, (byte)(PC & 0xFF));        // Low byte
                PC = address;
                break;

            case 0x61: // RET - Return from subroutine
                // Pop return address
                byte low = Memory.ReadByte(SP);
                SP = (ushort)(SP + 1);
                byte high = Memory.ReadByte(SP);
                SP = (ushort)(SP + 1);
                PC = (ushort)(low | (high << 8));
                break;

            // === STACK INSTRUCTIONS ===
            case 0x70: // PUSH A - Push A onto stack
                SP--;
                Memory.WriteByte(SP, _regA);
                break;

            case 0x71: // POP A - Pop from stack to A
                _regA = Memory.ReadByte(SP);
                SP++;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0x72: // PUSH16 DA - Push DA onto stack (16-bit)
                SP = (ushort)(SP - 2);
                Memory.WriteWord(SP, _regDA);
                break;

            case 0x73: // POP16 DA - Pop from stack to DA (16-bit)
                _regDA = Memory.ReadWord(SP);
                SP = (ushort)(SP + 2);
                SR.UpdateZeroFlag(_regDA);
                break;

            case 0x74: // PUSH B - Push B onto stack
                SP--;
                Memory.WriteByte(SP, _regB);
                break;

            case 0x75: // POP B - Pop from stack to B
                _regB = Memory.ReadByte(SP);
                SP++;
                SR.UpdateZeroFlag(_regB);
                break;

            case 0x76: // PUSH16 DB - Push DB onto stack (16-bit)
                SP = (ushort)(SP - 2);
                Memory.WriteWord(SP, _regDB);
                break;

            case 0x77: // POP16 DB - Pop from stack to DB (16-bit)
                _regDB = Memory.ReadWord(SP);
                SP = (ushort)(SP + 2);
                SR.UpdateZeroFlag(_regDB);
                break;

            case 0x78: // PUSH C - Push C onto stack
                SP--;
                Memory.WriteByte(SP, _regC);
                break;

            case 0x79: // POP C - Pop from stack to C
                _regC = Memory.ReadByte(SP);
                SP++;
                SR.UpdateZeroFlag(_regC);
                break;

            case 0x7A: // PUSH D - Push D onto stack
                SP--;
                Memory.WriteByte(SP, _regD);
                break;

            case 0x7B: // POP D - Pop from stack to D
                _regD = Memory.ReadByte(SP);
                SP++;
                SR.UpdateZeroFlag(_regD);
                break;

            case 0x7C: // PUSH E - Push E onto stack
                SP--;
                Memory.WriteByte(SP, _regE);
                break;

            case 0x7D: // POP E - Pop from stack to E
                _regE = Memory.ReadByte(SP);
                SP++;
                SR.UpdateZeroFlag(_regE);
                break;

            case 0x7E: // PUSH F - Push F onto stack
                SP--;
                Memory.WriteByte(SP, _regF);
                break;

            case 0x7F: // POP F - Pop from stack to F
                _regF = Memory.ReadByte(SP);
                SP++;
                SR.UpdateZeroFlag(_regF);
                break;

            // === MEMORY LOAD INSTRUCTIONS (0x80-0x85) ===
            case 0x90: // LDA addr - Load A from memory address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                _regA = Memory.ReadByte(address);
                SR.UpdateZeroFlag(_regA);
                break;

            case 0x91: // LDB addr - Load B from memory address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                _regB = Memory.ReadByte(address);
                SR.UpdateZeroFlag(_regB);
                break;

            case 0x92: // LDC addr - Load C from memory address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                _regC = Memory.ReadByte(address);
                SR.UpdateZeroFlag(_regC);
                break;

            case 0x93: // LDD addr - Load D from memory address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                _regD = Memory.ReadByte(address);
                SR.UpdateZeroFlag(_regD);
                break;

            case 0x94: // LDE addr - Load E from memory address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                _regE = Memory.ReadByte(address);
                SR.UpdateZeroFlag(_regE);
                break;

            case 0x95: // LDF addr - Load F from memory address
                address = (ushort)(Memory.ReadByte(PC) | (Memory.ReadByte((ushort)(PC + 1)) << 8));
                PC = (ushort)(PC + 2);
                _regF = Memory.ReadByte(address);
                SR.UpdateZeroFlag(_regF);
                break;

            // === REGISTER TRANSFER INSTRUCTIONS (0xA0-0xA7) ===
            case 0xA0: // MOV A,B - Move B to A
                _regA = _regB;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0xA1: // MOV A,C - Move C to A
                _regA = _regC;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0xA2: // MOV B,A - Move A to B
                _regB = _regA;
                SR.UpdateZeroFlag(_regB);
                break;

            case 0xA3: // MOV B,C - Move C to B
                _regB = _regC;
                SR.UpdateZeroFlag(_regB);
                break;

            case 0xA4: // MOV C,A - Move A to C
                _regC = _regA;
                SR.UpdateZeroFlag(_regC);
                break;

            case 0xA5: // MOV C,B - Move B to C
                _regC = _regB;
                SR.UpdateZeroFlag(_regC);
                break;

            case 0xA6: // SWP A,B - Swap A and B
                byte temp = _regA;
                _regA = _regB;
                _regB = temp;
                SR.UpdateZeroFlag(_regA); // Update flags based on new A value
                break;

            case 0xA7: // SWP A,C - Swap A and C
                temp = _regA;
                _regA = _regC;
                _regC = temp;
                SR.UpdateZeroFlag(_regA); // Update flags based on new A value
                break;

            case 0xA8: // SWP A,D - Swap A and D
                temp = _regA;
                _regA = _regD;
                _regD = temp;
                SR.UpdateZeroFlag(_regA); // Update flags based on new A value
                break;

            case 0xA9: // SWP A,E - Swap A and E
                temp = _regA;
                _regA = _regE;
                _regE = temp;
                SR.UpdateZeroFlag(_regA); // Update flags based on new A value
                break;

            case 0xAA: // SWP A,F - Swap A and F
                temp = _regA;
                _regA = _regF;
                _regF = temp;
                SR.UpdateZeroFlag(_regA); // Update flags based on new A value
                break;

            case 0xAB: // SWP DA,DB - Swap DA and DB
                ushort dtemp = _regDA;
                _regDA = _regDB;
                _regDB = dtemp;
                SR.UpdateZeroFlag(_regDA); // Update flags based on new DA value
                break;

            case 0xAC: // MOV DA,DB - Move DB to DA
                _regDA = _regDB;
                SR.UpdateZeroFlag(_regDA);
                break;

            case 0xAD: // MOV DB,DA - Move DA to DB
                _regDB = _regDA;
                SR.UpdateZeroFlag(_regDB);
                break;

            case 0xAE: // SWP DA,IDX - Swap DA and IDX
                dtemp = _regDA;
                _regDA = _regIDX;
                _regIDX = dtemp;
                SR.UpdateZeroFlag(_regDA); // Update flags based on new DA value
                break;

            case 0xAF: // SWP DA,IDY - Swap DA and IDY
                dtemp = _regDA;
                _regDA = _regIDY;
                _regIDY = dtemp;
                SR.UpdateZeroFlag(_regDA); // Update flags based on new DA value
                break;

            case 0xB0: // MOV DA,IDX - Move IDX to DA
                _regDA = _regIDX;
                SR.UpdateZeroFlag(_regDA);
                break;

            case 0xB1: // MOV DA,IDY - Move IDY to DA
                _regDA = _regIDY;
                SR.UpdateZeroFlag(_regDA);
                break;

            case 0xB2: // MOV IDX,DA - Move DA to IDX
                _regIDX = _regDA;
                SR.UpdateZeroFlag(_regIDX);
                break;

            case 0xB3: // MOV IYX,DA - Move DA to IDY
                _regIDY = _regDA;
                SR.UpdateZeroFlag(_regIDY);
                break;

            // === RELATIVE JUMP INSTRUCTIONS (0xC0-0xC3) ===
            case 0xC0: // JR offset - Jump Relative (signed 8-bit offset)
                sbyte offset = (sbyte)Memory.ReadByte(PC++);
                PC = (ushort)(PC + offset);
                break;

            case 0xC1: // JRZ offset - Jump Relative if Zero
                offset = (sbyte)Memory.ReadByte(PC++);
                if (SR.Zero) PC = (ushort)(PC + offset);
                break;

            case 0xC2: // JRNZ offset - Jump Relative if Not Zero
                offset = (sbyte)Memory.ReadByte(PC++);
                if (!SR.Zero) PC = (ushort)(PC + offset);
                break;

            case 0xC3: // JRC offset - Jump Relative if Carry
                offset = (sbyte)Memory.ReadByte(PC++);
                if (SR.Carry) PC = (ushort)(PC + offset);
                break;

            // === AUTO-INCREMENT/DECREMENT FOR ARRAYS (0xC4-0xCB) ===
            case 0xC4: // LDAIDX+ - Load A from (IDX), then increment IDX
                _regA = Memory.ReadByte(_regIDX);
                _regIDX++;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0xC5: // LDAIDY+ - Load A from (IDY), then increment IDY
                _regA = Memory.ReadByte(_regIDY);
                _regIDY++;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0xC6: // STAIDX+ - Store A at (IDX), then increment IDX
                Memory.WriteByte(_regIDX, _regA);
                _regIDX++;
                break;

            case 0xC7: // STAIDY+ - Store A at (IDY), then increment IDY
                Memory.WriteByte(_regIDY, _regA);
                _regIDY++;
                break;

            case 0xC8: // LDAIDX- - Load A from (IDX), then decrement IDX
                _regA = Memory.ReadByte(_regIDX);
                _regIDX--;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0xC9: // LDAIDY- - Load A from (IDY), then decrement IDY
                _regA = Memory.ReadByte(_regIDY);
                _regIDY--;
                SR.UpdateZeroFlag(_regA);
                break;

            case 0xCA: // STAIDX- - Store A at (IDX), then decrement IDX
                Memory.WriteByte(_regIDX, _regA);
                _regIDX--;
                break;

            case 0xCB: // STAIDY- - Store A at (IDY), then decrement IDY
                Memory.WriteByte(_regIDY, _regA);
                _regIDY--;
                break;

            case 0xD0: // CMP A, #Imm - Compare A with immediate value
                {
                    byte immediate = Memory.ReadByte(PC++);
                    ALU.Compare(_regA, immediate);
                }
                break;

            case 0xD1: // CMP B, #Imm - Compare B with immediate value
                {
                    byte immediate = Memory.ReadByte(PC++);
                    ALU.Compare(_regB, immediate);
                }
                break;

            case 0xD2: // CMP C, #Imm - Compare C with immediate value
                {
                    byte immediate = Memory.ReadByte(PC++);
                    ALU.Compare(_regC, immediate);
                }
                break;

            case 0xD3: // CMP D, #Imm - Compare D with immediate value
                {
                    byte immediate = Memory.ReadByte(PC++);
                    ALU.Compare(_regD, immediate);
                }
                break;

            case 0xD4: // CMP E, #Imm - Compare E with immediate value
                {
                    byte immediate = Memory.ReadByte(PC++);
                    ALU.Compare(_regE, immediate);
                }
                break;

            case 0xD5: // CMP F, #Imm - Compare F with immediate value
                {
                    byte immediate = Memory.ReadByte(PC++);
                    ALU.Compare(_regF, immediate);
                }
                break;

            case 0xD6: // CMP DA, #Imm16 - Compare DA with immediate value
                {
                    ushort immediate16 = Memory.ReadWord(PC);
                    PC += 2;
                    ALU.Compare(_regDA, immediate16);
                }
                break;

            case 0xD7: // CMP DB, #Imm16 - Compare DB with immediate value
                {
                    ushort immediate16 = Memory.ReadWord(PC);
                    PC += 2;
                    ALU.Compare(_regDB, immediate16);
                }
                break;

            // === INDEX REGISTER INCREMENT/DECREMENT INSTRUCTIONS (0xE0-0xE7) ===
            case 0xE0: // INCIDX - Increment IDX
                _regIDX++;
                SR.UpdateZeroFlag(_regIDX);
                break;

            case 0xE1: // DECIDX - Decrement IDX
                _regIDX--;
                SR.UpdateZeroFlag(_regIDX);
                break;

            case 0xE2: // INCIDY - Increment IDY
                _regIDY++;
                SR.UpdateZeroFlag(_regIDY);
                break;

            case 0xE3: // DECIDY - Decrement IDY
                _regIDY--;
                SR.UpdateZeroFlag(_regIDY);
                break;

            // === INDEX REGISTER ADD IMMEDIATE INSTRUCTIONS (0xE8-0xEB) ===
            case 0xE8: // ADDIDX #imm16 - Add 16-bit immediate to IDX
                {
                    ushort immediate = Memory.ReadWord(PC);
                    PC = (ushort)(PC + 2);
                    uint result = (uint)_regIDX + immediate;
                    SR.SetCarryFlag(result > 0xFFFF); // Set carry if overflow
                    _regIDX = (ushort)result;
                    SR.UpdateZeroFlag(_regIDX);
                }
                break;

            case 0xEA: // ADDIDY #imm16 - Add 16-bit immediate to IDY
                {
                    ushort immediate = Memory.ReadWord(PC);
                    PC = (ushort)(PC + 2);
                    uint result = (uint)_regIDY + immediate;
                    SR.SetCarryFlag(result > 0xFFFF); // Set carry if overflow
                    _regIDY = (ushort)result;
                    SR.UpdateZeroFlag(_regIDY);
                }
                break;

            // === INDEX REGISTER TRANSFER INSTRUCTIONS (0xF1-0xF9) ===
            case 0xF5: // MVIDXIDY - Move IDX to IDY
                _regIDY = _regIDX;
                SR.UpdateZeroFlag(_regIDY);
                break;

            case 0xF6: // MVIDYIDX - Move IDY to IDX
                _regIDX = _regIDY;
                SR.UpdateZeroFlag(_regIDX);
                break;

            case 0xF9: // SWPIDXIDY - Swap IDX and IDY
                {
                    ushort tempXY = _regIDX;
                    _regIDX = _regIDY;
                    _regIDY = tempXY;
                    SR.UpdateZeroFlag(_regIDX); // Update flags based on new IDX value
                }
                break;

            case 0xFE: // Extended instruction set 1
                // Extended instruction opcode can be read from the next byte
                break;

            case 0xFF: // Extended instruction set 2
                // Extended instruction opcode can be read from the next byte
                break;

            default:
                throw new InvalidOperationException($"Unknown instruction: 0x{opcode:X2}");
        }
    }

    /// Obtient la valeur d'un registre par son nom
    /// </summary>
    public byte GetRegister(char register)
    {
        return register switch
        {
            'A' => _regA,
            'B' => _regB,
            'C' => _regC,
            'D' => _regD,
            'E' => _regE,
            'F' => _regF,
            _ => throw new ArgumentException($"Invalid register: {register}")
        };
    }

    /// <summary>
    /// Définit la valeur d'un registre par son nom
    /// </summary>
    public void SetRegister(char register, byte value)
    {
        switch (register)
        {
            case 'A': _regA = value; break;
            case 'B': _regB = value; break;
            case 'C': _regC = value; break;
            case 'D': _regD = value; break;
            case 'E': _regE = value; break;
            case 'F': _regF = value; break;
            default: throw new ArgumentException($"Invalid register: {register}");
        }
        SR.UpdateZeroFlag(value);
    }

    /// <summary>
    /// Get the value of a 16-bit register by its name
    /// </summary>
    public ushort GetRegister16(string register)
    {
        return register.ToUpper() switch
        {
            "DA" => _regDA,
            "DB" => _regDB,
            "IDX" => _regIDX,
            "IDY" => _regIDY,
            "PC" => PC,
            "SP" => SP,
            _ => throw new ArgumentException($"Invalid 16-bit register: {register}")
        };
    }

    /// <summary>
    /// Set the value of a 16-bit register by its name
    /// </summary>
    public void SetRegister16(string register, ushort value)
    {
        switch (register.ToUpper())
        {
            case "DA": _regDA = value; break;
            case "DB": _regDB = value; break;
            case "IDX": _regIDX = value; break;
            case "IDY": _regIDY = value; break;
            case "PC": PC = value; break;
            case "SP": SP = value; break;
            default: throw new ArgumentException($"Invalid 16-bit register: {register}");
        }
        SR.UpdateZeroFlag(value);
    }

    /// <summary>
    /// Calcule les cycles par seconde (CPS) et instructions par seconde (IPS)
    /// </summary>
    public (double CPS, double IPS) GetPerformanceMetrics()
    {
        double actualFreq = Clock.GetActualFrequency();
        double avgCyclesPerInstruction = TotalInstructionsExecuted > 0
            ? (double)TotalCyclesExecuted / TotalInstructionsExecuted
            : 0;

        return (actualFreq, actualFreq / Math.Max(avgCyclesPerInstruction, 1));
    }

    /// <summary>
    /// Obtient des informations de debug sur l'état du CPU
    /// </summary>
    public string GetCpuInfo()
    {
        var (cps, ips) = GetPerformanceMetrics();

        return $"CPU State:\n" +
               $"  PC = 0x{PC:X4} {(RomManager.IsRomAddress(PC) ? "(ROM)" : "(RAM)")}\n" +
               $"  SP = 0x{SP:X4}\n" +
               $"  8-bit: A={A:X2} B={B:X2} C={C:X2} D={D:X2} E={E:X2} F={F:X2}\n" +
               $"  16-bit: DA=0x{DA:X4} DB=0x{DB:X4}\n" +
               $"  Index: IDX=0x{IDX:X4} IDY=0x{IDY:X4} \n" +
               $"  SR = {SR}\n" +
               $"  Running = {IsRunning}\n" +
               $"  Instructions: {TotalInstructionsExecuted:N0}\n" +
               $"  Cycles: {TotalCyclesExecuted:N0}\n" +
               $"  CPS: {cps:F0} Hz\n" +
               $"  IPS: {ips:F0} Hz";
    }

    public void Dispose()
    {
        Clock?.Dispose();
    }
}