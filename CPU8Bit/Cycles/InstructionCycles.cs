namespace FSCPU.Cycles;

/// <summary>
/// Definition of cycles for each instruction
/// Based on real 8-bit processors like Z80/6502
/// </summary>
public static class InstructionCycles
{
    // Basic instructions (machine cycles)
    public const int NOP = 1;          // No Operation - 1 cycle
    public const int HALT = 1;         // Halt - 1 cycle

    // Immediate load instructions
    public const int LDA_IMM = 2;      // Load A immediate - 2 cycles (fetch opcode + fetch data)
    public const int LDB_IMM = 2;      // Load B immediate - 2 cycles
    public const int LDC_IMM = 2;      // Load C immediate - 2 cycles
    public const int LDD_IMM = 2;      // Load D immediate - 2 cycles
    public const int LDE_IMM = 2;      // Load E immediate - 2 cycles (same as LDA)
    public const int LDF_IMM = 2;      // Load F immediate - 2 cycles (same as LDA)

    // 16-bit load instructions
    public const int LDDA_IMM16 = 3;   // Load DA immediate (16 bits) - 3 cycles
    public const int LDDB_IMM16 = 3;   // Load DB immediate (16 bits) - 3 cycles
    public const int LDDA_ADDR = 5;    // Load DA from address - 5 cycles (opcode + addr + read word)
    public const int LDDB_ADDR = 5;    // Load DB from address - 5 cycles

    // Arithmetic and logical operations
    public const int ADD = 1;          // Addition - 1 cycle (internal registers)
    public const int SUB = 1;          // Subtraction - 1 cycle
    public const int AND = 1;          // Logical AND - 1 cycle
    public const int OR = 1;           // Logical OR - 1 cycle
    public const int XOR = 1;          // Logical XOR - 1 cycle
    public const int NOT = 1;          // Logical NOT - 1 cycle
    public const int SHL = 1;          // Shift Left - 1 cycle
    public const int SHR = 1;          // Shift Right - 1 cycle

    // Jump instructions
    public const int JMP = 3;          // Jump - 3 cycles (fetch opcode + fetch address 16-bit)
    public const int JZ = 3;           // Jump if Zero - 3 cycles (same as JMP)
    public const int JNZ = 3;          // Jump if Not Zero - 3 cycles
    public const int JC = 3;           // Jump if Carry - 3 cycles
    public const int JNC = 3;          // Jump if No Carry - 3 cycles
    public const int JN = 3;           // Jump if Negative - 3 cycles
    public const int JNN = 3;          // Jump if Not Negative - 3 cycles

    // Memory storage instructions
    public const int STA = 4;          // Store A - 4 cycles (opcode + address + write)
    public const int STDA = 5;         // Store DA - 5 cycles (16-bit store)
    public const int STDB = 5;         // Store DB - 5 cycles

    // Stack instructions (subroutines)
    public const int CALL = 5;         // Call subroutine - 5 cycles (fetch addr + push + jump)
    public const int RET = 4;          // Return - 4 cycles (pop + jump)

    // Processor operations
    public const int PUSH = 2;         // Push a value - 2 cycles (opcode + value)
    public const int POP = 2;          // Pop a value - 2 cycles (opcode + value)

    // Memory loads (instructions 0x90-0x95)
    public const int LDA_MEM = 4;      // LDA from address - 4 cycles (opcode + addr + read)
    public const int LDB_MEM = 4;      // LDB from address - 4 cycles
    public const int LDC_MEM = 4;      // LDC from address - 4 cycles
    public const int LDD_MEM = 4;      // LDD from address - 4 cycles
    public const int LDE_MEM = 4;      // LDE from address - 4 cycles
    public const int LDF_MEM = 4;      // LDF from address - 4 cycles

    // Compare instructions with immediate values
    public const int CMP_IMM = 2;      // Compare register with immediate - 2 cycles (opcode + immediate)
    public const int CMP16_IMM = 3;    // Compare 16-bit register with immediate - 3 cycles (opcode + 16-bit immediate)

    // Register transfers (instructions 0xA0-0xB3)
    public const int MOV_AB = 1;       // MOV A,B - 1 cycle (internal registers)
    public const int MOV_AC = 1;       // MOV A,C - 1 cycle
    public const int MOV_BA = 1;       // MOV B,A - 1 cycle
    public const int MOV_BC = 1;       // MOV B,C - 1 cycle
    public const int MOV_CA = 1;       // MOV C,A - 1 cycle
    public const int MOV_CB = 1;       // MOV C,B - 1 cycle
    public const int SWP_AB = 2;       // SWP A,B - 2 cycles (requires temporary storage)
    public const int SWP_AC = 2;       // SWP A,C - 2 cycles
    public const int SWP_AD = 2;       // SWP A,D - 2 cycles
    public const int SWP_AE = 2;       // SWP A,E - 2 cycles
    public const int SWP_AF = 2;       // SWP A,F - 2 cycles
    public const int SWP_DADB = 2;     // SWP DA,DB - 2 cycles (16-bit swap)
    public const int MOV_DBDA = 1;     // MOV DB,DA - 1 cycle
    public const int MOV_DABD = 1;     // MOV DA,DB - 1 cycle
    public const int SWP_DAIDX = 2;    // SWP DA,IDX - 2 cycles
    public const int SWP_DAIDY = 2;    // SWP DA,IDY - 2 cycles
    public const int MOV_DAIDX = 1;    // MOV DA,IDX - 1 cycle
    public const int MOV_DAIDY = 1;    // MOV DA,IDY - 1 cycle
    public const int MOV_IDXDA = 1;    // MOV IDX,DA - 1 cycle
    public const int MOV_IDYDA = 1;    // MOV IDY,DA - 1 cycle

    // Relative jumps (instructions 0xC0-0xC3)
    public const int JR = 2;           // JR offset - 2 cycles (opcode + offset)
    public const int JRZ = 2;          // JRZ offset - 2 cycles (conditional relative jump)
    public const int JRNZ = 2;         // JRNZ offset - 2 cycles
    public const int JRC = 2;          // JRC offset - 2 cycles

    // Auto-increment/decrement array operations (0xC4-0xCB)
    public const int LDAIX1 = 2;       // LDAIDX+ - 2 cycles (load + auto-increment)
    public const int LDAIY1 = 2;       // LDAIDY+ - 2 cycles
    public const int STAIX1 = 2;       // STAIDX+ - 2 cycles (store + auto-increment)
    public const int STAIY1 = 2;       // STAIDY+ - 2 cycles
    public const int LDAIX1_DEC = 2;   // LDAIDX- - 2 cycles (load + auto-decrement)
    public const int LDAIY1_DEC = 2;   // LDAIDY- - 2 cycles
    public const int STAIX1_DEC = 2;   // STAIDX- - 2 cycles (store + auto-decrement)
    public const int STAIY1_DEC = 2;   // STAIDY- - 2 cycles

    // Index register increment/decrement (0xE0-0xE3)
    public const int INCIX = 2;        // INCIDX - 2 cycles (16-bit increment)
    public const int DECIX = 2;        // DECIDX - 2 cycles (16-bit decrement)
    public const int INCIY = 2;        // INCIDY - 2 cycles
    public const int DECIY = 2;        // DECIDY - 2 cycles

    // Index register add immediate (0xE8, 0xEA)
    public const int ADDIX1 = 4;       // ADDIDX #imm16 - 4 cycles (opcode + 16-bit immediate + add)
    public const int ADDIDY1 = 4;      // ADDIDY #imm16 - 4 cycles

    // Index register transfer instructions (0xF5, 0xF6, 0xF9)
    public const int MVIX1IY1 = 2;     // MVIDXIDY - 2 cycles (16-bit transfer)
    public const int MVIY1IX1 = 2;     // MVIDYIDX - 2 cycles
    public const int SWPIX1IY1 = 3;    // SWPIDXIDY - 3 cycles (16-bit swap)

    /// <summary>
    /// Gets the number of cycles for a given opcode
    /// </summary>
    /// <param name="opcode">Instruction code</param>
    /// <returns>Number of machine cycles</returns>
    public static int GetCycles(byte opcode)
    {
        return opcode switch
        {
            // Basic instructions
            0x00 => NOP,        // NOP
            0x01 => HALT,       // HALT

            // Immediate 8-bit loads
            0x10 => LDA_IMM,    // LDA #imm
            0x11 => LDB_IMM,    // LDB #imm
            0x12 => LDC_IMM,    // LDC #imm
            0x13 => LDD_IMM,    // LDD #imm
            0x14 => LDE_IMM,    // LDE #imm (same timing as LDA)
            0x15 => LDF_IMM,    // LDF #imm (same timing as LDA)

            // 16-bit loads
            0x16 => LDDA_IMM16, // LDDA #imm16 - 3 cycles (opcode + 16 bits of data)
            0x17 => LDDB_IMM16, // LDDB #imm16 - 3 cycles
            0x18 => LDDA_ADDR,  // LDDA addr - 5 cycles (opcode + addr + read word)
            0x19 => LDDB_ADDR,  // LDDB addr - 5 cycles
            0x1A => 3,          // LDIDX #imm16 - 3 cycles (16-bit load)
            0x1B => 3,          // LDIDY #imm16 - 3 cycles
            0x1C => 2,          // LDDC addr - Load C immediate (2 cycles)
            0x1D => 2,          // LDDD addr - Load D immediate (2 cycles)
            0x1E => 2,          // LDDE addr - Load E immediate (2 cycles)

            // Arithmetic
            0x20 => ADD,        // ADD A,B
            0x21 => SUB,        // SUB A,B
            0x22 => 2,          // ADD16 DA,DB - 2 cycles (16-bit operation)
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

            // Logical operations
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

            // Jump instructions
            0x40 => JMP,        // JMP addr
            0x41 => JMP,        // JZ addr - same timing
            0x42 => JMP,        // JNZ addr
            0x43 => JMP,        // JC addr
            0x44 => JMP,        // JNC addr
            0x45 => JMP,        // JN addr
            0x46 => JMP,        // JNN addr

            // Store instructions
            0x50 => STA,        // STA addr
            0x51 => 5,          // STDA addr - 5 cycles (16 bits)
            0x52 => 5,          // STDB addr - 5 cycles
            0x53 => STA,        // STB addr
            0x54 => STA,        // STC addr
            0x55 => STA,        // STD addr
            0x56 => STA,        // STE addr
            0x57 => STA,        // STF addr

            // Subroutines
            0x60 => CALL,       // CALL addr
            0x61 => RET,        // RET

            // Stack operations
            0x70 => PUSH,       // PUSH A - 2 cycles (opcode + push)
            0x71 => POP,        // POP A - 2 cycles (opcode + pop)
            0x72 => 3,          // PUSH16 DA - 3 cycles (push 16 bits)
            0x73 => 3,          // POP16 DA - 3 cycles (pop 16 bits)
            0x74 => PUSH,       // PUSH B
            0x75 => POP,        // POP B
            0x76 => 3,          // PUSH16 DB
            0x77 => 3,          // POP16 DB
            0x78 => PUSH,       // PUSH C
            0x79 => POP,        // POP C
            0x7A => PUSH,       // PUSH D
            0x7B => POP,        // POP D
            0x7C => PUSH,       // PUSH E
            0x7D => POP,        // POP E
            0x7E => PUSH,       // PUSH F
            0x7F => POP,        // POP F

            // Memory loads (0x90-0x95)
            0x90 => LDA_MEM,    // LDA addr - 4 cycles (opcode + addr + read)
            0x91 => LDB_MEM,    // LDB addr - 4 cycles
            0x92 => LDC_MEM,    // LDC addr - 4 cycles
            0x93 => LDD_MEM,    // LDD addr - 4 cycles
            0x94 => LDE_MEM,    // LDE addr - 4 cycles
            0x95 => LDF_MEM,    // LDF addr - 4 cycles

            // Register transfers (0xA0-0xB3)
            0xA0 => MOV_AB,     // MOV A,B - 1 cycle (internal registers)
            0xA1 => MOV_AC,     // MOV A,C - 1 cycle
            0xA2 => MOV_BA,     // MOV B,A - 1 cycle
            0xA3 => MOV_BC,     // MOV B,C - 1 cycle
            0xA4 => MOV_CA,     // MOV C,A - 1 cycle
            0xA5 => MOV_CB,     // MOV C,B - 1 cycle
            0xA6 => SWP_AB,     // SWP A,B - 2 cycles (requires temporary storage)
            0xA7 => SWP_AC,     // SWP A,C - 2 cycles
            0xA8 => 2,          // SWP A,D - 2 cycles
            0xA9 => 2,          // SWP A,E - 2 cycles
            0xAA => 2,          // SWP A,F - 2 cycles
            0xAB => 2,          // SWP DA,DB - 2 cycles (16-bit swap)
            0xAC => 1,          // MOV DA,DB - 1 cycle
            0xAD => 1,          // MOV DB,DA - 1 cycle
            0xAE => 2,          // SWP DA,IDX - 2 cycles
            0xAF => 2,          // SWP DA,IDY - 2 cycles
            0xB0 => 1,          // MOV DA,IDX - 1 cycle
            0xB1 => 1,          // MOV DA,IDY - 1 cycle
            0xB2 => 1,          // MOV IDX,DA - 1 cycle
            0xB3 => 1,          // MOV IDY,DA - 1 cycle

            // Relative jumps (0xC0-0xC3)
            0xC0 => JR,         // JR offset - 2 cycles (opcode + offset)
            0xC1 => JRZ,        // JRZ offset - 2 cycles (conditional relative jump)
            0xC2 => JRNZ,       // JRNZ offset - 2 cycles
            0xC3 => JRC,        // JRC offset - 2 cycles

            // Auto-increment/decrement array operations (0xC4-0xCB)
            0xC4 => 2,          // LDAIDX+ - 2 cycles (load + auto-increment)
            0xC5 => 2,          // LDAIDY+ - 2 cycles
            0xC6 => 2,          // STAIDX+ - 2 cycles (store + auto-increment)
            0xC7 => 2,          // STAIDY+ - 2 cycles
            0xC8 => 2,          // LDAIDX- - 2 cycles (load + auto-decrement)
            0xC9 => 2,          // LDAIDY- - 2 cycles
            0xCA => 2,          // STAIDX- - 2 cycles (store + auto-decrement)
            0xCB => 2,          // STAIDY- - 2 cycles

            // Compare instructions with immediate values (0xD0-0xD7) - NEW!
            0xD0 => CMP_IMM,    // CMP A,#imm - 2 cycles (opcode + immediate)
            0xD1 => CMP_IMM,    // CMP B,#imm - 2 cycles
            0xD2 => CMP_IMM,    // CMP C,#imm - 2 cycles
            0xD3 => CMP_IMM,    // CMP D,#imm - 2 cycles
            0xD4 => CMP_IMM,    // CMP E,#imm - 2 cycles
            0xD5 => CMP_IMM,    // CMP F,#imm - 2 cycles
            0xD6 => CMP16_IMM,  // CMP DA,#imm16 - 3 cycles (opcode + 16-bit immediate)
            0xD7 => CMP16_IMM,  // CMP DB,#imm16 - 3 cycles

            // Index register increment/decrement (0xE0-0xE3)
            0xE0 => 2,          // INCIDX - 2 cycles (16-bit increment)
            0xE1 => 2,          // DECIDX - 2 cycles (16-bit decrement)
            0xE2 => 2,          // INCIDY - 2 cycles
            0xE3 => 2,          // DECIDY - 2 cycles

            // Index register add immediate (0xE8, 0xEA)
            0xE8 => 4,          // ADDIDX #imm16 - 4 cycles (opcode + 16-bit immediate + add)
            0xEA => 4,          // ADDIDY #imm16 - 4 cycles

            // Index register transfer instructions (0xF5, 0xF6, 0xF9)
            0xF5 => 2,          // MVIDXIDY - 2 cycles (16-bit transfer)
            0xF6 => 2,          // MVIDYIDX - 2 cycles
            0xF9 => 3,          // SWPIDXIDY - 3 cycles (16-bit swap)

            // Extended instruction sets
            0xFE => 1,          // Extended instruction set 1
            0xFF => 1,          // Extended instruction set 2

            // Unknown instructions
            _ => 1              // Default - 1 cycle
        };
    }
}
