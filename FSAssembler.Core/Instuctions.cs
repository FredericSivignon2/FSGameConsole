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
            // Basic instructions (0x00-0x01) - CONFIRMED in CPU8Bit.cs
            { "NOP", new InstructionInfo { BaseOpcode = 0x00, BaseSize = 1, Description = "No Operation" } },
            { "HALT", new InstructionInfo { BaseOpcode = 0x01, BaseSize = 1, Description = "Stop processor" } },

            // 8-bit load instructions (0x10-0x15) - CONFIRMED in CPU8Bit.cs
            // Note: These have variable size based on operand format
            { "LDA", new InstructionInfo { BaseOpcode = 0x10, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load A register" } },
            { "LDB", new InstructionInfo { BaseOpcode = 0x11, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load B register" } },
            { "LDC", new InstructionInfo { BaseOpcode = 0x12, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load C register" } },
            { "LDD", new InstructionInfo { BaseOpcode = 0x13, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load D register" } },
            { "LDE", new InstructionInfo { BaseOpcode = 0x14, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load E register" } },
            { "LDF", new InstructionInfo { BaseOpcode = 0x15, BaseSize = 2, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load F register" } },

            // 16-bit load instructions (0x16-0x1B) - CONFIRMED in CPU8Bit.cs
            { "LDDA", new InstructionInfo { BaseOpcode = 0x16, BaseSize = 3, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load DA register (16-bit)" } },
            { "LDDB", new InstructionInfo { BaseOpcode = 0x17, BaseSize = 3, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Load DB register (16-bit)" } },
            { "LDIDX", new InstructionInfo { BaseOpcode = 0x1A, BaseSize = 3, RequiresSpecialHandling = true, Description = "Load IDX register (16-bit)" } },
            { "LDIDY", new InstructionInfo { BaseOpcode = 0x1B, BaseSize = 3, RequiresSpecialHandling = true, Description = "Load IDY register (16-bit)" } },

            // Arithmetic instructions (0x20-0x2F) - CONFIRMED in CPU8Bit.cs
            { "ADD", new InstructionInfo { BaseOpcode = 0x20, BaseSize = 1, Description = "Add A,B" } },
            { "SUB", new InstructionInfo { BaseOpcode = 0x21, BaseSize = 1, Description = "Subtract A,B" } },
            { "ADD16", new InstructionInfo { BaseOpcode = 0x22, BaseSize = 1, Description = "Add DA,DB (16-bit)" } },
            { "SUB16", new InstructionInfo { BaseOpcode = 0x23, BaseSize = 1, Description = "Subtract DA,DB (16-bit)" } },
            { "INC16", new InstructionInfo { BaseOpcode = 0x24, BaseSize = 1, RequiresSpecialHandling = true, Description = "Increment 16-bit register" } },
            { "DEC16", new InstructionInfo { BaseOpcode = 0x25, BaseSize = 1, RequiresSpecialHandling = true, Description = "Decrement 16-bit register" } },
            { "INC", new InstructionInfo { BaseOpcode = 0x28, BaseSize = 1, RequiresSpecialHandling = true, Description = "Increment 8-bit register" } },
            { "DEC", new InstructionInfo { BaseOpcode = 0x29, BaseSize = 1, RequiresSpecialHandling = true, Description = "Decrement 8-bit register" } },
            { "CMP", new InstructionInfo { BaseOpcode = 0x2C, BaseSize = 1, HasVariableSize = true, RequiresSpecialHandling = true, Description = "Compare registers or register with immediate" } },

            // Logical instructions (0x30-0x39) - CONFIRMED in CPU8Bit.cs
            { "AND", new InstructionInfo { BaseOpcode = 0x30, BaseSize = 1, RequiresSpecialHandling = true, Description = "Logical AND" } },
            { "OR", new InstructionInfo { BaseOpcode = 0x31, BaseSize = 1, RequiresSpecialHandling = true, Description = "Logical OR" } },
            { "XOR", new InstructionInfo { BaseOpcode = 0x32, BaseSize = 1, RequiresSpecialHandling = true, Description = "Logical XOR" } },
            { "NOT", new InstructionInfo { BaseOpcode = 0x33, BaseSize = 1, Description = "Logical NOT A" } },
            { "SHL", new InstructionInfo { BaseOpcode = 0x34, BaseSize = 1, RequiresSpecialHandling = true, Description = "Shift Left" } },
            { "SHR", new InstructionInfo { BaseOpcode = 0x35, BaseSize = 1, Description = "Shift Right A" } },

            // Jump instructions (0x40-0x46) - CONFIRMED in CPU8Bit.cs
            { "JMP", new InstructionInfo { BaseOpcode = 0x40, BaseSize = 3, Description = "Unconditional jump" } },
            { "JZ", new InstructionInfo { BaseOpcode = 0x41, BaseSize = 3, Description = "Jump if Zero" } },
            { "JNZ", new InstructionInfo { BaseOpcode = 0x42, BaseSize = 3, Description = "Jump if Not Zero" } },
            { "JC", new InstructionInfo { BaseOpcode = 0x43, BaseSize = 3, Description = "Jump if Carry" } },
            { "JNC", new InstructionInfo { BaseOpcode = 0x44, BaseSize = 3, Description = "Jump if No Carry" } },
            { "JN", new InstructionInfo { BaseOpcode = 0x45, BaseSize = 3, Description = "Jump if Negative" } },
            { "JNN", new InstructionInfo { BaseOpcode = 0x46, BaseSize = 3, Description = "Jump if Not Negative" } },

            // Store instructions (0x50-0x57) - CONFIRMED in CPU8Bit.cs
            { "STA", new InstructionInfo { BaseOpcode = 0x50, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store A at address" } },
            { "STDA", new InstructionInfo { BaseOpcode = 0x51, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store DA at address (16-bit)" } },
            { "STDB", new InstructionInfo { BaseOpcode = 0x52, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store DB at address (16-bit)" } },
            { "STB", new InstructionInfo { BaseOpcode = 0x53, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store B at address" } },
            { "STC", new InstructionInfo { BaseOpcode = 0x54, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store C at address" } },
            { "STD", new InstructionInfo { BaseOpcode = 0x55, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store D at address" } },
            { "STE", new InstructionInfo { BaseOpcode = 0x56, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store E at address" } },
            { "STF", new InstructionInfo { BaseOpcode = 0x57, BaseSize = 3, RequiresSpecialHandling = true, Description = "Store F at address" } },

            // Subroutine instructions (0x60-0x61) - CONFIRMED in CPU8Bit.cs
            { "CALL", new InstructionInfo { BaseOpcode = 0x60, BaseSize = 3, Description = "Call subroutine" } },
            { "RET", new InstructionInfo { BaseOpcode = 0x61, BaseSize = 1, Description = "Return from subroutine" } },

            // Stack instructions (0x70-0x7F) - CONFIRMED in CPU8Bit.cs
            { "PUSH", new InstructionInfo { BaseOpcode = 0x70, BaseSize = 1, RequiresSpecialHandling = true, Description = "Push register onto stack" } },
            { "POP", new InstructionInfo { BaseOpcode = 0x71, BaseSize = 1, RequiresSpecialHandling = true, Description = "Pop from stack to register" } },
            { "PUSH16", new InstructionInfo { BaseOpcode = 0x72, BaseSize = 1, RequiresSpecialHandling = true, Description = "Push 16-bit register onto stack" } },
            { "POP16", new InstructionInfo { BaseOpcode = 0x73, BaseSize = 1, RequiresSpecialHandling = true, Description = "Pop from stack to 16-bit register" } },

            // I/O Port Instructions (0x80-0x83) - CONFIRMED in CPU8Bit.cs
            { "OUT", new InstructionInfo { BaseOpcode = 0x80, BaseSize = 2, Description = "Output to port" } },
            { "IN", new InstructionInfo { BaseOpcode = 0x81, BaseSize = 2, Description = "Input from port" } },

            // Register transfer instructions (0xA0-0xB3) - CONFIRMED in CPU8Bit.cs
            { "MOV", new InstructionInfo { BaseOpcode = 0xA0, BaseSize = 1, RequiresSpecialHandling = true, Description = "Move between registers" } },
            { "SWP", new InstructionInfo { BaseOpcode = 0xA6, BaseSize = 1, RequiresSpecialHandling = true, Description = "Swap registers" } },

            // Relative jump instructions (0xC0-0xC3) - CONFIRMED in CPU8Bit.cs
            { "JR", new InstructionInfo { BaseOpcode = 0xC0, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative" } },
            { "JRZ", new InstructionInfo { BaseOpcode = 0xC1, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative if Zero" } },
            { "JRNZ", new InstructionInfo { BaseOpcode = 0xC2, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative if Not Zero" } },
            { "JRC", new InstructionInfo { BaseOpcode = 0xC3, BaseSize = 2, RequiresSpecialHandling = true, Description = "Jump relative if Carry" } },

            // Auto-increment/decrement indexed operations (0xC4-0xCB) - CONFIRMED in CPU8Bit.cs
            { "LDAIDX+", new InstructionInfo { BaseOpcode = 0xC4, BaseSize = 1, Description = "Load A from (IDX), then increment IDX" } },
            { "LDAIDY+", new InstructionInfo { BaseOpcode = 0xC5, BaseSize = 1, Description = "Load A from (IDY), then increment IDY" } },
            { "STAIDX+", new InstructionInfo { BaseOpcode = 0xC6, BaseSize = 1, Description = "Store A at (IDX), then increment IDX" } },
            { "STAIDY+", new InstructionInfo { BaseOpcode = 0xC7, BaseSize = 1, Description = "Store A at (IDY), then increment IDY" } },
            { "LDAIDX-", new InstructionInfo { BaseOpcode = 0xC8, BaseSize = 1, Description = "Load A from (IDX), then decrement IDX" } },
            { "LDAIDY-", new InstructionInfo { BaseOpcode = 0xC9, BaseSize = 1, Description = "Load A from (IDY), then decrement IDY" } },
            { "STAIDX-", new InstructionInfo { BaseOpcode = 0xCA, BaseSize = 1, Description = "Store A at (IDX), then decrement IDX" } },
            { "STAIDY-", new InstructionInfo { BaseOpcode = 0xCB, BaseSize = 1, Description = "Store A at (IDY), then decrement IDY" } },

            // Index register increment/decrement (0xE0-0xE3) - CONFIRMED in CPU8Bit.cs
            { "INCIDX", new InstructionInfo { BaseOpcode = 0xE0, BaseSize = 1, Description = "Increment IDX" } },
            { "DECIDX", new InstructionInfo { BaseOpcode = 0xE1, BaseSize = 1, Description = "Decrement IDX" } },
            { "INCIDY", new InstructionInfo { BaseOpcode = 0xE2, BaseSize = 1, Description = "Increment IDY" } },
            { "DECIDY", new InstructionInfo { BaseOpcode = 0xE3, BaseSize = 1, Description = "Decrement IDY" } },

            // Index register add immediate (0xE8, 0xEA) - CONFIRMED in CPU8Bit.cs
            { "ADDIDX", new InstructionInfo { BaseOpcode = 0xE8, BaseSize = 3, RequiresSpecialHandling = true, Description = "Add 16-bit immediate to IDX" } },
            { "ADDIDY", new InstructionInfo { BaseOpcode = 0xEA, BaseSize = 3, RequiresSpecialHandling = true, Description = "Add 16-bit immediate to IDY" } },

            // System call (0xF0) - CONFIRMED in CPU8Bit.cs
            { "SYS", new InstructionInfo { BaseOpcode = 0xF0, BaseSize = 1, Description = "System call" } },

            // Index register transfer instructions (0xF5, 0xF6, 0xF9) - CONFIRMED in CPU8Bit.cs
            { "MVIDXIDY", new InstructionInfo { BaseOpcode = 0xF5, BaseSize = 1, Description = "Move IDX to IDY" } },
            { "MVIDYIDX", new InstructionInfo { BaseOpcode = 0xF6, BaseSize = 1, Description = "Move IDY to IDX" } },
            { "SWPIDXIDY", new InstructionInfo { BaseOpcode = 0xF9, BaseSize = 1, Description = "Swap IDX and IDY" } },

            // Debug instruction (0xFD) - CONFIRMED in CPU8Bit.cs
            { "DEBUG", new InstructionInfo { BaseOpcode = 0xFD, BaseSize = 1, Description = "Debug instruction" } },

            // Extended instruction sets (0xFE-0xFF) - CONFIRMED in CPU8Bit.cs
            { "EXT1", new InstructionInfo { BaseOpcode = 0xFE, BaseSize = 1, Description = "Extended instruction set 1" } },
            { "EXT2", new InstructionInfo { BaseOpcode = 0xFF, BaseSize = 1, Description = "Extended instruction set 2" } }
        };
        }
    }
}
