using System;
using System.Collections.Generic;

namespace FSAssembler.Core
{ 

/// <summary>
/// Delegate for instruction assembly function
/// </summary>
/// <param name="assembler">Reference to the assembler instance</param>
/// <param name="mnemonic">Instruction mnemonic</param>
/// <param name="parts">Instruction parts (mnemonic + operands)</param>
/// <param name="currentAddress">Current assembly address</param>
/// <returns>List of assembled bytes</returns>
public delegate List<byte> AssemblyFunction(object assembler, string mnemonic, string[] parts, ushort currentAddress);

/// <summary>
/// Information about an assembly instruction including its opcode and characteristics
/// </summary>
public class InstructionInfo
{
    /// <summary>
    /// The base opcode for this instruction
    /// </summary>
    public byte BaseOpcode { get; internal set; }

    /// <summary>
    /// The base size of this instruction in bytes (including opcode)
    /// </summary>
    public ushort BaseSize { get; internal set; }

    /// <summary>
    /// Whether this instruction requires special handling (has complex logic)
    /// </summary>
    public bool RequiresSpecialHandling { get; internal set; }

    /// <summary>
    /// Whether this instruction has variable size based on operands
    /// </summary>
    public bool HasVariableSize { get; internal set; }

    /// <summary>
    /// Description for documentation purposes
    /// </summary>
    public string Description { get; internal set; } = string.Empty;
    
    /// <summary>
    /// Assembly function for this instruction (if requires special handling)
    /// </summary>
    public AssemblyFunction AssemblyFunction { get; internal set; }
    
    /// <summary>
    /// Number of parameters required for this instruction (including mnemonic)
    /// 1 = no parameters (e.g., NOP, HALT)
    /// 2 = one parameter (e.g., JMP addr, LDA #imm)
    /// </summary>
    public int RequiredParameters { get; internal set; } = 1;
    
    /// <summary>
    /// Whether this instruction takes an address parameter that needs parsing
    /// </summary>
    public bool HasAddressParameter { get; internal set; }
}
}
