using System.Text;

namespace FSAssembler;

/// <summary>
/// Assembleur pour le processeur 8 bits FSGameConsole
/// Compile les fichiers .fs8 en code machine
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FSAssembler v2.0 - Assembleur pour FSGameConsole");
        Console.WriteLine("===============================================");

        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        string inputFile = args[0];
        string outputFile = args.Length > 1 ? args[1] : Path.ChangeExtension(inputFile, ".bin");

        try
        {
            var assembler = new Assembler();
            byte[] machineCode = assembler.AssembleFile(inputFile);
            
            File.WriteAllBytes(outputFile, machineCode);
            
            Console.WriteLine($"✓ Assemblage réussi!");
            Console.WriteLine($"  Fichier d'entrée: {inputFile}");
            Console.WriteLine($"  Fichier de sortie: {outputFile}");
            Console.WriteLine($"  Taille: {machineCode.Length} octets");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Erreur: {ex.Message}");
            Environment.ExitCode = 1;
        }
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  FSAssembler <fichier.fs8> [sortie.bin]");
        Console.WriteLine();
        Console.WriteLine("Instructions supportées:");
        Console.WriteLine("  Basic: NOP, HALT, SYS");
        Console.WriteLine("  Load: LDA #val, LDB addr, LDDA #val16");
        Console.WriteLine("  Arithmetic: ADD, SUB, ADD16, SUB16, INC A, DEC A");
        Console.WriteLine("  Logic: AND A,B, OR A,B, XOR A,B, NOT A, SHL A");
        Console.WriteLine("  Jump: JMP addr, JZ addr, JR offset");
        Console.WriteLine("  Store: STA addr, STB addr");
        Console.WriteLine("  Stack: PUSH A, POP A, PUSH DA, POP DA");
        Console.WriteLine("  Transfer: MOV A,B, SWP A,B");
        Console.WriteLine("  Compare: CMP A,B");
        Console.WriteLine("  Subroutine: CALL addr, RET");
    }
}

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
            
            // System call
            { "SYS", 0xF0 }
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

        return mnemonic switch
        {
            "NOP" or "HALT" or "RET" or "SYS" or "ADD" or "SUB" or "ADD16" or "SUB16" => 1,
            "LDA" or "LDB" or "LDC" or "LDD" or "LDE" or "LDF" when parts.Length == 2 && parts[1].StartsWith('#') => 2,
            "LDA" or "LDB" or "LDC" or "LDD" or "LDE" or "LDF" => 3,
            "LDDA" or "LDDB" when parts.Length == 2 && parts[1].StartsWith('#') => 3,
            "LDDA" or "LDDB" => 3,
            "INC" or "DEC" or "INC16" or "DEC16" or "CMP" => 1,
            "AND" or "OR" or "XOR" or "NOT" or "SHL" or "SHR" => 1,
            "JMP" or "JZ" or "JNZ" or "JC" or "JNC" or "JN" or "JNN" => 3,
            "JR" or "JRZ" or "JRNZ" or "JRC" => 2,
            "STA" or "STB" or "STC" or "STD" or "STDA" or "STDB" or "CALL" => 3,
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
                
            case "LDDA":
            case "LDDB":
                return AssembleLdda_LddbInstruction(mnemonic, parts, currentAddress);
                
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
        else
        {
            // Memory load
            byte opcode = mnemonic switch
            {
                "LDA" => 0x80, "LDB" => 0x81, "LDC" => 0x82,
                "LDD" => 0x83, "LDE" => 0x84, "LDF" => 0x85,
                _ => throw new AssemblerException($"Invalid load instruction: {mnemonic}")
            };
            
            bytes.Add(opcode);
            ushort address = ParseAddress(operand, currentAddress);
            bytes.Add((byte)(address & 0xFF));
            bytes.Add((byte)((address >> 8) & 0xFF));
        }
        
        return bytes;
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

    private List<byte> AssembleIncDecInstruction(string mnemonic, string[] parts)
    {
        var bytes = new List<byte>();
        
        if (parts.Length != 2)
            throw new AssemblerException($"Instruction {mnemonic} requires a register");

        string register = parts[1].ToUpper();
        
        byte opcode = (mnemonic.ToUpper(), register) switch
        {
            ("INC", "A") => 0x28, ("DEC", "A") => 0x29,
            ("INC", "B") => 0x2A, ("DEC", "B") => 0x2B,
            ("INC", "C") => 0x2D, ("DEC", "C") => 0x2E,
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
            ("INC16", "DA") => 0x24, ("DEC16", "DA") => 0x25,
            ("INC16", "DB") => 0x26, ("DEC16", "DB") => 0x27,
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
            ("AND", "A", "B") => 0x30, ("AND", "A", "C") => 0x36,
            ("OR", "A", "B") => 0x31, ("OR", "A", "C") => 0x37,
            ("XOR", "A", "B") => 0x32, ("XOR", "A", "C") => 0x38,
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
            ("PUSH", "A") => 0x70, ("POP", "A") => 0x71,
            ("PUSH", "DA") => 0x72, ("POP", "DA") => 0x73,
            ("PUSH", "B") => 0x74, ("POP", "B") => 0x75,
            ("PUSH", "DB") => 0x76, ("POP", "DB") => 0x77,
            ("PUSH", "C") => 0x78, ("POP", "C") => 0x79,
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
            ("A", "B") => 0xA0, ("A", "C") => 0xA1,
            ("B", "A") => 0xA2, ("B", "C") => 0xA3,
            ("C", "A") => 0xA4, ("C", "B") => 0xA5,
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
        address = address.Trim();

        if (!char.IsDigit(address[0]) && !address.StartsWith("0x") && !address.StartsWith("$"))
        {
            _unresolvedReferences.Add((currentAddress + 1, address));
            return 0;
        }

        if (address.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.ToUInt16(address.Substring(2), 16);
        }

        if (address.StartsWith("$"))
        {
            return Convert.ToUInt16(address.Substring(1), 16);
        }

        return Convert.ToUInt16(address);
    }

    private sbyte ParseRelativeOffset(string offsetStr, ushort currentAddress)
    {
        offsetStr = offsetStr.Trim();

        if (!char.IsDigit(offsetStr[0]) && !offsetStr.StartsWith("-") && !offsetStr.StartsWith("+") && 
            !offsetStr.StartsWith("0x") && !offsetStr.StartsWith("$"))
        {
            if (_labels.TryGetValue(offsetStr, out ushort targetAddress))
            {
                int offset = targetAddress - (currentAddress + 2);
                if (offset < -128 || offset > 127)
                    throw new AssemblerException($"Relative jump offset out of range: {offset} (must be -128 to +127)");
                return (sbyte)offset;
            }
            else
            {
                throw new AssemblerException($"Undefined label for relative jump: {offsetStr}");
            }
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

/// <summary>
/// Exception spécifique aux erreurs d'assemblage
/// </summary>
public class AssemblerException : Exception
{
    public AssemblerException(string message) : base(message) { }
    public AssemblerException(string message, Exception innerException) : base(message, innerException) { }
}