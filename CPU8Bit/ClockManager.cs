namespace CPU8Bit;

/// <summary>
/// Gestionnaire d'horloge pour simuler un vrai timing de processeur
/// Chaque instruction a un nombre de cycles spécifique
/// </summary>
public class ClockManager
{
    // Fréquence de base du processeur (en Hz)
    public const long BASE_FREQUENCY = 1_000_000; // 1 MHz (1 million de cycles par seconde)
    
    // Durée d'un cycle en microsecondes (1/1MHz = 1µs)
    public const double CYCLE_DURATION_MICROSECONDS = 1000000.0 / BASE_FREQUENCY;
    
    // Durée d'un cycle en millisecondes pour les timers
    public const double CYCLE_DURATION_MILLISECONDS = CYCLE_DURATION_MICROSECONDS / 1000.0;
    
    private readonly CPU8Bit _cpu;
    private readonly System.Timers.Timer _clockTimer;
    private int _remainingCycles;
    private DateTime _lastCycleTime;
    
    public ClockManager(CPU8Bit cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
        
        // Timer haute précision pour simuler l'horloge
        _clockTimer = new System.Timers.Timer(CYCLE_DURATION_MILLISECONDS);
        _clockTimer.Elapsed += OnClockTick;
        _clockTimer.AutoReset = true;
        
        _remainingCycles = 0;
        _lastCycleTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Démarre l'horloge du processeur
    /// </summary>
    public void Start()
    {
        _lastCycleTime = DateTime.UtcNow;
        _clockTimer.Start();
    }
    
    /// <summary>
    /// Arrête l'horloge du processeur
    /// </summary>
    public void Stop()
    {
        _clockTimer.Stop();
    }
    
    /// <summary>
    /// Ajoute des cycles d'attente pour une instruction
    /// </summary>
    /// <param name="cycles">Nombre de cycles que prend l'instruction</param>
    public void AddInstructionCycles(int cycles)
    {
        _remainingCycles += cycles;
    }
    
    /// <summary>
    /// Gestionnaire de tick d'horloge - exécute les cycles en attente
    /// </summary>
    private void OnClockTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (_cpu == null || !_cpu.IsRunning)
            return;
            
        // Si on a des cycles en attente, on décrémente
        if (_remainingCycles > 0)
        {
            _remainingCycles--;
            return;
        }
        
        // Sinon, exécuter la prochaine instruction
        try
        {
            _cpu.ExecuteCycleWithTiming();
        }
        catch (Exception ex)
        {
            // En cas d'erreur, arrêter l'horloge
            Stop();
            _cpu.Stop();
            throw new InvalidOperationException($"Erreur d'exécution CPU: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Obtient le nombre de cycles restants en attente
    /// </summary>
    public int RemainingCycles => _remainingCycles;
    
    /// <summary>
    /// Calcule la fréquence réelle du CPU en Hz
    /// </summary>
    public double GetActualFrequency()
    {
        // Calcul basé sur le temps écoulé
        var elapsed = DateTime.UtcNow - _lastCycleTime;
        if (elapsed.TotalSeconds <= 0) return 0;
        
        return 1.0 / elapsed.TotalSeconds;
    }
    
    /// <summary>
    /// Informations de debug sur l'horloge
    /// </summary>
    public string GetClockInfo()
    {
        return $"Clock Manager:\n" +
               $"  Fréquence de base: {BASE_FREQUENCY:N0} Hz\n" +
               $"  Durée cycle: {CYCLE_DURATION_MICROSECONDS:F1} µs\n" +
               $"  Cycles en attente: {_remainingCycles}\n" +
               $"  Fréquence réelle: {GetActualFrequency():F0} Hz\n" +
               $"  État: {(_clockTimer.Enabled ? "Actif" : "Arrêté")}";
    }
    
    public void Dispose()
    {
        _clockTimer?.Dispose();
    }
}

/// <summary>
/// Définition des cycles de chaque instruction
/// Basé sur les vrais processeurs 8 bits comme le Z80/6502
/// </summary>
public static class InstructionCycles
{
    // Instructions de base (cycles machine)
    public const int NOP = 1;          // No Operation - 1 cycle
    public const int HALT = 1;         // Halt - 1 cycle
    
    // Instructions de chargement immédiat
    public const int LDA_IMM = 2;      // Load A immediate - 2 cycles (fetch opcode + fetch data)
    public const int LDB_IMM = 2;      // Load B immediate - 2 cycles
    public const int LDC_IMM = 2;      // Load C immediate - 2 cycles
    public const int LDD_IMM = 2;      // Load D immediate - 2 cycles
    public const int LDE_IMM = 2;      // Load E immediate - 2 cycles (same as LDA)
    public const int LDF_IMM = 2;      // Load F immediate - 2 cycles (same as LDA)
    
    // Instructions de chargement 16 bits
    public const int LDDA_IMM16 = 3;   // Load DA immediate (16 bits) - 3 cycles
    public const int LDDB_IMM16 = 3;   // Load DB immediate (16 bits) - 3 cycles
    public const int LDDA_ADDR = 5;    // Load DA from address - 5 cycles (opcode + addr + read word)
    public const int LDDB_ADDR = 5;    // Load DB from address - 5 cycles
    
    // Opérations arithmétiques et logiques
    public const int ADD = 1;          // Addition - 1 cycle (registres internes)
    public const int SUB = 1;          // Soustraction - 1 cycle
    public const int AND = 1;          // AND logique - 1 cycle
    public const int OR = 1;           // OR logique - 1 cycle
    public const int XOR = 1;          // XOR logique - 1 cycle
    public const int NOT = 1;          // NOT logique - 1 cycle
    public const int SHL = 1;          // Décalage logique à gauche - 1 cycle
    public const int SHR = 1;          // Décalage logique à droite - 1 cycle
    
    // Instructions de saut
    public const int JMP = 3;          // Jump - 3 cycles (fetch opcode + fetch address 16-bit)
    public const int JZ = 3;           // Jump if Zero - 3 cycles (same as JMP)
    public const int JNZ = 3;          // Jump if Not Zero - 3 cycles
    public const int JC = 3;           // Jump if Carry - 3 cycles
    public const int JNC = 3;          // Jump if No Carry - 3 cycles
    public const int JN = 3;           // Jump if Negative - 3 cycles
    public const int JNN = 3;          // Jump if Not Negative - 3 cycles
    
    // Instructions de stockage mémoire
    public const int STA = 4;          // Store A - 4 cycles (opcode + address + write)
    public const int STDA = 5;         // Store DA - 5 cycles (16-bit store)
    public const int STDB = 5;         // Store DB - 5 cycles
    
    // Appels système
    public const int SYS_CALL = 8;      // System call - 8 cycles (coûteux car traitement complexe)
    
    // Instructions de pile (sous-routines)
    public const int CALL = 5;         // Call subroutine - 5 cycles (fetch addr + push + jump)
    public const int RET = 4;          // Return - 4 cycles (pop + jump)
    
    // Opérations sur le processeur
    public const int PUSH = 2;         // Empile une valeur - 2 cycles (opcode + valeur)
    public const int POP = 2;          // Dépile une valeur - 2 cycles (opcode + valeur)
    
    // Chargements de mémoire (instructions 0x80-0x85)
    public const int LDA_MEM = 4;      // LDA depuis adresse - 4 cycles (opcode + addr + read)
    public const int LDB_MEM = 4;      // LDB depuis adresse - 4 cycles
    public const int LDC_MEM = 4;      // LDC depuis adresse - 4 cycles
    public const int LDD_MEM = 4;      // LDD depuis adresse - 4 cycles
    public const int LDE_MEM = 4;      // LDE depuis adresse - 4 cycles
    public const int LDF_MEM = 4;      // LDF depuis adresse - 4 cycles
    
    // Transferts entre registres (instructions 0xA0-0xA7)
    public const int MOV_AB = 1;       // MOV A,B - 1 cycle (registres internes)
    public const int MOV_AC = 1;       // MOV A,C - 1 cycle
    public const int MOV_BA = 1;       // MOV B,A - 1 cycle
    public const int MOV_BC = 1;       // MOV B,C - 1 cycle
    public const int MOV_CA = 1;       // MOV C,A - 1 cycle
    public const int MOV_CB = 1;       // MOV C,B - 1 cycle
    public const int SWP_AB = 2;       // SWP A,B - 2 cycles (nécessite un stockage temporaire)
    public const int SWP_AC = 2;       // SWP A,C - 2 cycles
    
    // Sauts relatifs (instructions 0xC0-0xC3)
    public const int JR = 2;           // JR offset - 2 cycles (opcode + offset)
    public const int JRZ = 2;          // JRZ offset - 2 cycles (saut conditionnel relatif)
    public const int JRNZ = 2;         // JRNZ offset - 2 cycles
    public const int JRC = 2;          // JRC offset - 2 cycles
    
    // Auto-increment/decrement array operations (0xC4-0xCB)
    public const int LDAIX1 = 2;       // LDAIX1+ - 2 cycles (load + auto-increment)
    public const int LDAIY1 = 2;       // LDAIY1+ - 2 cycles
    public const int STAIX1 = 2;       // STAIX1+ - 2 cycles (store + auto-increment)
    public const int STAIY1 = 2;       // STAIY1+ - 2 cycles
    public const int LDAIX1_DEC = 2;   // LDAIX1- - 2 cycles (load + auto-decrement)
    public const int LDAIY1_DEC = 2;   // LDAIY1- - 2 cycles
    public const int STAIX1_DEC = 2;   // STAIX1- - 2 cycles (store + auto-decrement)
    public const int STAIY1_DEC = 2;   // STAIY1- - 2 cycles
    
    // Index register increment/decrement (0xE0-0xE7)
    public const int INCIX1 = 2;       // INCIX1 - 2 cycles (16-bit increment)
    public const int DECIX1 = 2;       // DECIX1 - 2 cycles (16-bit decrement)
    public const int INCIY1 = 2;       // INCIY1 - 2 cycles
    public const int DECIY1 = 2;       // DECIY1 - 2 cycles
    public const int INCIX2 = 2;       // INCIX2 - 2 cycles
    public const int DECIX2 = 2;       // DECIX2 - 2 cycles
    public const int INCIY2 = 2;       // INCIY2 - 2 cycles
    public const int DECIY2 = 2;       // DECIY2 - 2 cycles
    
    // Index register add immediate (0xE8-0xEB)
    public const int ADDIX1 = 4;       // ADDIX1 #imm16 - 4 cycles (opcode + 16-bit immediate + add)
    public const int ADDIX2 = 4;       // ADDIX2 #imm16 - 4 cycles
    public const int ADDIY1 = 4;       // ADDIY1 #imm16 - 4 cycles
    public const int ADDIY2 = 4;       // ADDIY2 #imm16 - 4 cycles
    
    // Index register transfer instructions (0xF1-0xF9)
    public const int MVIX1IX2 = 2;     // MVIX1IX2 - 2 cycles (16-bit transfer)
    public const int MVIX2IX1 = 2;     // MVIX2IX1 - 2 cycles
    public const int MVIY1IY2 = 2;     // MVIY1IY2 - 2 cycles
    public const int MVIY2IY1 = 2;     // MVIY2IY1 - 2 cycles
    public const int MVIX1IY1 = 2;     // MVIX1IY1 - 2 cycles
    public const int MVIY1IX1 = 2;     // MVIY1IX1 - 2 cycles
    public const int SWPIX1IX2 = 3;    // SWPIX1IX2 - 3 cycles (16-bit swap)
    public const int SWPIY1IY2 = 3;    // SWPIY1IY2 - 3 cycles
    public const int SWPIX1IY1 = 3;    // SWPIX1IY1 - 3 cycles
    
    /// <summary>
    /// Obtient le nombre de cycles pour un opcode donné
    /// </summary>
    /// <param name="opcode">Code d'instruction</param>
    /// <returns>Nombre de cycles machine</returns>
    public static int GetCycles(byte opcode)
    {
        return opcode switch
        {
            // Instructions de base
            0x00 => NOP,        // NOP
            0x01 => HALT,       // HALT
            
            // Chargements immédiats 8 bits
            0x10 => LDA_IMM,    // LDA #imm
            0x11 => LDB_IMM,    // LDB #imm
            0x12 => LDC_IMM,    // LDC #imm
            0x13 => LDD_IMM,    // LDD #imm
            0x14 => LDA_IMM,    // LDE #imm (même timing que LDA)
            0x15 => LDA_IMM,    // LDF #imm (même timing que LDA)
            
            // Chargements 16 bits
            0x16 => LDDA_IMM16, // LDDA #imm16 - 3 cycles (opcode + 16 bits de données)
            0x17 => LDDB_IMM16, // LDDB #imm16 - 3 cycles
            0x18 => LDDA_ADDR,  // LDDA addr - 5 cycles (opcode + addr + lecture mot)
            0x19 => LDDB_ADDR,  // LDDB addr - 5 cycles
            0x1A => 3,          // LDIX1 #imm16 - 3 cycles (16-bit load)
            0x1B => 3,          // LDIY1 #imm16 - 3 cycles
            
            // Arithmétique²
            0x20 => ADD,        // ADD A,B
            0x21 => SUB,        // SUB A,B
            0x22 => 2,          // ADD16 DA,DB - 2 cycles (opération 16 bits)
            0x23 => 2,          // SUB16 DA,DB - 2 cycles
            0x24 => 2,          // INC16 DA - 2 cycles
            0x25 => 2,          // DEC16 DA - 2 cycles
            0x26 => 2,          // INC16 DB - 2 cycles
            0x27 => 2,          // DEC16 DB - 2 cycles
            0x28 => 1,          // INC A - 1 cycle
            0x29 => 1,          // DEC A - 1 cycle
            0x2A => 1,          // INC B - 1 cycle
            0x2B => 1,          // DEC B - 1 cycle
            0x2C => 1,          // CMP A,B - 1 cycle
            0x2D => 1,          // INC C - 1 cycle
            0x2E => 1,          // DEC C - 1 cycle
            0x2F => 1,          // CMP A,C - 1 cycle
            
            // Opérations logiques
            0x30 => AND,        // AND A,B
            0x31 => OR,         // OR A,B
            0x32 => XOR,        // XOR A,B - 1 cycle
            0x33 => NOT,        // NOT A - 1 cycle
            0x34 => SHL,        // SHL A - 1 cycle
            0x35 => SHR,        // SHR A - 1 cycle
            0x36 => AND,        // AND A,C
            0x37 => OR,         // OR A,C
            0x38 => XOR,        // XOR A,C - 1 cycle
            0x39 => SHL,        // SHL B - 1 cycle
            
            // Instructions de saut
            0x40 => JMP,        // JMP addr
            0x41 => JMP,        // JZ addr - même timing
            0x42 => JMP,        // JNZ addr
            0x43 => JMP,        // JC addr
            0x44 => JMP,        // JNC addr
            0x45 => JMP,        // JN addr
            0x46 => JMP,        // JNN addr
            
            // Instructions de stockage
            0x50 => STA,        // STA addr
            0x51 => 5,          // STDA addr - 5 cycles (16 bits)
            0x52 => 5,          // STDB addr - 5 cycles
            0x53 => STA,        // STB addr
            0x54 => STA,        // STC addr
            0x55 => STA,        // STD addr
            
            // Sous-routines
            0x60 => CALL,       // CALL addr
            0x61 => RET,        // RET
            
            // Opérations sur la pile
            0x70 => PUSH,       // Empile A - 2 cycles (opcode + push)
            0x71 => POP,        // Dépile A - 2 cycles (opcode + pop)
            0x72 => 3,          // PUSH16 DA - 3 cycles (push 16 bits)
            0x73 => 3,          // POP16 DA - 3 cycles (dépile 16 bits)
            0x74 => PUSH,       // Empile B
            0x75 => POP,        // Dépile B
            0x76 => 3,          // PUSH16 DB
            0x77 => 3,          // POP16 DB
            0x78 => PUSH,       // Empile C
            0x79 => POP,        // Dépile C
            
            // Index register instructions
            0x7A => 3,          // LDIDX1 #imm16 - 3 cycles
            0x7B => 3,          // LDIDX2 #imm16 - 3 cycles
            0x7C => 3,          // LDIDY1 #imm16 - 3 cycles
            0x7D => 3,          // LDIDY2 #imm16 - 3 cycles
            0x7E => 2,          // INCIDX1 - 2 cycles (16-bit operation)
            0x7F => 2,          // DECIDX1 - 2 cycles
            
            // Chargements de mémoire (0x80-0x8F)
            0x80 => LDA_MEM,    // LDA addr - 4 cycles (opcode + addr + lecture)
            0x81 => LDB_MEM,    // LDB addr - 4 cycles
            0x82 => LDC_MEM,    // LDC addr - 4 cycles
            0x83 => LDD_MEM,    // LDD addr - 4 cycles
            0x84 => LDE_MEM,    // LDE addr - 4 cycles
            0x85 => LDF_MEM,    // LDF addr - 4 cycles
            0x86 => 2,          // INCIDX2 - 2 cycles
            0x87 => 2,          // DECIDX2 - 2 cycles
            0x88 => 2,          // INCIDY1 - 2 cycles
            0x89 => 2,          // DECIDY1 - 2 cycles
            0x8A => 2,          // INCIDY2 - 2 cycles
            0x8B => 2,          // DECIDY2 - 2 cycles
            0x8C => 2,          // LDA (IDX1) - 2 cycles (indexed load)
            0x8D => 2,          // LDA (IDX2) - 2 cycles
            0x8E => 2,          // LDA (IDY1) - 2 cycles
            0x8F => 2,          // LDA (IDY2) - 2 cycles
            
            // Indexed store operations (0x90-0x9B)
            0x90 => 3,          // LDA (IDX1+offset) - 3 cycles (opcode + offset + indexed load)
            0x91 => 3,          // LDB (IDX1+offset) - 3 cycles
            0x92 => 3,          // LDA (IDY1+offset) - 3 cycles
            0x93 => 3,          // LDB (IDY1+offset) - 3 cycles
            0x94 => 3,          // STA (IDX1+offset) - 3 cycles (opcode + offset + indexed store)
            0x95 => 3,          // STB (IDX1+offset) - 3 cycles
            0x96 => 3,          // STA (IDY1+offset) - 3 cycles
            0x97 => 3,          // STB (IDY1+offset) - 3 cycles
            0x98 => 3,          // PUSHIDY1 - 3 cycles
            0x99 => 3,          // POPIDY1 - 3 cycles
            0x9A => 3,          // PUSHIDY2 - 3 cycles
            0x9B => 3,          // POPIDY2 - 3 cycles
            
            // Transferts entre registres (0xA0-0xA7)
            0xA0 => MOV_AB,     // MOV A,B - 1 cycle (registres internes)
            0xA1 => MOV_AC,     // MOV A,C - 1 cycle
            0xA2 => MOV_BA,     // MOV B,A - 1 cycle
            0xA3 => MOV_BC,     // MOV B,C - 1 cycle
            0xA4 => MOV_CA,     // MOV C,A - 1 cycle
            0xA5 => MOV_CB,     // MOV C,B - 1 cycle
            0xA6 => SWP_AB,     // SWP A,B - 2 cycles (nécessite un stockage temporaire)
            0xA7 => SWP_AC,     // SWP A,C - 2 cycles
            
            // Sauts relatifs (0xC0-0xC3)
            0xC0 => JR,         // JR offset - 2 cycles (opcode + offset)
            0xC1 => JRZ,        // JRZ offset - 2 cycles (saut conditionnel relatif)
            0xC2 => JRNZ,       // JRNZ offset - 2 cycles
            0xC3 => JRC,        // JRC offset - 2 cycles
            
            // Auto-increment/decrement array operations (0xC4-0xCB)
            0xC4 => 2,          // LDAIX1+ - 2 cycles (load + auto-increment)
            0xC5 => 2,          // LDAIY1+ - 2 cycles
            0xC6 => 2,          // STAIX1+ - 2 cycles (store + auto-increment)
            0xC7 => 2,          // STAIY1+ - 2 cycles
            0xC8 => 2,          // LDAIX1- - 2 cycles (load + auto-decrement)
            0xC9 => 2,          // LDAIY1- - 2 cycles
            0xCA => 2,          // STAIX1- - 2 cycles (store + auto-decrement)
            0xCB => 2,          // STAIY1- - 2 cycles
            
            // Index register increment/decrement (0xE0-0xE7)
            0xE0 => 2,          // INCIX1 - 2 cycles (16-bit increment)
            0xE1 => 2,          // DECIX1 - 2 cycles (16-bit decrement)
            0xE2 => 2,          // INCIY1 - 2 cycles
            0xE3 => 2,          // DECIY1 - 2 cycles
            0xE4 => 2,          // INCIX2 - 2 cycles
            0xE5 => 2,          // DECIX2 - 2 cycles
            0xE6 => 2,          // INCIY2 - 2 cycles
            0xE7 => 2,          // DECIY2 - 2 cycles
            
            // Index register add immediate (0xE8-0xEB)
            0xE8 => 4,          // ADDIX1 #imm16 - 4 cycles (opcode + 16-bit immediate + add)
            0xE9 => 4,          // ADDIX2 #imm16 - 4 cycles
            0xEA => 4,          // ADDIY1 #imm16 - 4 cycles
            0xEB => 4,          // ADDIY2 #imm16 - 4 cycles
            
            // Appel système
            0xF0 => SYS_CALL,   // SYS (appel système)
            
            // Index register transfer instructions (0xF1-0xF9)
            0xF1 => 2,          // MVIX1IX2 - 2 cycles (16-bit transfer)
            0xF2 => 2,          // MVIX2IX1 - 2 cycles
            0xF3 => 2,          // MVIY1IY2 - 2 cycles
            0xF4 => 2,          // MVIY2IY1 - 2 cycles
            0xF5 => 2,          // MVIX1IY1 - 2 cycles
            0xF6 => 2,          // MVIY1IX1 - 2 cycles
            0xF7 => 3,          // SWPIX1IX2 - 3 cycles (16-bit swap)
            0xF8 => 3,          // SWPIY1IY2 - 3 cycles
            0xF9 => 3,          // SWPIX1IY1 - 3 cycles
            
            // Instructions inconnues
            _ => 1              // Par défaut - 1 cycle
        };
    }
}