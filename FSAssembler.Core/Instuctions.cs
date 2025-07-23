using System;
using System.Collections.Generic;

namespace FSAssembler.Core
{
    public static class Instuctions
    {
        public static Dictionary<string, InstructionInfo> GetAllInstructions()
        {
            return new Dictionary<string, InstructionInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // === BASIC INSTRUCTIONS (0x00-0x01) ===
            { "NOP", new InstructionInfo { BaseOpcode = 0x00, BaseSize = 1, Description = "No Operation", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "HALT", new InstructionInfo { BaseOpcode = 0x01, BaseSize = 1, Description = "Stop processor", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === 8-BIT LOAD INSTRUCTIONS (0x10-0x15) ===
            { "LDA", new InstructionInfo { BaseOpcode = 0x10, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load A register", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction } },
            { "LDB", new InstructionInfo { BaseOpcode = 0x11, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load B register", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction } },
            { "LDC", new InstructionInfo { BaseOpcode = 0x12, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load C register", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction } },
            { "LDD", new InstructionInfo { BaseOpcode = 0x13, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load D register", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction } },
            { "LDE", new InstructionInfo { BaseOpcode = 0x14, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load E register", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction } },
            { "LDF", new InstructionInfo { BaseOpcode = 0x15, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load F register", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction } },

            // === 16-BIT LOAD INSTRUCTIONS (0x16-0x1B) ===
            { "LDDA", new InstructionInfo { BaseOpcode = 0x16, BaseSize = 3, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load DA register (16-bit)", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction16 } },
            { "LDDB", new InstructionInfo { BaseOpcode = 0x17, BaseSize = 3, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load DB register (16-bit)", AssemblyFunction = AssemblyFunctions.AssembleLoadInstruction16 } },
            { "LDIDX", new InstructionInfo { BaseOpcode = 0x1A, BaseSize = 3, RequiresSpecialHandling = true, Description = "Load IDX register (16-bit)", AssemblyFunction = AssemblyFunctions.AssembleIndexLoadInstruction } },
            { "LDIDY", new InstructionInfo { BaseOpcode = 0x1B, BaseSize = 3, RequiresSpecialHandling = true, Description = "Load IDY register (16-bit)", AssemblyFunction = AssemblyFunctions.AssembleIndexLoadInstruction } },

            // === ARITHMETIC INSTRUCTIONS (0x20-0x2F) ===
            { "ADD", new InstructionInfo { BaseOpcode = 0x20, BaseSize = 1, Description = "Add A,B", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "SUB", new InstructionInfo { BaseOpcode = 0x21, BaseSize = 1, Description = "Subtract A,B", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "ADD16", new InstructionInfo { BaseOpcode = 0x22, BaseSize = 1, Description = "Add DA,DB (16-bit)", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "SUB16", new InstructionInfo { BaseOpcode = 0x23, BaseSize = 1, Description = "Subtract DA,DB (16-bit)", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "INC16", new InstructionInfo { BaseOpcode = 0x24, BaseSize = 1, RequiresSpecialHandling = true, Description = "Increment 16-bit register", AssemblyFunction = AssemblyFunctions.AssembleInc16Dec16Instruction } },
            { "DEC16", new InstructionInfo { BaseOpcode = 0x25, BaseSize = 1, RequiresSpecialHandling = true, Description = "Decrement 16-bit register", AssemblyFunction = AssemblyFunctions.AssembleInc16Dec16Instruction } },
            { "INC", new InstructionInfo { BaseOpcode = 0x28, BaseSize = 1, RequiresSpecialHandling = true, Description = "Increment 8-bit register", AssemblyFunction = AssemblyFunctions.AssembleIncDecInstruction } },
            { "DEC", new InstructionInfo { BaseOpcode = 0x29, BaseSize = 1, RequiresSpecialHandling = true, Description = "Decrement 8-bit register", AssemblyFunction = AssemblyFunctions.AssembleIncDecInstruction } },
            { "CMP", new InstructionInfo { BaseOpcode = 0x2C, BaseSize = 1, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Compare registers or register with immediate", AssemblyFunction = AssemblyFunctions.AssembleCmpInstruction } },

            // === LOGICAL INSTRUCTIONS (0x30-0x39) ===
            { "AND", new InstructionInfo { BaseOpcode = 0x30, BaseSize = 1, RequiresSpecialHandling = true, Description = "Logical AND", AssemblyFunction = AssemblyFunctions.AssembleLogicalInstruction } },
            { "OR", new InstructionInfo { BaseOpcode = 0x31, BaseSize = 1, RequiresSpecialHandling = true, Description = "Logical OR", AssemblyFunction = AssemblyFunctions.AssembleLogicalInstruction } },
            { "XOR", new InstructionInfo { BaseOpcode = 0x32, BaseSize = 1, RequiresSpecialHandling = true, Description = "Logical XOR", AssemblyFunction = AssemblyFunctions.AssembleLogicalInstruction } },
            { "NOT", new InstructionInfo { BaseOpcode = 0x33, BaseSize = 1, Description = "Logical NOT A", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "SHL", new InstructionInfo { BaseOpcode = 0x34, BaseSize = 1, RequiresSpecialHandling = true, Description = "Shift Left", AssemblyFunction = AssemblyFunctions.AssembleShlInstruction } },
            { "SHR", new InstructionInfo { BaseOpcode = 0x35, BaseSize = 1, Description = "Shift Right A", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === JUMP INSTRUCTIONS (0x40-0x46) ===
            { "JMP", new InstructionInfo { BaseOpcode = 0x40, BaseSize = 3, Description = "Unconditional jump", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "JZ", new InstructionInfo { BaseOpcode = 0x41, BaseSize = 3, Description = "Jump if Zero", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "JNZ", new InstructionInfo { BaseOpcode = 0x42, BaseSize = 3, Description = "Jump if Not Zero", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "JC", new InstructionInfo { BaseOpcode = 0x43, BaseSize = 3, Description = "Jump if Carry", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "JNC", new InstructionInfo { BaseOpcode = 0x44, BaseSize = 3, Description = "Jump if No Carry", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "JN", new InstructionInfo { BaseOpcode = 0x45, BaseSize = 3, Description = "Jump if Negative", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "JNN", new InstructionInfo { BaseOpcode = 0x46, BaseSize = 3, Description = "Jump if Not Negative", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === STORE INSTRUCTIONS (0x50-0x57) ===
            { "STA", new InstructionInfo { BaseOpcode = 0x50, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store A at address", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STDA", new InstructionInfo { BaseOpcode = 0x51, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store DA at address (16-bit)", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STDB", new InstructionInfo { BaseOpcode = 0x52, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store DB at address (16-bit)", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STB", new InstructionInfo { BaseOpcode = 0x53, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store B at address", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STC", new InstructionInfo { BaseOpcode = 0x54, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store C at address", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STD", new InstructionInfo { BaseOpcode = 0x55, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store D at address", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STE", new InstructionInfo { BaseOpcode = 0x56, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store E at address", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },
            { "STF", new InstructionInfo { BaseOpcode = 0x57, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store F at address", AssemblyFunction = AssemblyFunctions.AssembleStoreInstruction } },

            // === SUBROUTINE INSTRUCTIONS (0x60-0x61) ===
            { "CALL", new InstructionInfo { BaseOpcode = 0x60, BaseSize = 3, Description = "Call subroutine", RequiredParameters = 2, HasAddressParameter = true, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "RET", new InstructionInfo { BaseOpcode = 0x61, BaseSize = 1, Description = "Return from subroutine", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === STACK INSTRUCTIONS (0x70-0x7F) ===
            { "PUSH", new InstructionInfo { BaseOpcode = 0x70, BaseSize = 1, RequiresSpecialHandling = true, Description = "Push register onto stack", AssemblyFunction = AssemblyFunctions.AssembleStackInstruction } },
            { "POP", new InstructionInfo { BaseOpcode = 0x71, BaseSize = 1, RequiresSpecialHandling = true, Description = "Pop from stack to register", AssemblyFunction = AssemblyFunctions.AssembleStackInstruction } },
            { "PUSH16", new InstructionInfo { BaseOpcode = 0x72, BaseSize = 1, RequiresSpecialHandling = true, Description = "Push 16-bit register onto stack", AssemblyFunction = AssemblyFunctions.AssembleStackInstruction } },
            { "POP16", new InstructionInfo { BaseOpcode = 0x73, BaseSize = 1, RequiresSpecialHandling = true, Description = "Pop from stack to 16-bit register", AssemblyFunction = AssemblyFunctions.AssembleStackInstruction } },

            // === I/O PORT INSTRUCTIONS (0x80-0x83) ===
            { "OUT", new InstructionInfo { BaseOpcode = 0x80, BaseSize = 2, Description = "Output to port", RequiredParameters = 2, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "IN", new InstructionInfo { BaseOpcode = 0x81, BaseSize = 2, Description = "Input from port", RequiredParameters = 2, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === REGISTER TRANSFER INSTRUCTIONS (0xA0-0xB3) ===
            { "MOV", new InstructionInfo { BaseOpcode = 0xA0, BaseSize = 1, RequiresSpecialHandling = true, Description = "Move between registers", AssemblyFunction = AssemblyFunctions.AssembleMovInstruction } },
            { "SWP", new InstructionInfo { BaseOpcode = 0xA6, BaseSize = 1, RequiresSpecialHandling = true, Description = "Swap registers", AssemblyFunction = AssemblyFunctions.AssembleSwpInstruction } },

            // === RELATIVE JUMP INSTRUCTIONS (0xC0-0xC3) ===
            { "JR", new InstructionInfo { BaseOpcode = 0xC0, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative", AssemblyFunction = AssemblyFunctions.AssembleRelativeJumpInstruction } },
            { "JRZ", new InstructionInfo { BaseOpcode = 0xC1, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative if Zero", AssemblyFunction = AssemblyFunctions.AssembleRelativeJumpInstruction } },
            { "JRNZ", new InstructionInfo { BaseOpcode = 0xC2, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative if Not Zero", AssemblyFunction = AssemblyFunctions.AssembleRelativeJumpInstruction } },
            { "JRC", new InstructionInfo { BaseOpcode = 0xC3, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative if Carry", AssemblyFunction = AssemblyFunctions.AssembleRelativeJumpInstruction } },

            // === AUTO-INCREMENT/DECREMENT INDEXED OPERATIONS (0xC4-0xCB) ===
            { "LDAIDX+", new InstructionInfo { BaseOpcode = 0xC4, BaseSize = 1, Description = "Load A from (IDX), then increment IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "LDAIDY+", new InstructionInfo { BaseOpcode = 0xC5, BaseSize = 1, Description = "Load A from (IDY), then increment IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "STAIDX+", new InstructionInfo { BaseOpcode = 0xC6, BaseSize = 1, Description = "Store A at (IDX), then increment IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "STAIDY+", new InstructionInfo { BaseOpcode = 0xC7, BaseSize = 1, Description = "Store A at (IDY), then increment IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "LDAIDX-", new InstructionInfo { BaseOpcode = 0xC8, BaseSize = 1, Description = "Load A from (IDX), then decrement IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "LDAIDY-", new InstructionInfo { BaseOpcode = 0xC9, BaseSize = 1, Description = "Load A from (IDY), then decrement IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "STAIDX-", new InstructionInfo { BaseOpcode = 0xCA, BaseSize = 1, Description = "Store A at (IDX), then decrement IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "STAIDY-", new InstructionInfo { BaseOpcode = 0xCB, BaseSize = 1, Description = "Store A at (IDY), then decrement IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === INDEX REGISTER INCREMENT/DECREMENT (0xE0-0xE3) ===
            { "INCIDX", new InstructionInfo { BaseOpcode = 0xE0, BaseSize = 1, Description = "Increment IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "DECIDX", new InstructionInfo { BaseOpcode = 0xE1, BaseSize = 1, Description = "Decrement IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "INCIDY", new InstructionInfo { BaseOpcode = 0xE2, BaseSize = 1, Description = "Increment IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "DECIDY", new InstructionInfo { BaseOpcode = 0xE3, BaseSize = 1, Description = "Decrement IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === INDEX REGISTER ADD IMMEDIATE (0xE8, 0xEA) ===
            { "ADDIDX", new InstructionInfo { BaseOpcode = 0xE8, BaseSize = 3, RequiresSpecialHandling = true, Description = "Add 16-bit immediate to IDX", AssemblyFunction = AssemblyFunctions.AssembleIndexAddInstruction } },
            { "ADDIDY", new InstructionInfo { BaseOpcode = 0xEA, BaseSize = 3, RequiresSpecialHandling = true, Description = "Add 16-bit immediate to IDY", AssemblyFunction = AssemblyFunctions.AssembleIndexAddInstruction } },

            // === SYSTEM CALL (0xF0) ===
            { "SYS", new InstructionInfo { BaseOpcode = 0xF0, BaseSize = 1, Description = "System call", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === INDEX REGISTER TRANSFER INSTRUCTIONS (0xF5, 0xF6, 0xF9) ===
            { "MVIDXIDY", new InstructionInfo { BaseOpcode = 0xF5, BaseSize = 1, Description = "Move IDX to IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "MVIDYIDX", new InstructionInfo { BaseOpcode = 0xF6, BaseSize = 1, Description = "Move IDY to IDX", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "SWPIDXIDY", new InstructionInfo { BaseOpcode = 0xF9, BaseSize = 1, Description = "Swap IDX and IDY", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === DEBUG INSTRUCTION (0xFD) ===
            { "DEBUG", new InstructionInfo { BaseOpcode = 0xFD, BaseSize = 1, Description = "Debug instruction", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === EXTENDED INSTRUCTION SETS (0xFE-0xFF) ===
            { "EXT1", new InstructionInfo { BaseOpcode = 0xFE, BaseSize = 1, Description = "Extended instruction set 1", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },
            { "EXT2", new InstructionInfo { BaseOpcode = 0xFF, BaseSize = 1, Description = "Extended instruction set 2", RequiredParameters = 1, AssemblyFunction = AssemblyFunctions.AssembleStandardInstruction } },

            // === SPECIAL DIRECTIVE ===
            { "DB", new InstructionInfo { BaseOpcode = 0x00, BaseSize = 0, RequiresSpecialHandling = true, Description = "Data bytes directive", AssemblyFunction = AssemblyFunctions.AssembleDbDirective } }
        };
        }
    }
}
