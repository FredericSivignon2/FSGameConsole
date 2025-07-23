namespace FSAssembler.Core
{ 

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
}
}
