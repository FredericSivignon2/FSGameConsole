using CPU8Bit;
using CPU8Bit.Graphics;

namespace CPU8Bit.Tests;

/// <summary>
/// Programme de test pour d�montrer les nouvelles capacit�s bitmap
/// </summary>
public class BitmapTestProgram
{
    public static void RunBitmapTest()
    {
        Console.WriteLine("=== Test du Syst�me Bitmap ===");
        
        // Cr�er la m�moire et le contr�leur vid�o
        var memory = new Memory(0x10000);
        var videoController = new VideoController(memory);
        var renderer = new BitmapRenderer(videoController);
        
        Console.WriteLine("Nouveau layout m�moire:");
        Console.WriteLine(memory.GetMemoryLayout());
        Console.WriteLine();
        
        // Test 1: Effacer l'�cran
        Console.WriteLine("Test 1: Effacement de l'�cran...");
        videoController.ClearScreen(1); // Bleu fonc�
        
        // Test 2: Dessiner des pixels
        Console.WriteLine("Test 2: Dessin de pixels...");
        for (int i = 0; i < 50; i++)
        {
            videoController.SetPixel(i * 2, i, (byte)(i % 16));
        }
        
        // Test 3: Dessiner du texte avec la police Amstrad CPC
        Console.WriteLine("Test 3: Affichage de texte...");
        videoController.DrawText(0, 0, "HELLO AMSTRAD CPC!", 14, 0); // Jaune sur noir
        videoController.DrawText(0, 1, "Police 8x8 pixels", 10, 0);  // Vert clair sur noir
        videoController.DrawText(0, 2, "Resolution 320x200", 12, 0); // Rouge clair sur noir
        videoController.DrawText(0, 3, "16 couleurs", 13, 0);        // Magenta clair sur noir
        
        // Test 4: Dessiner des caract�res individuels
        Console.WriteLine("Test 4: Caract�res sp�ciaux...");
        for (int i = 0; i < 32; i++)
        {
            videoController.DrawTextCharacter(i, 5, (byte)i, 15, 0);
        }
        
        // Test 5: Test de la police
        Console.WriteLine("Test 5: Test complet de la police...");
        int textX = 0, textY = 7;
        for (byte c = 32; c < 128; c++) // Caract�res ASCII imprimables
        {
            videoController.DrawTextCharacter(textX, textY, c, 7, 0);
            textX++;
            if (textX >= VideoController.CharsPerLine)
            {
                textX = 0;
                textY++;
                if (textY >= VideoController.LinesOfText)
                    break;
            }
        }
        
        // Test 6: Informations de rendu
        Console.WriteLine("Test 6: Informations de rendu:");
        Console.WriteLine(renderer.GetRenderInfo());
        Console.WriteLine();
        
        // Test 7: V�rification du frame buffer
        Console.WriteLine("Test 7: V�rification du frame buffer...");
        byte[] frameBuffer = videoController.GetFrameBuffer();
        Console.WriteLine($"Taille du frame buffer: {frameBuffer.Length} octets");
        
        // Compter les pixels non-noirs
        int nonBlackPixels = 0;
        for (int y = 0; y < VideoController.ScreenHeight; y++)
        {
            for (int x = 0; x < VideoController.ScreenWidth; x++)
            {
                if (videoController.GetPixel(x, y) != 0)
                    nonBlackPixels++;
            }
        }
        Console.WriteLine($"Pixels non-noirs: {nonBlackPixels}");
        
        // Test 8: Test de synchronisation m�moire
        Console.WriteLine("Test 8: Synchronisation m�moire...");
        memory.SetPixel(100, 100, 15); // Pixel blanc via la m�moire
        videoController.SyncFromMemory();
        byte pixelFromController = videoController.GetPixel(100, 100);
        Console.WriteLine($"Pixel (100,100) via m�moire puis contr�leur: {pixelFromController}");
        
        Console.WriteLine("\n=== Tests bitmap termin�s avec succ�s! ===");
        
        // Nettoyer
        renderer.Dispose();
    }
}