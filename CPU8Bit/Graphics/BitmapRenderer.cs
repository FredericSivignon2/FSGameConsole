using System.Drawing;
using System.Drawing.Imaging;
using SystemGraphics = System.Drawing.Graphics;

namespace FSCPU.Graphics;

/// <summary>
/// Moteur de rendu bitmap pour convertir le frame buffer en image affichable
/// </summary>
/*public class BitmapRenderer
{
    private readonly VideoController _videoController;
    private Bitmap? _displayBitmap;
    private SystemGraphics? _displayGraphics;
    
    public BitmapRenderer(VideoController videoController)
    {
        _videoController = videoController ?? throw new ArgumentNullException(nameof(videoController));
        
        // Créer le bitmap de sortie
        _displayBitmap = new Bitmap(VideoController.ScreenWidth, VideoController.ScreenHeight, PixelFormat.Format32bppArgb);
        _displayGraphics = SystemGraphics.FromImage(_displayBitmap);
        _displayGraphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        _displayGraphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
    }
    
    /// <summary>
    /// Rend le frame buffer en bitmap affichable
    /// </summary>
    /// <returns>Bitmap prêt pour l'affichage</returns>
    public Bitmap RenderFrame()
    {
        if (_displayBitmap == null || _displayGraphics == null)
            return new Bitmap(1, 1);
            
        // Synchroniser avec la mémoire
        _videoController.SyncFromMemory();
        
        // Obtenir le frame buffer
        byte[] frameBuffer = _videoController.GetFrameBuffer();
        
        // Convertir le frame buffer en bitmap
        BitmapData bitmapData = _displayBitmap.LockBits(
            new Rectangle(0, 0, VideoController.ScreenWidth, VideoController.ScreenHeight),
            ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        
        unsafe
        {
            byte* pixels = (byte*)bitmapData.Scan0;
            int stride = bitmapData.Stride;
            
            for (int y = 0; y < VideoController.ScreenHeight; y++)
            {
                for (int x = 0; x < VideoController.ScreenWidth; x++)
                {
                    // Obtenir l'index de couleur pour ce pixel
                    byte colorIndex = _videoController.GetPixel(x, y);
                    Color color = _videoController.GetColor(colorIndex);
                    
                    // Calculer l'offset dans le bitmap
                    int offset = y * stride + x * 4;
                    
                    // Écrire les composantes BGRA
                    pixels[offset] = color.B;     // Blue
                    pixels[offset + 1] = color.G; // Green
                    pixels[offset + 2] = color.R; // Red
                    pixels[offset + 3] = color.A; // Alpha
                }
            }
        }
        
        _displayBitmap.UnlockBits(bitmapData);
        
        return _displayBitmap;
    }
    
    /// <summary>
    /// Rend le frame buffer en bitmap avec mise à l'échelle
    /// </summary>
    /// <param name="scaleX">Facteur d'échelle horizontal</param>
    /// <param name="scaleY">Facteur d'échelle vertical</param>
    /// <returns>Bitmap mis à l'échelle</returns>
    public Bitmap RenderFrameScaled(int scaleX, int scaleY)
    {
        var originalBitmap = RenderFrame();
        
        if (scaleX == 1 && scaleY == 1)
            return originalBitmap;
            
        var scaledBitmap = new Bitmap(
            VideoController.ScreenWidth * scaleX,
            VideoController.ScreenHeight * scaleY,
            PixelFormat.Format32bppArgb);
            
        using (var graphics = SystemGraphics.FromImage(scaledBitmap))
        {
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            
            graphics.DrawImage(originalBitmap,
                new Rectangle(0, 0, scaledBitmap.Width, scaledBitmap.Height),
                new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height),
                GraphicsUnit.Pixel);
        }
        
        return scaledBitmap;
    }
    
    /// <summary>
    /// Rend directement dans un Graphics (pour WinForms)
    /// </summary>
    /// <param name="graphics">Graphics de destination</param>
    /// <param name="destRect">Rectangle de destination</param>
    public void RenderToGraphics(SystemGraphics graphics, Rectangle destRect)
    {
        var frameBitmap = RenderFrame();
        
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        
        graphics.DrawImage(frameBitmap, destRect,
            new Rectangle(0, 0, VideoController.ScreenWidth, VideoController.ScreenHeight),
            GraphicsUnit.Pixel);
    }
    
    /// <summary>
    /// Obtient des statistiques de rendu pour debug
    /// </summary>
    /// <returns>Informations de debug</returns>
    public string GetRenderInfo()
    {
        return $"Résolution: {VideoController.ScreenWidth}x{VideoController.ScreenHeight}\n" +
               $"Couleurs: {VideoController.ColorsCount}\n" +
               $"Mode texte: {VideoController.CharsPerLine}x{VideoController.LinesOfText}\n" +
               $"Police: {AmstradCPCFont.CharWidth}x{AmstradCPCFont.CharHeight}";
    }
    
    /// <summary>
    /// Libère les ressources
    /// </summary>
    public void Dispose()
    {
        _displayGraphics?.Dispose();
        _displayBitmap?.Dispose();
    }
}*/