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
        Console.WriteLine("  Load: LDA #val, LDB addr, LDDA #val16, LDIX1 #val16");
        Console.WriteLine("  Arithmetic: ADD, SUB, ADD16, SUB16, INC A, DEC A");
        Console.WriteLine("  Index: INCIX1, DECIY1, ADDIX1 #val16, MVIX1IX2, SWPIX1IX2");
        Console.WriteLine("  Auto-inc: LDAIX1+, STAIY1+, LDAIX1-, STAIY1-");
        Console.WriteLine("  Logic: AND A,B, OR A,B, XOR A,B, NOT A, SHL A");
        Console.WriteLine("  Jump: JMP addr, JZ addr, JR offset");
        Console.WriteLine("  Store: STA addr, STB addr");
        Console.WriteLine("  Stack: PUSH A, POP A, PUSH DA, POP DA");
        Console.WriteLine("  Transfer: MOV A,B, SWP A,B");
        Console.WriteLine("  Compare: CMP A,B");
        Console.WriteLine("  Subroutine: CALL addr, RET");
    }
}
