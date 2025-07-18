using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSAssembler;

/// <summary>
/// Assembleur principal qui parse et compile le code assembleur
/// </summary>
public class Assembler
{
    private readonly Dictionary<string, byte> _mnemonics;
    private readonly Dictionary<string, ushort> _labels;
    private readonly List<(int position, string label)> _unresolvedReferences;

    public Assembler()
    {
        _mnemonics = InitializeMnemonics();
        _labels = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase);
        _unresolvedReferences = new List<(int, string)>();
    }

    private Dictionary<string, byte> InitializeMnemonics()
    {
        return new Dictionary<string, byte>(StringComparer.OrdinalIgnoreCase)
    {
        // Basic instructions
        { "NOP", 0x00 }, { "HALT", 0x01 },
        
        // 8-bit load instructions
        { "LDA", 0x10 }, { "LDB", 0x11 }, { "LDC", 0x12 },
        { "LDD", 0x13 }, { "LDE", 0x14 }, { "LDF", 0x15 },
        
        // 16-bit load instructions
        { "LDDA", 0x16 }, { "LDDB", 0x17 },
        
        // Index register load instructions (16-bit immediate)
        { "LDIX1", 0x1A }, { "LDIX2", 0x1B },
        { "LDIY1", 0x1C }, { "LDIY2", 0x1D },
        
        // Arithmetic instructions
        { "ADD", 0x20 }, { "SUB", 0x21 }, { "ADD16", 0x22 }, { "SUB16", 0x23 },
        { "INC16", 0x24 }, { "DEC16", 0x25 }, { "INC", 0x28 }, { "DEC", 0x29 },
        { "CMP", 0x2C },
        
        // Logical instructions
        { "AND", 0x30 }, { "OR", 0x31 }, { "XOR", 0x32 }, { "NOT", 0x33 },
        { "SHL", 0x34 }, { "SHR", 0x35 },
        
        // Jump instructions
        { "JMP", 0x40 }, { "JZ", 0x41 }, { "JNZ", 0x42 },
        { "JC", 0x43 }, { "JNC", 0x44 }, { "JN", 0x45 }, { "JNN", 0x46 },
        
        // Store instructions
        { "STA", 0x50 }, { "STDA", 0x51 }, { "STDB", 0x52 },
        { "STB", 0x53 }, { "STC", 0x54 }, { "STD", 0x55 },
        
        // Subroutine instructions
        { "CALL", 0x60 }, { "RET", 0x61 },
        
        // Stack instructions
        { "PUSH", 0x70 }, { "POP", 0x71 },
        
        // Register transfer instructions
        { "MOV", 0xA0 }, { "SWP", 0xA6 },
        
        // Relative jump instructions
        { "JR", 0xC0 }, { "JRZ", 0xC1 }, { "JRNZ", 0xC2 }, { "JRC", 0xC3 },
        
        // Auto-increment/decrement indexed operations
        { "LDAIX1+", 0xC4 }, { "LDAIY1+", 0xC5 }, { "STAIX1+", 0xC6 }, { "STAIY1+", 0xC7 },
        { "LDAIX1-", 0xC8 }, { "LDAIY1-", 0xC9 }, { "STAIX1-", 0xCA }, { "STAIY1-", 0xCB },
        
        // Index register increment/decrement
        { "INCIX1", 0xE0 }, { "DECIX1", 0xE1 }, { "INCIY1", 0xE2 }, { "DECIY1", 0xE3 },
        { "INCIX2", 0xE4 }, { "DECIX2", 0xE5 }, { "INCIY2", 0xE6 }, { "DECIY2", 0xE7 },
        
        // Index register add immediate
        { "ADDIX1", 0xE8 }, { "ADDIX2", 0xE9 }, { "ADDIY1", 0xEA }, { "ADDIY2", 0xEB },
        
        // System call
        { "SYS", 0xF0 },
        
        // Index register transfer instructions
        { "MVIX1IX2", 0xF1 }, { "MVIX2IX1", 0xF2 }, { "MVIY1IY2", 0xF3 }, { "MVIY2IY1", 0xF4 },
        { "MVIX1IY1", 0xF5 }, { "MVIY1IX1", 0xF6 },
        
        // Index register swap instructions
        { "SWPIX1IX2", 0xF7 }, { "SWPIY1IY2", 0xF8 }, { "SWPIX1IY1", 0xF9 }
    };
    }

    public byte[] AssembleFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Fichier non trouvé: {filePath}");

        string[] lines = File.ReadAllLines(filePath);
        return AssembleLines(lines);
    }

    public byte[] AssembleLines(string[] lines)
    {
        _labels.Clear();
        _unresolvedReferences.Clear();

        var machineCode = new List<byte>();
        ushort currentAddress = 0;

        // Premier passage: collecter les labels
        for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
        {
            string line = PreprocessLine(lines[lineNumber]);
            if (string.IsNullOrEmpty(line)) continue;

            if (IsLabel(line))
            {
                string labelName = line.TrimEnd(':');
                if (_labels.ContainsKey(labelName))
                    throw new AssemblerException($"Ligne {lineNumber + 1}: Label '{labelName}' déjà défini");

                _labels[labelName] = currentAddress;
            }
            else
            {
                currentAddress += GetInstructionSize(line);
            }
        }

        // Deuxième passage: générer le code machine
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
                throw new AssemblerException($"Ligne {lineNumber + 1}", ex);
            }
        }

        // Résoudre les références de labels
        ResolveLabels(machineCode);
        return machineCode.ToArray();
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

    private ushort GetInstructionSize(string line)
    {
        var parts = SplitInstruction(line);
        string mnemonic = parts[0].ToUpper();

        if (mnemonic == "DB")
        {
            // Count the number of values in DB directive
            if (parts.Length == 1) return 0;

            // Parse the data part to count values
            string datapart = string.Join(" ", parts.Skip(1));
            var values = ParseDataValues(datapart);
            return (ushort)values.Count;
        }

        // Handle instructions with special operand parsing
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

        return mnemonic switch
        {
            // Basic instructions (1 byte)
            "NOP" or "HALT" or "RET" or "SYS" or "ADD" or "SUB" or "ADD16" or "SUB16" => 1,

            // 8-bit load instructions
            "LDA" or "LDB" or "LDC" or "LDD" or "LDE" or "LDF" when parts.Length == 2 && parts[1].StartsWith('#') => 2,
            "LDA" or "LDB" or "LDC" or "LDD" or "LDE" or "LDF" => 3,

            // 16-bit load instructions
            "LDDA" or "LDDB" when parts.Length == 2 && parts[1].StartsWith('#') => 3,
            "LDDA" or "LDDB" => 3,

            // Index register load instructions (16-bit immediate) - always 3 bytes
            "LDIX1" or "LDIX2" or "LDIY1" or "LDIY2" => 3,

            // Increment/Decrement operations
            "INC" or "DEC" or "INC16" or "DEC16" or "CMP" => 1,

            // Index register arithmetic operations (1 byte)
            "INCIX1" or "DECIX1" or "INCIY1" or "DECIY1" or
            "INCIX2" or "DECIX2" or "INCIY2" or "DECIY2" => 1,

            // Index register add immediate (3 bytes)
            "ADDIX1" or "ADDIX2" or "ADDIY1" or "ADDIY2" => 3,

            // Auto-increment/decrement indexed operations (1 byte)
            "LDAIX1+" or "LDAIY1+" or "STAIX1+" or "STAIY1+" or
            "LDAIX1-" or "LDAIY1-" or "STAIX1-" or "STAIY1-" => 1,

            // Index register transfer and swap operations (1 byte)
            "MVIX1IX2" or "MVIX2IX1" or "MVIY1IY2" or "MVIY2IY1" or
            "MVIX1IY1" or "MVIY1IX1" or "SWPIX1IX2" or "SWPIY1IY2" or "SWPIX1IY1" => 1,

            // Logic operations
            "AND" or "OR" or "XOR" or "NOT" or "SHL" or "SHR" => 1,

            // Jump instructions
            "JMP" or "JZ" or "JNZ" or "JC" or "JNC" or "JN" or "JNN" => 3,
            "JR" or "JRZ" or "JRNZ" or "JRC" => 2,

            // Store and call instructions
            "STA" or "STB" or "STC" or "STD" or "STDA" or "STDB" or "CALL" => 3,

            // Stack and other single-byte instructions
            "PUSH" or "POP" or "MOV" or "SWP" => 1,

            _ => throw new AssemblerException($"Unknown instruction: {mnemonic}")
        };
    }

    private List<byte> ParseDataValues(string dataString)
    {
        var values = new List<byte>();
        var tokens = SplitDataString(dataString);

        foreach (var token in tokens)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                values.Add(ParseValue(token.Trim()));
            }
        }

        return values;
    }

    private List<string> SplitDataString(string dataString)
    {
        var tokens = new List<string>();
        var currentToken = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < dataString.Length; i++)
        {
            char c = dataString[i];

            if (c == '\'' && !inQuotes)
            {
                inQuotes = true;
                currentToken.Append(c);
            }
            else if (c == '\'' && inQuotes)
            {
                inQuotes = false;
                currentToken.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                // Add current token if it has content
                string tokenStr = currentToken.ToString().Trim();
                if (!string.IsNullOrEmpty(tokenStr))
                {
                    tokens.Add(tokenStr);
                }
                currentToken.Clear();
            }
            else if (!char.IsWhiteSpace(c) || inQuotes)
            {
                currentToken.Append(c);
            }
            else if (char.IsWhiteSpace(c) && currentToken.Length > 0 && !inQuotes)
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
                return AssembleStoreInstruction(mnemonic, parts, currentAddress);

            case "LDDA":
            case "LDDB":
                return AssembleLdda_LddbInstruction(mnemonic, parts, currentAddress);

            case "LDIX1":
            case "LDIX2":
            case "LDIY1":
            case "LDIY2":
                return AssembleIndexLoadInstruction(mnemonic, parts, currentAddress);

            case "ADDIX1":
            case "ADDIX2":
            case "ADDIY1":
            case "ADDIY2":
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
                return AssembleStackInstruction(mnemonic, parts);

            case "MOV":
                return AssembleMovInstruction(parts, line);

            case "SWP":
                return AssembleSwpInstruction(parts, line);
        }

        // Standard instruction handling
        if (!_mnemonics.TryGetValue(mnemonic, out byte opcode))
            throw new AssemblerException($"Unknown instruction: {mnemonic}");

        bytes.Add(opcode);

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
            // Index register operations that don't take parameters
            case "INCIX1":
            case "DECIX1":
            case "INCIY1":
            case "DECIY1":
            case "INCIX2":
            case "DECIX2":
            case "INCIY2":
            case "DECIY2":
            case "LDAIX1+":
            case "LDAIY1+":
            case "STAIX1+":
            case "STAIY1+":
            case "LDAIX1-":
            case "LDAIY1-":
            case "STAIX1-":
            case "STAIY1-":
            case "MVIX1IX2":
            case "MVIX2IX1":
            case "MVIY1IY2":
            case "MVIY2IY1":
            case "MVIX1IY1":
            case "MVIY1IX1":
            case "SWPIX1IX2":
            case "SWPIY1IY2":
            case "SWPIX1IY1":
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
            case "STA":
            case "STB":
            case "STC":
            case "STD":
            case "STDA":
            case "STDB":
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
            byte opcode = _mnemonics[mnemonic];
            bytes.Add(opcode);
            byte value = ParseValue(operand.Substring(1));
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
                "LDA" => 0x80,
                "LDB" => 0x81,
                "LDC" => 0x82,
                "LDD" => 0x83,
                "LDE" => 0x84,
                "LDF" => 0x85,
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
            if (!_mnemonics.TryGetValue(mnemonic, out byte opcode))
                throw new AssemblerException($"Unknown store instruction: {mnemonic}");

            bytes.Add(opcode);
            ushort address = ParseAddress(operand, currentAddress);
            bytes.Add((byte)(address & 0xFF));
            bytes.Add((byte)((address >> 8) & 0xFF));
        }

        return bytes;
    }

    /// <summary>
    /// Assemble indexed load instructions like LDA (IDX1), LDB (IDX1+5), etc.
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
    /// Assemble indexed store instructions like STA (IDX1), STB (IDX1+5), etc.
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
                ("STA", "IDX1") => 0x94,  // STA (IDX1+offset)
                ("STB", "IDX1") => 0x95,  // STB (IDX1+offset)
                ("STA", "IDY1") => 0x96,  // STA (IDY1+offset)
                ("STB", "IDY1") => 0x97,  // STB (IDY1+offset)
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
                ("STA", "IDX1") => 0x8A,  // STA (IDX1)
                ("STB", "IDX1") => 0x8B,  // STB (IDX1)
                ("STA", "IDY1") => 0x8C,  // STA (IDY1)
                ("STB", "IDY1") => 0x8D,  // STB (IDY1)
                _ => throw new AssemblerException($"Invalid indexed store instruction: {mnemonic} ({inner})")
            };
            
            bytes.Add(opcode);
        }
        
        return bytes;
    }

    /// <summary>
    /// Parse an indexed expression with offset like "IDX1+5" or "IDY1-10"
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

    private List<byte> AssembleLdda_LddbInstruction(string mnemonic, string[] parts, ushort currentAddress)
    {
        var bytes = new List<byte>();

        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires one operand");

        string operand = parts[1];

        if (operand.StartsWith('#'))
        {
            // 16-bit immediate load
            byte opcode = _mnemonics[mnemonic];
            bytes.Add(opcode);
            ushort value = ParseValue16(operand.Substring(1));
            bytes.Add((byte)(value & 0xFF));
            bytes.Add((byte)((value >> 8) & 0xFF));
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

        if (!operand.StartsWith('#'))
            throw new AssemblerException($"Index load instruction {mnemonic} requires immediate value (format: {mnemonic} #value)");

        // Index register load with 16-bit immediate value
        byte opcode = _mnemonics[mnemonic];
        bytes.Add(opcode);

        ushort value = 0;
        if (!GetLabelReference(operand.Substring(1), out value))
        {
            value = ParseValue16(operand.Substring(1));
        }

        bytes.Add((byte)(value & 0xFF));        // Low byte first (little-endian)
        bytes.Add((byte)((value >> 8) & 0xFF)); // High byte second

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
        byte opcode = _mnemonics[mnemonic];
        bytes.Add(opcode);
        ushort value = ParseValue16(operand.Substring(1));
        bytes.Add((byte)(value & 0xFF));        // Low byte first (little-endian)
        bytes.Add((byte)((value >> 8) & 0xFF)); // High byte second

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
            throw new AssemblerException("CMP instruction requires two registers (format: CMP A,B or CMP A,C)");

        // Parse registers from the operand part, handling spaces properly
        var registers = ParseTwoRegisters(parts[1], "CMP");

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

        byte opcode = _mnemonics[mnemonic];
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
            ("PUSH", "A") => 0x70,
            ("POP", "A") => 0x71,
            ("PUSH", "DA") => 0x72,
            ("POP", "DA") => 0x73,
            ("PUSH", "B") => 0x74,
            ("POP", "B") => 0x75,
            ("PUSH", "DB") => 0x76,
            ("POP", "DB") => 0x77,
            ("PUSH", "C") => 0x78,
            ("POP", "C") => 0x79,
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
            ("A", "B") => 0xA0,
            ("A", "C") => 0xA1,
            ("B", "A") => 0xA2,
            ("B", "C") => 0xA3,
            ("C", "A") => 0xA4,
            ("C", "B") => 0xA5,
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
            ("A", "B") => 0xA6,
            ("A", "C") => 0xA7,
            _ => throw new AssemblerException($"Invalid SWP registers: {reg1},{reg2} (only A,B and A,C supported)")
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

            if (c == '\'' && !inString)
            {
                inString = true;
                currentPart.Append(c);
            }
            else if (c == '\'' && inString)
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

    private byte ParseValue(string value)
    {
        value = value.Trim();

        if (value.StartsWith('\'') && value.EndsWith('\'') && value.Length == 3)
        {
            return (byte)value[1];
        }

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

        try
        {
            return Convert.ToUInt16(value);
        }
        catch (OverflowException)
        {
            throw new OverflowException("Value was either too large or too small for a UInt16.");
        }
        catch (FormatException)
        {
            throw new AssemblerException($"Invalid numeric format: {value}");
        }
    }

    private ushort ParseAddress(string address, ushort currentAddress)
    {
        // If address is something like (IDX1-200), we need to handle it    

        address = address.Trim();

        // Handle indexed addressing expressions like (IDX1+5) or (IDY1-10)
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
    /// Parse indexed addressing expressions like (IDX1+5), (IDY1-10), (IDX2+0), etc.
    /// This method handles the evaluation of index register expressions with offsets.
    /// </summary>
    /// <param name="expression">The indexed expression including parentheses, e.g., "(IDX1+5)"</param>
    /// <returns>The evaluated address or throws an exception if invalid</returns>
    private ushort ParseIndexedAddressExpression(string expression)
    {
        // Remove parentheses
        string inner = expression.Substring(1, expression.Length - 2).Trim();
        
        // Indexed expressions should now be handled by the specific instruction assemblers
        // (AssembleIndexedLoadInstruction, AssembleIndexedStoreInstruction)
        // If we reach this point, it means an indexed expression was used in a context
        // where it's not supported (like JMP (IDX1+5) which doesn't exist)
        
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
                throw new AssemblerException($"Label non défini: {labelName}");

            if (position + 1 >= machineCode.Count)
                throw new AssemblerException($"Position invalide pour le label {labelName}");

            machineCode[position] = (byte)(address & 0xFF);
            machineCode[position + 1] = (byte)((address >> 8) & 0xFF);
        }
    }
}
