namespace FSAssembler;

/// <summary>
/// Information about an assembly instruction including its opcode and characteristics
/// </summary>
public record InstructionInfo
{
    /// <summary>
    /// The base opcode for this instruction
    /// </summary>
    public byte BaseOpcode { get; init; }

    /// <summary>
    /// The base size of this instruction in bytes (including opcode)
    /// </summary>
    public ushort BaseSize { get; init; }

    /// <summary>
    /// Whether this instruction requires special handling (has complex logic)
    /// </summary>
    public bool RequiresSpecialHandling { get; init; }

    /// <summary>
    /// Whether this instruction has variable size based on operands
    /// </summary>
    public bool HasVariableSize { get; init; }

    /// <summary>
    /// Description for documentation purposes
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
