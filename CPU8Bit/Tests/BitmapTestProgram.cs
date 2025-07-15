using CPU8Bit;
using CPU8Bit.Graphics;

namespace CPU8Bit.Tests;

/// <summary>
/// Programme de test pour démontrer les nouvelles capacités bitmap
/// </summary>
public class BitmapTestProgram
{
    public static void RunBitmapTest()
    {
        Console.WriteLine("=== Test du Système Bitmap ===");
        
        // Créer la mémoire et le contrôleur vidéo
        var memory = new Memory(0x10000);
        var videoController = new VideoController(memory);
        var renderer = new BitmapRenderer(videoController);
        
        Console.WriteLine("Nouveau layout mémoire:");
        Console.WriteLine(memory.GetMemoryLayout());
        Console.WriteLine();
        
        // Test 1: Effacer l'écran
        Console.WriteLine("Test 1: Effacement de l'écran...");
        videoController.ClearScreen(1); // Bleu foncé
        
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
        
        // Test 4: Dessiner des caractères individuels
        Console.WriteLine("Test 4: Caractères spéciaux...");
        for (int i = 0; i < 32; i++)
        {
            videoController.DrawTextCharacter(i, 5, (byte)i, 15, 0);
        }
        
        // Test 5: Test de la police
        Console.WriteLine("Test 5: Test complet de la police...");
        int textX = 0, textY = 7;
        for (byte c = 32; c < 128; c++) // Caractères ASCII imprimables
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
        
        // Test 7: Vérification du frame buffer
        Console.WriteLine("Test 7: Vérification du frame buffer...");
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
        
        // Test 8: Test de synchronisation mémoire
        Console.WriteLine("Test 8: Synchronisation mémoire...");
        memory.SetPixel(100, 100, 15); // Pixel blanc via la mémoire
        videoController.SyncFromMemory();
        byte pixelFromController = videoController.GetPixel(100, 100);
        Console.WriteLine($"Pixel (100,100) via mémoire puis contrôleur: {pixelFromController}");
        
        Console.WriteLine("\n=== Tests bitmap terminés avec succès! ===");
        
        // Nettoyer
        renderer.Dispose();
    }
}