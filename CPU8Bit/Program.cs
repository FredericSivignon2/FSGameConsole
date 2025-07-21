using FSCPU.Tests;

namespace FSCPU;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("FSGameConsole - Test du Système Bitmap");
        Console.WriteLine("=====================================");
        Console.WriteLine();
        
        try
        {
            BitmapTestProgram.RunBitmapTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nAppuyez sur une touche pour quitter...");
        Console.ReadKey();
    }
}