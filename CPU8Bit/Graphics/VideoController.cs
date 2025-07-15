using System.Drawing;

namespace CPU8Bit.Graphics;

/// <summary>
/// Contr�leur vid�o pour l'�mulation d'un affichage bitmap 320x200 16 couleurs
/// Approche CPC authentique - PAS de m�moire texte s�par�e
/// Tout est rendu directement dans la m�moire bitmap unifi�e
/// </summary>
public class VideoController
{
    // Configuration de l'�cran
    public const int ScreenWidth = 320;
    public const int ScreenHeight = 200;
    public const int ColorsCount = 16;
    
    // Configuration des caract�res
    public const int CharsPerLine = ScreenWidth / AmstradCPCFont.CharWidth;  // 40 caract�res par ligne
    public const int LinesOfText = ScreenHeight / AmstradCPCFont.CharHeight; // 25 lignes de texte
    
    private readonly Memory _memory;
    private readonly byte[] _frameBuffer;
    private readonly Color[] _palette;
    
    // Zones m�moire
    private readonly int _bitmapMemoryStart;
    private readonly int _bitmapMemorySize;
    
    public VideoController(Memory memory)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        
        // Calculer la taille du frame buffer (1 pixel = 4 bits, donc 2 pixels par octet)
        _frameBuffer = new byte[ScreenWidth * ScreenHeight / 2];
        
        // Configuration des zones m�moire - approche CPC authentique
        _bitmapMemorySize = _memory.BitmapMemorySize; // 16KB comme un vrai CPC
        _bitmapMemoryStart = _memory.BitmapMemoryStart; // 0x8000
        
        // Initialiser la palette de couleurs 16 couleurs style CGA/EGA
        _palette = new Color[ColorsCount]
        {
            Color.FromArgb(0, 0, 0),       // 0 - Noir
            Color.FromArgb(0, 0, 128),     // 1 - Bleu fonc�
            Color.FromArgb(0, 128, 0),     // 2 - Vert fonc�
            Color.FromArgb(0, 128, 128),   // 3 - Cyan fonc�
            Color.FromArgb(128, 0, 0),     // 4 - Rouge fonc�
            Color.FromArgb(128, 0, 128),   // 5 - Magenta fonc�
            Color.FromArgb(128, 128, 0),   // 6 - Jaune fonc�/Brun
            Color.FromArgb(192, 192, 192), // 7 - Gris clair
            Color.FromArgb(128, 128, 128), // 8 - Gris fonc�
            Color.FromArgb(0, 0, 255),     // 9 - Bleu clair
            Color.FromArgb(0, 255, 0),     // 10 - Vert clair
            Color.FromArgb(0, 255, 255),   // 11 - Cyan clair
            Color.FromArgb(255, 0, 0),     // 12 - Rouge clair
            Color.FromArgb(255, 0, 255),   // 13 - Magenta clair
            Color.FromArgb(255, 255, 0),   // 14 - Jaune clair
            Color.FromArgb(255, 255, 255)  // 15 - Blanc
        };
        
        // Initialiser l'�cran avec un fond noir propre
        ClearScreen(0); // �cran noir au d�marrage
    }
    
    /// <summary>
    /// Adresse de d�but de la m�moire bitmap
    /// </summary>
    public int BitmapMemoryStart => _bitmapMemoryStart;
    
    /// <summary>
    /// Taille de la m�moire bitmap
    /// </summary>
    public int BitmapMemorySize => _bitmapMemorySize;
    
    /// <summary>
    /// Propri�t�s obsol�tes - plus de m�moire texte s�par�e dans l'approche CPC authentique
    /// </summary>
    [Obsolete("Plus de m�moire texte s�par�e - tout est dans la m�moire bitmap unifi�e")]
    public int TextMemoryStart => _bitmapMemoryStart;
    
    [Obsolete("Plus de m�moire texte s�par�e - tout est dans la m�moire bitmap unifi�e")]
    public int TextMemorySize => 1000;
    
    /// <summary>
    /// Obtient la couleur � partir de l'index de palette
    /// </summary>
    public Color GetColor(byte colorIndex)
    {
        return _palette[colorIndex & 0x0F]; // Limiter � 16 couleurs
    }
    
    /// <summary>
    /// D�finit un pixel dans le frame buffer
    /// </summary>
    /// <param name="x">Position X (0-319)</param>
    /// <param name="y">Position Y (0-199)</param>
    /// <param name="colorIndex">Index de couleur (0-15)</param>
    public void SetPixel(int x, int y, byte colorIndex)
    {
        if (x < 0 || x >= ScreenWidth || y < 0 || y >= ScreenHeight)
            return;
            
        colorIndex &= 0x0F; // Limiter � 16 couleurs
        
        int pixelIndex = y * ScreenWidth + x;
        int byteIndex = pixelIndex / 2;
        bool isHighNibble = (pixelIndex % 2) == 0;
        
        if (isHighNibble)
        {
            _frameBuffer[byteIndex] = (byte)((_frameBuffer[byteIndex] & 0x0F) | (colorIndex << 4));
        }
        else
        {
            _frameBuffer[byteIndex] = (byte)((_frameBuffer[byteIndex] & 0xF0) | colorIndex);
        }
        
        // Synchroniser avec la m�moire
        if (byteIndex < _bitmapMemorySize)
        {
            _memory.WriteByte((ushort)(_bitmapMemoryStart + byteIndex), _frameBuffer[byteIndex]);
        }
    }
    
    /// <summary>
    /// Obtient la couleur d'un pixel
    /// </summary>
    /// <param name="x">Position X (0-319)</param>
    /// <param name="y">Position Y (0-199)</param>
    /// <returns>Index de couleur (0-15)</returns>
    public byte GetPixel(int x, int y)
    {
        if (x < 0 || x >= ScreenWidth || y < 0 || y >= ScreenHeight)
            return 0;
            
        int pixelIndex = y * ScreenWidth + x;
        int byteIndex = pixelIndex / 2;
        bool isHighNibble = (pixelIndex % 2) == 0;
        
        if (byteIndex >= _frameBuffer.Length)
            return 0;
            
        if (isHighNibble)
        {
            return (byte)((_frameBuffer[byteIndex] & 0xF0) >> 4);
        }
        else
        {
            return (byte)(_frameBuffer[byteIndex] & 0x0F);
        }
    }
    
    /// <summary>
    /// Efface l'�cran avec une couleur
    /// </summary>
    /// <param name="colorIndex">Index de couleur (0-15)</param>
    public void ClearScreen(byte colorIndex = 0)
    {
        colorIndex &= 0x0F;
        byte fillValue = (byte)((colorIndex << 4) | colorIndex);
        
        Array.Fill(_frameBuffer, fillValue);
        
        // Synchroniser avec la m�moire
        for (int i = 0; i < _bitmapMemorySize && i < _frameBuffer.Length; i++)
        {
            _memory.WriteByte((ushort)(_bitmapMemoryStart + i), fillValue);
        }
    }
    
    /// <summary>
    /// Dessine un caract�re � une position donn�e
    /// </summary>
    /// <param name="x">Position X en pixels</param>
    /// <param name="y">Position Y en pixels</param>
    /// <param name="charCode">Code ASCII du caract�re</param>
    /// <param name="foregroundColor">Couleur du texte (0-15)</param>
    /// <param name="backgroundColor">Couleur de fond (0-15)</param>
    public void DrawCharacter(int x, int y, byte charCode, byte foregroundColor = 15, byte backgroundColor = 0)
    {
        byte[] charData = AmstradCPCFont.GetCharacterData(charCode);
        
        for (int charY = 0; charY < AmstradCPCFont.CharHeight; charY++)
        {
            byte rowData = charData[charY];
            for (int charX = 0; charX < AmstradCPCFont.CharWidth; charX++)
            {
                bool pixelOn = (rowData & (0x80 >> charX)) != 0;
                byte pixelColor = pixelOn ? foregroundColor : backgroundColor;
                SetPixel(x + charX, y + charY, pixelColor);
            }
        }
    }
    
    /// <summary>
    /// Dessine un caract�re � une position de grille texte
    /// </summary>
    /// <param name="textX">Position X en caract�res (0-39)</param>
    /// <param name="textY">Position Y en caract�res (0-24)</param>
    /// <param name="charCode">Code ASCII du caract�re</param>
    /// <param name="foregroundColor">Couleur du texte (0-15)</param>
    /// <param name="backgroundColor">Couleur de fond (0-15)</param>
    public void DrawTextCharacter(int textX, int textY, byte charCode, byte foregroundColor = 15, byte backgroundColor = 0)
    {
        if (textX < 0 || textX >= CharsPerLine || textY < 0 || textY >= LinesOfText)
            return;
            
        int pixelX = textX * AmstradCPCFont.CharWidth;
        int pixelY = textY * AmstradCPCFont.CharHeight;
        
        DrawCharacter(pixelX, pixelY, charCode, foregroundColor, backgroundColor);
        
        // PLUS de synchronisation avec une m�moire texte s�par�e
        // Dans l'approche CPC authentique, tout est directement dans la bitmap
    }
    
    /// <summary>
    /// Dessine une cha�ne de texte
    /// </summary>
    /// <param name="textX">Position X de d�part en caract�res</param>
    /// <param name="textY">Position Y en caract�res</param>
    /// <param name="text">Texte � afficher</param>
    /// <param name="foregroundColor">Couleur du texte (0-15)</param>
    /// <param name="backgroundColor">Couleur de fond (0-15)</param>
    public void DrawText(int textX, int textY, string text, byte foregroundColor = 15, byte backgroundColor = 0)
    {
        if (string.IsNullOrEmpty(text))
            return;
            
        for (int i = 0; i < text.Length; i++)
        {
            int currentX = textX + i;
            if (currentX >= CharsPerLine)
                break; // D�bordement de ligne
                
            DrawTextCharacter(currentX, textY, (byte)text[i], foregroundColor, backgroundColor);
        }
    }
    
    /// <summary>
    /// Synchronise le frame buffer avec la m�moire bitmap
    /// </summary>
    public void SyncFromMemory()
    {
        for (int i = 0; i < _bitmapMemorySize && i < _frameBuffer.Length; i++)
        {
            _frameBuffer[i] = _memory.ReadByte((ushort)(_bitmapMemoryStart + i));
        }
    }
    
    /// <summary>
    /// Obtient le frame buffer pour l'affichage
    /// </summary>
    /// <returns>Frame buffer contenant les donn�es de pixels</returns>
    public byte[] GetFrameBuffer()
    {
        return (byte[])_frameBuffer.Clone();
    }
    
    /// <summary>
    /// M�THODE OBSOL�TE - Plus de m�moire texte s�par�e dans l'approche CPC authentique
    /// Les programmes doivent utiliser DrawText() directement ou �crire dans la bitmap
    /// </summary>
    [Obsolete("Plus de m�moire texte s�par�e - utiliser DrawText() directement")]
    public void ProcessTextMemory(byte foregroundColor = 15, byte backgroundColor = 0)
    {
        // Dans l'approche CPC authentique, cette m�thode ne fait plus rien
        // Les programmes doivent dessiner directement dans la bitmap
        // via VideoController.DrawText() ou en manipulant les pixels
    }
}