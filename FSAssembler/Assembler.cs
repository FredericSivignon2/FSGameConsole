using System.Text;
using FSAssembler.Core;

namespace FSAssembler;

/// <summary>
/// Main assembler that parses and compiles assembly code
/// </summary>
public class Assembler
{
    private readonly Dictionary<string, InstructionInfo> _instructions;
    private readonly Dictionary<string, ushort> _labels;
    private readonly List<(int position, string label)> _unresolvedReferences;

    public Assembler()
    {
        _instructions = Instuctions.GetAllInstructions();       
        _labels = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
        _unresolvedReferences = new List<(int, string)>();
    }

    public byte[] AssembleFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        string[] lines = File.ReadAllLines(filePath);
        return AssembleLines(lines);
    }

    public byte[] AssembleLines(string[] lines)
    {
        _labels.Clear();
        _unresolvedReferences.Clear();

        var machineCode = new List<byte>();
        ushort currentAddress = 0;

        // First pass: collect labels
        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            string line = PreprocessLine(lines[lineNumber]);
            if (string.IsNullOrEmpty(line)) continue;

            if (IsLabel(line))
            {
                string labelName = line.TrimEnd(':');
                if (_labels.ContainsKey(labelName))
                    throw new AssemblerException($"Line {lineNumber + 1}: Label '{labelName}' already defined");

                _labels[labelName] = currentAddress;
            }
            else
            {
                currentAddress += GetInstructionSize(line);
            }
        }

        // Second pass: generate machine code
        currentAddress = 0;
        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            string line = PreprocessLine(lines[lineNumber]);
            if (string.IsNullOrEmpty(line) || IsLabel(line)) continue;

            try
            {
                var instructionBytes = AssembleInstruction(line, currentAddress);
                machineCode.AddRange(instructionBytes);
                currentAddress += (ushort)instructionBytes.Count;
            }
            catch (Exception ex)
            {
                throw new AssemblerException($"Line {lineNumber + 1}", ex);
            }
        }

        // Resolve label references
        ResolveLabels(machineCode);
        return machineCode.ToArray();
    }

    private List<byte> AssembleInstruction(string line, ushort currentAddress)
    {
        var parts = SplitInstruction(line);
        string mnemonic = parts[0].ToUpper();
        var bytes = new List<byte>();

        if (mnemonic == "DB")
        {
            if (parts.Length == 1)
                throw new AssemblerException("DB directive requires at least one value");

            // Join all parts after "DB" to handle comma-separated values properly
            string datapart = string.Join(" ", parts.Skip(1));
            var values = ParseDataValues(datapart);
            bytes.AddRange(values);
            return bytes;
        }

        // Handle special instructions that require custom logic
        switch (mnemonic)
        {
            case "LDA":
            case "LDB":
            case "LDC":
            case "LDD":
            case "LDE":
            case "LDF":
                return AssembleLoadInstruction(mnemonic, parts, currentAddress);

            case "STA":
            case "STB":
            case "STC":
            case "STD":
            case "STE":
            case "STF":
                return AssembleStoreInstruction(mnemonic, parts, currentAddress);

            case "LDDA":
            case "LDDB":
                return AssembleLoadInstruction16(mnemonic, parts, currentAddress);

            case "STDA":
            case "STDB":
                return AssembleStoreInstruction(mnemonic, parts, currentAddress);

            case "LDIDX":
            case "LDIDY":
                return AssembleIndexLoadInstruction(mnemonic, parts, currentAddress);

            case "ADDIDX":
            case "ADDIDY":
                return AssembleIndexAddInstruction(mnemonic, parts, currentAddress);

            case "INC":
            case "DEC":
                return AssembleIncDecInstruction(mnemonic, parts);

            case "INC16":
            case "DEC16":
                return AssembleInc16Dec16Instruction(mnemonic, parts);

            case "CMP":
                return AssembleCmpInstruction(parts, line);

            case "AND":
            case "OR":
            case "XOR":
                return AssembleLogicalInstruction(mnemonic, parts, line);

            case "SHL":
                return AssembleShlInstruction(parts);

            case "JR":
            case "JRZ":
            case "JRNZ":
            case "JRC":
                return AssembleRelativeJumpInstruction(mnemonic, parts, currentAddress);

            case "PUSH":
            case "POP":
            case "PUSH16":
            case "POP16":
                return AssembleStackInstruction(mnemonic, parts);

            case "MOV":
                return AssembleMovInstruction(parts, line);

            case "SWP":
                return AssembleSwpInstruction(parts, line);
        }

        // Standard instruction handling
        if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
            throw new AssemblerException($"Unknown instruction: {mnemonic}");

        bytes.Add(instructionInfo.BaseOpcode);

        // Handle standard instructions with operands
        switch (mnemonic)
        {
            case "NOP":
            case "HALT":
            case "RET":
            case "SYS":
            case "ADD":
            case "SUB":
            case "ADD16":
            case "SUB16":
            case "NOT":
            case "SHR":
            case "DEBUG":
            case "EXT1":
            case "EXT2":
            // Index register operations that don't take parameters
            case "INCIDX":
            case "DECIDX":
            case "INCIDY":
            case "DECIDY":
            case "LDAIDX+" :
            case "LDAIDY+" :
            case "STAIDX+" :
            case "STAIDY+" :
            case "LDAIDX-" :
            case "LDAIDY-" :
            case "STAIDX-" :
            case "STAIDY-" :
            case "MVIDXIDY":
            case "MVIDYIDX":
            case "SWPIDXIDY":
                if (parts.Length > 1)
                    throw new AssemblerException($"Instruction {mnemonic} takes no parameters");
                break;

            case "JMP":
            case "JZ":
            case "JNZ":
            case "JC":
            case "JNC":
            case "JN":
            case "JNN":
            case "CALL":
                if (parts.Length != 2)
                    throw new AssemblerException($"Instruction {mnemonic} requires an address");

                ushort address = ParseAddress(parts[1], currentAddress);
                bytes.Add((byte)(address & 0xFF));
                bytes.Add((byte)((address >> 8) & 0xFF));
                break;
        }

        return bytes;
    }

    private List<byte> AssembleLoadInstruction(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires one operand");

        string operand = parts[1];

        if (operand.StartsWith('#'))
        {
            // Immediate load
            if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
                throw new AssemblerException($"Unknown load instruction: {mnemonic}");
            
            bytes.Add(instructionInfo.BaseOpcode);
            byte value = ParseSingleValue(operand.Substring(1));
            bytes.Add(value);
        }
        else if (operand.StartsWith("(") && operand.EndsWith(")"))
        {
            // Indexed addressing mode
            return AssembleIndexedLoadInstruction(mnemonic, operand, currentAddress);
        }
        else
        {
            // Memory load
            byte opcode = mnemonic switch
            {
                "LDA" => 0x90,
                "LDB" => 0x91,
                "LDC" => 0x92,
                "LDD" => 0x93,
                "LDE" => 0x94,
                "LDF" => 0x95,
                _ => throw new AssemblerException($"Invalid load instruction: {mnemonic}")
            };

            bytes.Add(opcode);
            ushort address = ParseAddress(operand, currentAddress);
            bytes.Add((byte)(address & 0xFF));
            bytes.Add((byte)((address >> 8) & 0xFF));
        }

        return bytes;
    }

    private List<byte> AssembleStoreInstruction(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires one operand");

        string operand = parts[1];

        if (operand.StartsWith("(") && operand.EndsWith(")"))
        {
            // Indexed addressing mode
            return AssembleIndexedStoreInstruction(mnemonic, operand, currentAddress);
        }
        else
        {
            // Memory store - use standard instruction handling
            if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
                throw new AssemblerException($"Unknown store instruction: {mnemonic}");

            bytes.Add(instructionInfo.BaseOpcode);
            ushort address = ParseAddress(operand, currentAddress);
            bytes.Add((byte)(address & 0xFF));
            bytes.Add((byte)((address >> 8) & 0xFF));
        }

        return bytes;
    }

    /// <summary>
    /// Assemble indexed load instructions like LDA (IDX), LDB (IDX+5), etc.
    /// </summary>
    private List<byte> AssembleIndexedLoadInstruction(string mnemonic, string indexedOperand, ushort currentAddress)
    {
        var bytes = new List<byte>();
        
        // Remove parentheses
        string inner = indexedOperand.Substring(1, indexedOperand.Length - 2).Trim();
        
        // Check if it has an offset (contains + or -)
        if (inner.Contains('+') || inner.Contains('-'))
        {
            // Indexed addressing with offset (0x90-0x99 range)
            var (register, offset) = ParseIndexedWithOffset(inner);
            
            byte opcode = (mnemonic.ToUpper(), register.ToUpper()) switch
            {
                ("LDA", "IDX") => 0x90,   // LDA (IDX+offset) - simplified register names
                ("LDB", "IDX") => 0x91,   // LDB (IDX+offset)
                ("LDA", "IDY") => 0x92,   // LDA (IDY+offset)  
                ("LDB", "IDY") => 0x93,   // LDB (IDY+offset)
                // Full register names
                ("LDA", "IDX1") => 0x90,  // LDA (IDX1+offset)
                ("LDB", "IDX1") => 0x91,  // LDB (IDX1+offset)
                ("LDA", "IDY1") => 0x92,  // LDA (IDY1+offset)
                ("LDB", "IDY1") => 0x93,  // LDB (IDY1+offset)
                ("LDA", "IDX2") => 0x98,  // LDA (IDX2+offset)
                ("LDA", "IDY2") => 0x99,  // LDA (IDY2+offset)
                _ => throw new AssemblerException($"Invalid indexed load instruction: {mnemonic} ({register}+offset)")
            };
            
            bytes.Add(opcode);
            bytes.Add((byte)offset);  // 8-bit signed offset
        }
        else
        {
            // Direct indexed addressing (0x86-0x8F range)
            byte opcode = (mnemonic.ToUpper(), inner.ToUpper()) switch
            {
                ("LDA", "IDX") => 0x86,   // LDA (IDX) - simplified register names
                ("LDB", "IDX") => 0x87,   // LDB (IDX)
                ("LDA", "IDY") => 0x88,   // LDA (IDY)
                ("LDB", "IDY") => 0x89,   // LDB (IDY)
                // Full register names
                ("LDA", "IDX1") => 0x86,  // LDA (IDX1)
                ("LDB", "IDX1") => 0x87,  // LDB (IDX1)
                ("LDA", "IDY1") => 0x88,  // LDA (IDY1)
                ("LDB", "IDY1") => 0x89,  // LDB (IDY1)
                ("LDA", "IDX2") => 0x8E,  // LDA (IDX2)
                ("LDA", "IDY2") => 0x8F,  // LDA (IDY2)
                _ => throw new AssemblerException($"Invalid indexed load instruction: {mnemonic} ({inner})")
            };
            
            bytes.Add(opcode);
        }
        
        return bytes;
    }

    /// <summary>
    /// Assemble indexed store instructions like STA (IDX), STB (IDX+5), etc.
    /// </summary>
    private List<byte> AssembleIndexedStoreInstruction(string mnemonic, string indexedOperand, ushort currentAddress)
    {
        var bytes = new List<byte>();
        
        // Remove parentheses
        string inner = indexedOperand.Substring(1, indexedOperand.Length - 2).Trim();
        
        // Check if it has an offset (contains + or -)
        if (inner.Contains('+') || inner.Contains('-'))
        {
            // Indexed addressing with offset (0x94-0x97 range)
            var (register, offset) = ParseIndexedWithOffset(inner);
            
            byte opcode = (mnemonic.ToUpper(), register.ToUpper()) switch
            {
                ("STA", "IDX") => 0x94,  // STA (IDX+offset) - using simplified register names
                ("STB", "IDX") => 0x95,  // STB (IDX+offset)
                ("STA", "IDY") => 0x96,  // STA (IDY+offset)
                ("STB", "IDY") => 0x97,  // STB (IDY+offset)
                _ => throw new AssemblerException($"Invalid indexed store instruction: {mnemonic} ({register}+offset)")
            };
            
            bytes.Add(opcode);
            bytes.Add((byte)offset);  // 8-bit signed offset
        }
        else
        {
            // Direct indexed addressing (0x8A-0x8D range)
            byte opcode = (mnemonic.ToUpper(), inner.ToUpper()) switch
            {
                ("STA", "IDX") => 0x8A,  // STA (IDX) - using simplified register names
                ("STB", "IDX") => 0x8B,  // STB (IDX)
                ("STA", "IDY") => 0x8C,  // STA (IDY)
                ("STB", "IDY") => 0x8D,  // STB (IDY)
                _ => throw new AssemblerException($"Invalid indexed store instruction: {mnemonic} ({inner})")
            };
            
            bytes.Add(opcode);
        }
        
        return bytes;
    }

    /// <summary>
    /// Parse an indexed expression with offset like "IDX+5" or "IDY-10"
    /// </summary>
    private (string register, sbyte offset) ParseIndexedWithOffset(string expression)
    {
        string register;
        sbyte offset;
        
        if (expression.Contains('+'))
        {
            var parts = expression.Split('+');
            if (parts.Length != 2)
                throw new AssemblerException($"Invalid indexed expression: {expression}");
                
            register = parts[0].Trim();
            
            if (!sbyte.TryParse(parts[1].Trim(), out offset))
                throw new AssemblerException($"Invalid offset in indexed expression: {parts[1]}");
        }
        else if (expression.Contains('-'))
        {
            var parts = expression.Split('-');
            if (parts.Length != 2)
                throw new AssemblerException($"Invalid indexed expression: {expression}");
                
            register = parts[0].Trim();
            
            // Parse the positive value and then make it negative
            if (!int.TryParse(parts[1].Trim(), out int positiveOffset))
                throw new AssemblerException($"Invalid offset in indexed expression: {parts[1]}");
                
            // Convert to negative and check range
            int negativeOffset = -positiveOffset;
            if (negativeOffset < -128 || negativeOffset > 127)
                throw new AssemblerException($"Indexed offset out of range: {negativeOffset} (must be -128 to +127)");
                
            offset = (sbyte)negativeOffset;
        }
        else
        {
            throw new AssemblerException($"Indexed expression must contain + or - operator: {expression}");
        }
        
        // Validate offset range for positive offsets (negative range already checked above)
        if (offset > 127)
            throw new AssemblerException($"Indexed offset out of range: {offset} (must be -128 to +127)");
            
        return (register, offset);
    }

    private List<byte> AssembleLoadInstruction16(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires one operand");

        string operand = parts[1];

        if (operand.StartsWith('#'))
        {
            // 16-bit immediate load
            if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
                throw new AssemblerException($"Unknown 16-bit load instruction: {mnemonic}");
            
            bytes.Add(instructionInfo.BaseOpcode);
            
            string valueStr = operand.Substring(1); // Remove '#'
            
            // Check if it's a numeric value or a label
            if (valueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                valueStr.StartsWith("$") ||
                (char.IsDigit(valueStr[0]) || (valueStr.StartsWith("-") && char.IsDigit(valueStr[1]))))
            {
                // Numeric immediate value
                ushort value = ParseValue16(valueStr);
                bytes.Add((byte)(value & 0xFF));
                bytes.Add((byte)((value >> 8) & 0xFF));
            }
            else
            {
                // Label immediate value - use the same mechanism as ParseAddress
                _unresolvedReferences.Add((currentAddress + 1, valueStr));
                bytes.Add(0x00); // Placeholder low byte
                bytes.Add(0x00); // Placeholder high byte
            }
        }
        else
        {
            // Memory load
            byte opcode = mnemonic switch
            {
                "LDDA" => 0x18,
                "LDDB" => 0x19,
                _ => throw new AssemblerException($"Invalid 16-bit load instruction: {mnemonic}")
            };

            bytes.Add(opcode);
            ushort address = ParseAddress(operand, currentAddress);
            bytes.Add((byte)(address & 0xFF));
            bytes.Add((byte)((address >> 8) & 0xFF));
        }

        return bytes;
    }

    private List<byte> AssembleIndexLoadInstruction(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires one operand");

        string operand = parts[1];

        if (operand.StartsWith('#'))
        {
            // 16-bit immediate load (like LDIDX #0x1234 or LDIDX #SOURCE_ARRAY)
            if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
                throw new AssemblerException($"Unknown index load instruction: {mnemonic}");

            bytes.Add(instructionInfo.BaseOpcode);

            string valueStr = operand.Substring(1); // Remove '#'
            
            // Check if it's a numeric value or a label
            if (valueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                valueStr.StartsWith("$") ||
                (char.IsDigit(valueStr[0]) || (valueStr.StartsWith("-") && char.IsDigit(valueStr[1]))))
            {
                // Numeric immediate value
                ushort value = ParseValue16(valueStr);
                bytes.Add((byte)(value & 0xFF));        // Low byte first (little-endian)
                bytes.Add((byte)((value >> 8) & 0xFF)); // High byte second
            }
            else
            {
                // Label immediate value - use the same mechanism as ParseAddress
                _unresolvedReferences.Add((currentAddress + 1, valueStr));
                bytes.Add(0x00); // Placeholder low byte
                bytes.Add(0x00); // Placeholder high byte
            }
        }
        else
        {
            // Address/label load (like LDIDX WelcomeMessage) - same as LDDA/LDDB behavior
            if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
                throw new AssemblerException($"Unknown index load instruction: {mnemonic}");

            bytes.Add(instructionInfo.BaseOpcode);

            ushort address = ParseAddress(operand, currentAddress);
            bytes.Add((byte)(address & 0xFF));      // Low byte first (little-endian)
            bytes.Add((byte)((address >> 8) & 0xFF)); // High byte second
        }

        return bytes;
    }

    private List<byte> AssembleIndexAddInstruction(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires one operand");

        string operand = parts[1];

        if (!operand.StartsWith('#'))
            throw new AssemblerException($"Index add instruction {mnemonic} requires immediate value (format: {mnemonic} #value)");

        // Index register add with 16-bit immediate value
        if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
            throw new AssemblerException($"Unknown index add instruction: {mnemonic}");

        bytes.Add(instructionInfo.BaseOpcode);
        
        string valueStr = operand.Substring(1); // Remove '#'
        
        // Check if it's a numeric value or a label
        if (valueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
            valueStr.StartsWith("$") ||
            (char.IsDigit(valueStr[0]) || (valueStr.StartsWith("-") && char.IsDigit(valueStr[1]))))
        {
            // Numeric immediate value
            ushort value = ParseValue16(valueStr);
            bytes.Add((byte)(value & 0xFF));        // Low byte first (little-endian)
            bytes.Add((byte)((value >> 8) & 0xFF)); // High byte second
        }
        else
        {
            // Label immediate value - use the same mechanism as ParseAddress
            _unresolvedReferences.Add((currentAddress + 1, valueStr));
            bytes.Add(0x00); // Placeholder low byte
            bytes.Add(0x00); // Placeholder high byte
        }

        return bytes;
    }

    private List<byte> AssembleIncDecInstruction(string mnemonic, string[] parts)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires a register");

        string register = parts[1].ToUpper();

        byte opcode = (mnemonic.ToUpper(), register) switch
        {
            ("INC", "A") => 0x28,
            ("DEC", "A") => 0x29,
            ("INC", "B") => 0x2A,
            ("DEC", "B") => 0x2B,
            ("INC", "C") => 0x2D,
            ("DEC", "C") => 0x2E,
            _ => throw new AssemblerException($"Invalid {mnemonic} register: {register}")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private List<byte> AssembleInc16Dec16Instruction(string mnemonic, string[] parts)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires a 16-bit register");

        string register = parts[1].ToUpper();

        byte opcode = (mnemonic.ToUpper(), register) switch
        {
            ("INC16", "DA") => 0x24,
            ("DEC16", "DA") => 0x25,
            ("INC16", "DB") => 0x26,
            ("DEC16", "DB") => 0x27,
            _ => throw new AssemblerException($"Invalid {mnemonic} register: {register}")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private List<byte> AssembleCmpInstruction(string[] parts, string originalLine)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException("CMP instruction requires either two registers (format: CMP A,B) or register with immediate (format: CMP A,#imm)");

        string operand = parts[1];

        // Check if it's immediate value comparison (CMP A,#imm format)
        if (operand.Contains(',') && operand.Contains('#'))
        {
            // Parse register and immediate value
            var commaParts = operand.Split(',');
            if (commaParts.Length != 2)
                throw new AssemblerException("CMP with immediate requires format: CMP register,#value");

            string register = commaParts[0].Trim().ToUpper();
            string immediateStr = commaParts[1].Trim();

            if (!immediateStr.StartsWith('#'))
                throw new AssemblerException("CMP immediate value must be prefixed with '#'");

            // Determine opcode based on register
            byte opcode = register switch
            {
                "A" => 0xD0,   // CMP A,#imm
                "B" => 0xD1,   // CMP B,#imm
                "C" => 0xD2,   // CMP C,#imm
                "D" => 0xD3,   // CMP D,#imm
                "E" => 0xD4,   // CMP E,#imm
                "F" => 0xD5,   // CMP F,#imm
                "DA" => 0xD6,  // CMP DA,#imm16
                "DB" => 0xD7,  // CMP DB,#imm16
                _ => throw new AssemblerException($"Invalid CMP register: {register}")
            };

            bytes.Add(opcode);

            // Parse immediate value
            string valueStr = immediateStr.Substring(1); // Remove '#'
            if (register == "DA" || register == "DB")
            {
                // 16-bit immediate value - check if it's numeric or a label
                if (valueStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                    valueStr.StartsWith("$") ||
                    (char.IsDigit(valueStr[0]) || (valueStr.StartsWith("-") && char.IsDigit(valueStr[1]))))
                {
                    // Numeric immediate value
                    ushort value = ParseValue16(valueStr);
                    bytes.Add((byte)(value & 0xFF));        // Low byte first (little-endian)
                    bytes.Add((byte)((value >> 8) & 0xFF)); // High byte second
                }
                else
                {
                    // Label immediate value - not typically used but theoretically possible
                    // Note: We need to calculate currentAddress for this instruction
                    // This is complex because we're deep in the assembly process
                    // For now, we'll throw an error as labels in CMP immediate are unusual
                    throw new AssemblerException($"Labels are not supported in CMP immediate values. Use numeric values only: {valueStr}");
                }
            }
            else
            {
                // 8-bit immediate value - labels not practical for 8-bit values
                byte value = ParseSingleValue(valueStr);
                bytes.Add(value);
            }

            return bytes;
        }
        else if (operand.Contains(','))
        {
            // Original register-to-register comparison (CMP A,B format)
            var registers = ParseTwoRegisters(operand, "CMP");

            string reg1 = registers.reg1;
            string reg2 = registers.reg2;

            byte opcode = (reg1, reg2) switch
            {
                ("A", "B") => 0x2C,
                ("A", "C") => 0x2F,
                _ => throw new AssemblerException($"Invalid CMP registers: {reg1},{reg2} (only A,B and A,C supported)")
            };

            bytes.Add(opcode);
            return bytes;
        }
        else
        {
            throw new AssemblerException("CMP instruction requires either two registers (CMP A,B) or register with immediate (CMP A,#imm)");
        }
    }

    private List<byte> AssembleLogicalInstruction(string mnemonic, string[] parts, string originalLine)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"{mnemonic} instruction requires two registers (format: {mnemonic} A,B)");

        // Parse registers from the operand part, handling spaces properly
        var registers = ParseTwoRegisters(parts[1], mnemonic);

        string reg1 = registers.reg1;
        string reg2 = registers.reg2;

        byte opcode = (mnemonic.ToUpper(), reg1, reg2) switch
        {
            ("AND", "A", "B") => 0x30,
            ("AND", "A", "C") => 0x36,
            ("OR", "A", "B") => 0x31,
            ("OR", "A", "C") => 0x37,
            ("XOR", "A", "B") => 0x32,
            ("XOR", "A", "C") => 0x38,
            _ => throw new AssemblerException($"Invalid {mnemonic} registers: {reg1},{reg2}")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private List<byte> AssembleShlInstruction(string[] parts)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException("SHL instruction requires a register");

        string register = parts[1].ToUpper();

        byte opcode = register switch
        {
            "A" => 0x34,
            "B" => 0x39,
            _ => throw new AssemblerException($"Invalid SHL register: {register} (only A and B supported)")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private List<byte> AssembleRelativeJumpInstruction(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"{mnemonic} instruction requires an offset");

        byte opcode = _instructions[mnemonic].BaseOpcode;
        bytes.Add(opcode);

        sbyte offset = ParseRelativeOffset(parts[1], currentAddress);
        bytes.Add((byte)offset);

        return bytes;
    }

    private List<byte> AssembleStackInstruction(string mnemonic, string[] parts)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"{mnemonic} instruction requires a register");

        string register = parts[1].ToUpper();

        byte opcode = (mnemonic.ToUpper(), register) switch
        {
            // Original stack instructions
            ("PUSH", "A") => 0x70,
            ("POP", "A") => 0x71,
            ("PUSH16", "DA") => 0x72,
            ("POP16", "DA") => 0x73,
            ("PUSH", "B") => 0x74,
            ("POP", "B") => 0x75,
            ("PUSH16", "DB") => 0x76,
            ("POP16", "DB") => 0x77,
            ("PUSH", "C") => 0x78,
            ("POP", "C") => 0x79,
            // NEW: Extended stack instructions for D, E, F registers
            ("PUSH", "D") => 0x7A,
            ("POP", "D") => 0x7B,
            ("PUSH", "E") => 0x7C,
            ("POP", "E") => 0x7D,
            ("PUSH", "F") => 0x7E,
            ("POP", "F") => 0x7F,
            _ => throw new AssemblerException($"Invalid {mnemonic} register: {register}")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private List<byte> AssembleMovInstruction(string[] parts, string originalLine)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException("MOV instruction requires two registers (format: MOV dst,src)");

        // Parse registers from the operand part, handling spaces properly
        var registers = ParseTwoRegisters(parts[1], "MOV");

        string dst = registers.reg1;
        string src = registers.reg2;

        byte opcode = (dst, src) switch
        {
            // Original 8-bit MOV instructions
            ("A", "B") => 0xA0,
            ("A", "C") => 0xA1,
            ("B", "A") => 0xA2,
            ("B", "C") => 0xA3,
            ("C", "A") => 0xA4,
            ("C", "B") => 0xA5,
            // NEW: 16-bit MOV instructions
            ("DA", "DB") => 0xAC,
            ("DB", "DA") => 0xAD,
            ("DA", "IDX") => 0xB0,
            ("DA", "IDY") => 0xB1,
            ("IDX", "DA") => 0xB2,
            ("IDY", "DA") => 0xB3,
            _ => throw new AssemblerException($"Invalid MOV registers: {dst},{src}")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private List<byte> AssembleSwpInstruction(string[] parts, string originalLine)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException("SWP instruction requires two registers (format: SWP A,B or SWP A,C)");

        // Parse registers from the operand part, handling spaces properly
        var registers = ParseTwoRegisters(parts[1], "SWP");

        string reg1 = registers.reg1;
        string reg2 = registers.reg2;

        byte opcode = (reg1, reg2) switch
        {
            // Original 8-bit SWP instructions
            ("A", "B") => 0xA6,
            ("A", "C") => 0xA7,
            // NEW: Extended SWP instructions for all 8-bit registers
            ("A", "D") => 0xA8,
            ("A", "E") => 0xA9,
            ("A", "F") => 0xAA,
            // NEW: 16-bit SWP instructions
            ("DA", "DB") => 0xAB,
            ("DA", "IDX") => 0xAE,
            ("DA", "IDY") => 0xAF,
            _ => throw new AssemblerException($"Invalid SWP registers: {reg1},{reg2}")
        };

        bytes.Add(opcode);
        return bytes;
    }

    private (string reg1, string reg2) ParseTwoRegisters(string operands, string instruction)
    {
        // Split by comma and handle spaces
        var parts = operands.Split(',');
        if (parts.Length != 2)
            throw new AssemblerException($"{instruction} instruction requires two registers");

        string reg1 = parts[0].Trim().ToUpper();
        string reg2 = parts[1].Trim().ToUpper();

        return (reg1, reg2);
    }

    private string[] SplitInstruction(string line)
    {
        var parts = new List<string>();
        var currentPart = new StringBuilder();
        bool inString = false;

        // First, handle quoted strings properly
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if ((c == '\'' || c == '"') && !inString)
            {
                inString = true;
                currentPart.Append(c);
            }
            else if ((c == '\'' || c == '"') && inString)
            {
                inString = false;
                currentPart.Append(c);
            }
            else if (!inString && (c == ' ' || c == '\t'))
            {
                if (currentPart.Length > 0)
                {
                    parts.Add(currentPart.ToString());
                    currentPart.Clear();
                }
            }
            else
            {
                currentPart.Append(c);
            }
        }

        if (currentPart.Length > 0)
            parts.Add(currentPart.ToString());

        // If we have exactly 2 parts and the second contains a comma,
        // we need to handle instructions like "CMP  A , B" properly
        if (parts.Count == 2 && parts[1].Contains(','))
        {
            // Keep as is, the ParseTwoRegisters will handle it
            return parts.ToArray();
        }

        // Handle case where spaces around comma caused extra splitting
        // like ["CMP", "A", ",", "B"] -> ["CMP", "A,B"]
        if (parts.Count > 2)
        {
            var mnemonic = parts[0];
            var operands = new StringBuilder();

            for (int i = 1; i < parts.Count; i++)
            {
                operands.Append(parts[i]);
            }

            return new[] { mnemonic, operands.ToString() };
        }

        return parts.ToArray();
    }

    private List<byte> ParseValue(string value)
    {
        value = value.Trim();
        var values = new List<byte>();

        // Handle string literals with double quotes
        if (value.StartsWith('"') && value.EndsWith('"') && value.Length >= 2)
        {
            string stringContent = value.Substring(1, value.Length - 2);
            foreach (char c in stringContent)
            {
                values.Add((byte)c);
            }
            // Add null terminator for strings
            values.Add(0);
            return values;
        }

        // Handle single character literals with single quotes
        if (value.StartsWith('\'') && value.EndsWith('\'') && value.Length == 3)
        {
            values.Add((byte)value[1]);
            return values;
        }

        // Handle hexadecimal values (0x prefix)
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                values.Add(Convert.ToByte(value.Substring(2), 16));
                return values;
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for an unsigned byte.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid hexadecimal format: {value}");
            }
        }

        // Handle hexadecimal values ($ prefix)
        if (value.StartsWith("$"))
        {
            try
            {
                values.Add(Convert.ToByte(value.Substring(1), 16));
                return values;
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for an unsigned byte.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid hexadecimal format: {value}");
            }
        }

        // Check if it's a decimal number - IMPORTANT: check this before label detection
        if (char.IsDigit(value[0]) || (value.StartsWith("-") && char.IsDigit(value[1])))
        {
            try
            {
                values.Add(Convert.ToByte(value));
                return values;
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for an unsigned byte.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid numeric format: {value}");
            }
        }

        // If it's not a numeric value, it could be a label - but this is unusual for 8-bit values
        // For DB directive, this is typically an error, but we should provide a helpful message
        throw new AssemblerException($"Invalid value format: {value}. Labels are not supported in 8-bit value context. Use numeric values (0-255), hex (0xFF or $FF), or character literals ('A').");
    }

    private ushort ParseValue16(string value)
    {
        value = value.Trim();

        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Convert.ToUInt16(value.Substring(2), 16);
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid hexadecimal format: {value}");
            }
        }

        if (value.StartsWith("$"))
        {
            try
            {
                return Convert.ToUInt16(value.Substring(1), 16);
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for a UInt16.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid hexadecimal format: {value}");
            }
        }

        // Check if it's a decimal number - IMPORTANT: check this before label detection
        if (char.IsDigit(value[0]) || (value.StartsWith("-") && char.IsDigit(value[1])))
        {
            return Convert.ToUInt16(value);
        }

        // If it's not a numeric value, it must be a label
        // Check if it looks like a valid label (not starting with special characters)
        if (!value.StartsWith("+") && !value.StartsWith("-"))
        {
            // Try to resolve the label
            if (_labels.TryGetValue(value, out ushort address))
            {
                return address;
            }
            else
            {
                throw new AssemblerException($"Undefined label: {value}");
            }
        }

        throw new AssemblerException($"Invalid numeric format: {value}");
    }

    private ushort ParseAddress(string address, ushort currentAddress)
    {
        // If address is something like (IDX-200), we need to handle it    

        address = address.Trim();

        // Handle indexed addressing expressions like (IDX+5) or (IDY-10)
        if (address.StartsWith("(") && address.EndsWith(")"))
        {
            return ParseIndexedAddressExpression(address);
        }

        // Check if it's a numeric value (hex or decimal)
        if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToUInt16(address.Substring(2), 16);
        }

        if (address.StartsWith("$"))
        {
            return Convert.ToUInt16(address.Substring(1), 16);
        }

        // Check if it's a decimal number - IMPORTANT: check this before label detection
        if (char.IsDigit(address[0]) || (address.StartsWith("-") && char.IsDigit(address[1])))
        {
            return Convert.ToUInt16(address);
        }

        // If it's not a numeric value, it must be a label
        // Add to unresolved references for later resolution
        _unresolvedReferences.Add((currentAddress + 1, address));
        return 0;
    }

    /// <summary>
    /// Parse indexed addressing expressions like (IDX+5), (IDY-10), (IDX2+0), etc.
    /// This method handles the evaluation of index register expressions with offsets.
    /// </summary>
    /// <param name="expression">The indexed expression including parentheses, e.g., "(IDX+5)"</param>
    /// <returns>The evaluated address or throws an exception if invalid</returns>
    private ushort ParseIndexedAddressExpression(string expression)
    {
        // Remove parentheses
        string inner = expression.Substring(1, expression.Length - 2).Trim();
        
        // Indexed expressions should now be handled by the specific instruction assemblers
        // (AssembleIndexedLoadInstruction, AssembleIndexedStoreInstruction)
        // If we reach this point, it means an indexed expression was used in a context
        // where it's not supported (like JMP (IDX+5) which doesn't exist)
        
        throw new AssemblerException($"Indexed addressing expression '{expression}' is not supported in this context. " +
                                   "Indexed addressing is only available for LDA, LDB, STA, STB instructions.");
    }

    private bool GetLabelReference(string label, out ushort address)
    {
        if (!char.IsDigit(label[0]) && !label.StartsWith("-") && !label.StartsWith("+") &&
            !label.StartsWith("0x") && !label.StartsWith("$"))
        {
            if (_labels.TryGetValue(label, out address))
            {
                return true;
            }
            else
            {
                throw new AssemblerException($"Undefined label: {label}");
            }
        }
        address = 0;
        return false;
    }

    private sbyte ParseRelativeOffset(string offsetStr, ushort currentAddress)
    {
        offsetStr = offsetStr.Trim();

        if (GetLabelReference(offsetStr, out ushort targetAddress))
        {
            int offset = targetAddress - (currentAddress + 2);
            if (offset < -128 || offset > 127)
                throw new AssemblerException($"Relative jump offset out of range: {offset} (must be -128 to +127)");
            return (sbyte)offset;
        }

        if (offsetStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return (sbyte)Convert.ToSByte(offsetStr.Substring(2), 16);
            }
            catch (OverflowException)
            {
                throw new AssemblerException("Relative jump offset out of range");
            }
        }

        if (offsetStr.StartsWith("$"))
        {
            try
            {
                return (sbyte)Convert.ToSByte(offsetStr.Substring(1), 16);
            }
            catch (OverflowException)
            {
                throw new AssemblerException("Relative jump offset out of range");
            }
        }

        try
        {
            return Convert.ToSByte(offsetStr);
        }
        catch (OverflowException)
        {
            throw new AssemblerException("Relative jump offset out of range");
        }
    }

    private void ResolveLabels(List<byte> machineCode)
    {
        foreach (var (position, labelName) in _unresolvedReferences)
        {
            if (!_labels.TryGetValue(labelName, out ushort address))
                throw new AssemblerException($"Undefined label: {labelName}");

            if (position + 1 >= machineCode.Count)
                throw new AssemblerException($"Invalid position for label {labelName}");

            machineCode[position] = (byte)(address & 0xFF);
            machineCode[position + 1] = (byte)((address >> 8) & 0xFF);
        }
    }

    private byte ParseSingleValue(string value)
    {
        value = value.Trim();

        // Handle single character literals with single quotes
        if (value.StartsWith('\'') && value.EndsWith('\'') && value.Length == 3)
        {
            return (byte)value[1];
        }

        // Handle hexadecimal values (0x prefix)
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return Convert.ToByte(value.Substring(2), 16);
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for an unsigned byte.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid hexadecimal format: {value}");
            }
        }

        // Handle hexadecimal values ($ prefix)
        if (value.StartsWith("$"))
        {
            try
            {
                return Convert.ToByte(value.Substring(1), 16);
            }
            catch (OverflowException)
            {
                throw new OverflowException("Value was either too large or too small for an unsigned byte.");
            }
            catch (FormatException)
            {
                throw new AssemblerException($"Invalid hexadecimal format: {value}");
            }
        }

        // Handle decimal values
        try
        {
            return Convert.ToByte(value);
        }
        catch (OverflowException)
        {
            throw new OverflowException("Value was either too large or too small for an unsigned byte.");
        }
        catch (FormatException)
        {
            throw new AssemblerException($"Invalid numeric format: {value}");
        }
    }

    private ushort GetInstructionSize(string line)
    {
        var parts = SplitInstruction(line);
        string mnemonic = parts[0].ToUpper();

        // Handle DB directive specially
        if (mnemonic == "DB")
        {
            // Count the number of values in DB directive
            if (parts.Length == 1) return 0;

            // Parse the data part to count values
            string datapart = string.Join(" ", parts.Skip(1));
            var values = ParseDataValues(datapart);
            return (ushort)values.Count;
        }

        // Look up instruction in our centralized dictionary
        if (!_instructions.TryGetValue(mnemonic, out InstructionInfo instructionInfo))
        {
            throw new AssemblerException($"Unknown instruction: {mnemonic}");
        }

        // Handle instructions with variable size based on operands
        if (instructionInfo.HasVariableSize)
        {
            return GetVariableInstructionSize(mnemonic, parts, instructionInfo);
        }

        // Handle instructions with special operand parsing (indexed addressing)
        if (parts.Length == 2)
        {
            string operand = parts[1];
            
            // Check for indexed addressing modes
            if (operand.StartsWith("(") && operand.EndsWith(")"))
            {
                string inner = operand.Substring(1, operand.Length - 2).Trim();
                
                // Indexed addressing with offset requires 2 bytes (opcode + offset)
                if (inner.Contains('+') || inner.Contains('-'))
                {
                    return 2;
                }
                // Direct indexed addressing requires 1 byte (opcode only)
                else
                {
                    return 1;
                }
            }
        }

        // Return the base size for instructions without variable sizing
        return instructionInfo.BaseSize;
    }

    /// <summary>
    /// Get the size of instructions that have variable size based on their operands
    /// </summary>
    private ushort GetVariableInstructionSize(string mnemonic, string[] parts, InstructionInfo instructionInfo)
    {
        switch (mnemonic)
        {
            // 8-bit load instructions - size depends on addressing mode
            case "LDA":
            case "LDB":
            case "LDC":
            case "LDD":
            case "LDE":
            case "LDF":
                if (parts.Length == 2 && parts[1].StartsWith('#'))
                    return 2; // Immediate: opcode + value
                else
                    return 3; // Memory: opcode + address (16-bit)

            // 16-bit load instructions - size depends on addressing mode
            case "LDDA":
            case "LDDB":
                if (parts.Length == 2 && parts[1].StartsWith('#'))
                    return 3; // Immediate: opcode + value (16-bit)
                else
                    return 3; // Memory: opcode + address (16-bit)

            // Compare instructions - size depends on format
            case "CMP":
                if (parts.Length == 2 && parts[1].Contains('#') && parts[1].Contains(','))
                {
                    // CMP register,#immediate format
                    return parts[1].ToUpper().Contains("DA") || parts[1].ToUpper().Contains("DB") ? (ushort)3 : (ushort)2;
                }
                else
                {
                    return 1; // Register-to-register comparison
                }

            default:
                // For other variable-size instructions, return base size
                return instructionInfo.BaseSize;
        }
    }

    private string PreprocessLine(string line)
    {
        int commentIndex = line.IndexOf(';');
        if (commentIndex >= 0)
            line = line.Substring(0, commentIndex);
        return line.Trim();
    }

    private bool IsLabel(string line)
    {
        return line.EndsWith(':');
    }

    private List<byte> ParseDataValues(string dataString)
    {
        var values = new List<byte>();
        var tokens = SplitDataString(dataString);

        foreach (var token in tokens)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                var tokenValues = ParseValue(token.Trim());
                values.AddRange(tokenValues);
            }
        }

        return values;
    }

    private List<string> SplitDataString(string dataString)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        bool inSingleQuotes = false;
        bool inDoubleQuotes = false;

        for (int i = 0; i < dataString.Length; i++)
        {
            char c = dataString[i];

            if (c == '\'' && !inDoubleQuotes)
            {
                inSingleQuotes = !inSingleQuotes;
                currentToken.Append(c);
            }
            else if (c == '"' && !inSingleQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
                currentToken.Append(c);
            }
            else if (c == ',' && !inSingleQuotes && !inDoubleQuotes)
            {
                // Add current token if it has content
                string tokenStr = currentToken.ToString().Trim();
                if (!string.IsNullOrEmpty(tokenStr))
                {
                    tokens.Add(tokenStr);
                }
                currentToken.Clear();
            }
            else if (!char.IsWhiteSpace(c) || inSingleQuotes || inDoubleQuotes)
            {
                currentToken.Append(c);
            }
            else if (char.IsWhiteSpace(c) && currentToken.Length > 0 && !inSingleQuotes && !inDoubleQuotes)
            {
                // Keep building the token, just ignore the whitespace
                // (we'll trim it later)
            }
        }

        // Add the last token if it has content
        string finalToken = currentToken.ToString().Trim();
        if (!string.IsNullOrEmpty(finalToken))
        {
            tokens.Add(finalToken);
        }

        return tokens;
    }
}
