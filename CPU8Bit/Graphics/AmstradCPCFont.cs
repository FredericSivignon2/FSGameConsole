namespace FSCPU.Graphics;

/// <summary>
/// Police de caract�res bitmap inspir�e de l'Amstrad CPC
/// Chaque caract�re fait 8x8 pixels
/// </summary>
public static class AmstradCPCFont
{
    /// <summary>
    /// Largeur d'un caract�re en pixels
    /// </summary>
    public const int CharWidth = 8;
    
    /// <summary>
    /// Hauteur d'un caract�re en pixels
    /// </summary>
    public const int CharHeight = 8;
    
    /// <summary>
    /// Nombre de caract�res dans la police
    /// </summary>
    public const int CharCount = 128;
    
    /// <summary>
    /// Police de caract�res Amstrad CPC 8x8 pixels
    /// Chaque caract�re est repr�sent� par 8 octets (1 par ligne)
    /// Chaque bit repr�sente un pixel (1 = allum�, 0 = �teint)
    /// </summary>
    private static readonly byte[,] _fontData = new byte[CharCount, CharHeight]
    {
        // 0x00 - NULL
        { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
        // 0x01 - SOH
        { 0x7E, 0x81, 0xA5, 0x81, 0xBD, 0x99, 0x81, 0x7E },
        // 0x02 - STX
        { 0x7E, 0xFF, 0xDB, 0xFF, 0xC3, 0xE7, 0xFF, 0x7E },
        // 0x03 - ETX (coeur)
        { 0x6C, 0xFE, 0xFE, 0xFE, 0x7C, 0x38, 0x10, 0x00 },
        // 0x04 - EOT (diamant)
        { 0x10, 0x38, 0x7C, 0xFE, 0x7C, 0x38, 0x10, 0x00 },
        // 0x05 - ENQ (tr�fle)
        { 0x38, 0x7C, 0x38, 0xFE, 0xFE, 0x7C, 0x38, 0x7C },
        // 0x06 - ACK (pique)
        { 0x10, 0x10, 0x38, 0x7C, 0xFE, 0x7C, 0x38, 0x7C },
        // 0x07 - BEL (point)
        { 0x00, 0x00, 0x18, 0x3C, 0x3C, 0x18, 0x00, 0x00 },
        // 0x08 - BS
        { 0xFF, 0xFF, 0xE7, 0xC3, 0xC3, 0xE7, 0xFF, 0xFF },
        // 0x09 - TAB
        { 0x00, 0x3C, 0x66, 0x42, 0x42, 0x66, 0x3C, 0x00 },
        // 0x0A - LF
        { 0xFF, 0xC3, 0x99, 0xBD, 0xBD, 0x99, 0xC3, 0xFF },
        // 0x0B - VT
        { 0x0F, 0x07, 0x0F, 0x7D, 0xCC, 0xCC, 0xCC, 0x78 },
        // 0x0C - FF
        { 0x3C, 0x66, 0x66, 0x66, 0x3C, 0x18, 0x7E, 0x18 },
        // 0x0D - CR
        { 0x3F, 0x33, 0x3F, 0x30, 0x30, 0x70, 0xF0, 0xE0 },
        // 0x0E - SO
        { 0x7F, 0x63, 0x7F, 0x63, 0x63, 0x67, 0xE6, 0xC0 },
        // 0x0F - SI
        { 0x18, 0xDB, 0x3C, 0xE7, 0xE7, 0x3C, 0xDB, 0x18 },
        // 0x10 - DLE (triangle droite)
        { 0x80, 0xE0, 0xF8, 0xFE, 0xF8, 0xE0, 0x80, 0x00 },
        // 0x11 - DC1 (triangle gauche)
        { 0x02, 0x0E, 0x3E, 0xFE, 0x3E, 0x0E, 0x02, 0x00 },
        // 0x12 - DC2 (fl�che haut/bas)
        { 0x18, 0x3C, 0x7E, 0x18, 0x18, 0x7E, 0x3C, 0x18 },
        // 0x13 - DC3
        { 0x66, 0x66, 0x66, 0x66, 0x66, 0x00, 0x66, 0x00 },
        // 0x14 - DC4
        { 0x7F, 0xDB, 0xDB, 0x7B, 0x1B, 0x1B, 0x1B, 0x00 },
        // 0x15 - NAK
        { 0x3E, 0x61, 0x3C, 0x66, 0x66, 0x3C, 0x86, 0x7C },
        // 0x16 - SYN
        { 0x00, 0x00, 0x00, 0x00, 0x7E, 0x7E, 0x7E, 0x00 },
        // 0x17 - ETB
        { 0x18, 0x3C, 0x7E, 0x18, 0x7E, 0x3C, 0x18, 0xFF },
        // 0x18 - CAN (fl�che haut)
        { 0x18, 0x3C, 0x7E, 0x18, 0x18, 0x18, 0x18, 0x00 },
        // 0x19 - EM (fl�che bas)
        { 0x18, 0x18, 0x18, 0x18, 0x7E, 0x3C, 0x18, 0x00 },
        // 0x1A - SUB (fl�che droite)
        { 0x00, 0x18, 0x0C, 0xFE, 0x0C, 0x18, 0x00, 0x00 },
        // 0x1B - ESC (fl�che gauche)
        { 0x00, 0x30, 0x60, 0xFE, 0x60, 0x30, 0x00, 0x00 },
        // 0x1C - FS
        { 0x00, 0x00, 0xC0, 0xC0, 0xC0, 0xFE, 0x00, 0x00 },
        // 0x1D - GS
        { 0x00, 0x24, 0x66, 0xFF, 0x66, 0x24, 0x00, 0x00 },
        // 0x1E - RS
        { 0x00, 0x18, 0x3C, 0x7E, 0xFF, 0xFF, 0x00, 0x00 },
        // 0x1F - US
        { 0x00, 0xFF, 0xFF, 0x7E, 0x3C, 0x18, 0x00, 0x00 },
        
        // 0x20 - SPACE
        { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
        // 0x21 - !
        { 0x30, 0x78, 0x78, 0x30, 0x30, 0x00, 0x30, 0x00 },
        // 0x22 - "
        { 0x6C, 0x6C, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00 },
        // 0x23 - #
        { 0x6C, 0x6C, 0xFE, 0x6C, 0xFE, 0x6C, 0x6C, 0x00 },
        // 0x24 - $
        { 0x30, 0x7C, 0xC0, 0x78, 0x0C, 0xF8, 0x30, 0x00 },
        // 0x25 - %
        { 0x00, 0xC6, 0xCC, 0x18, 0x30, 0x66, 0xC6, 0x00 },
        // 0x26 - &
        { 0x38, 0x6C, 0x38, 0x76, 0xDC, 0xCC, 0x76, 0x00 },
        // 0x27 - '
        { 0x60, 0x60, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00 },
        // 0x28 - (
        { 0x18, 0x30, 0x60, 0x60, 0x60, 0x30, 0x18, 0x00 },
        // 0x29 - )
        { 0x60, 0x30, 0x18, 0x18, 0x18, 0x30, 0x60, 0x00 },
        // 0x2A - *
        { 0x00, 0x66, 0x3C, 0xFF, 0x3C, 0x66, 0x00, 0x00 },
        // 0x2B - +
        { 0x00, 0x30, 0x30, 0xFC, 0x30, 0x30, 0x00, 0x00 },
        // 0x2C - ,
        { 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x60 },
        // 0x2D - -
        { 0x00, 0x00, 0x00, 0xFC, 0x00, 0x00, 0x00, 0x00 },
        // 0x2E - .
        { 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x30, 0x00 },
        // 0x2F - /
        { 0x06, 0x0C, 0x18, 0x30, 0x60, 0xC0, 0x80, 0x00 },
        
        // 0x30 - 0
        { 0x7C, 0xC6, 0xCE, 0xDE, 0xF6, 0xE6, 0x7C, 0x00 },
        // 0x31 - 1
        { 0x30, 0x70, 0x30, 0x30, 0x30, 0x30, 0xFC, 0x00 },
        // 0x32 - 2
        { 0x78, 0xCC, 0x0C, 0x38, 0x60, 0xCC, 0xFC, 0x00 },
        // 0x33 - 3
        { 0x78, 0xCC, 0x0C, 0x38, 0x0C, 0xCC, 0x78, 0x00 },
        // 0x34 - 4
        { 0x1C, 0x3C, 0x6C, 0xCC, 0xFE, 0x0C, 0x1E, 0x00 },
        // 0x35 - 5
        { 0xFC, 0xC0, 0xF8, 0x0C, 0x0C, 0xCC, 0x78, 0x00 },
        // 0x36 - 6
        { 0x38, 0x60, 0xC0, 0xF8, 0xCC, 0xCC, 0x78, 0x00 },
        // 0x37 - 7
        { 0xFC, 0xCC, 0x0C, 0x18, 0x30, 0x30, 0x30, 0x00 },
        // 0x38 - 8
        { 0x78, 0xCC, 0xCC, 0x78, 0xCC, 0xCC, 0x78, 0x00 },
        // 0x39 - 9
        { 0x78, 0xCC, 0xCC, 0x7C, 0x0C, 0x18, 0x70, 0x00 },
        // 0x3A - :
        { 0x00, 0x30, 0x30, 0x00, 0x00, 0x30, 0x30, 0x00 },
        // 0x3B - ;
        { 0x00, 0x30, 0x30, 0x00, 0x00, 0x30, 0x30, 0x60 },
        // 0x3C - <
        { 0x18, 0x30, 0x60, 0xC0, 0x60, 0x30, 0x18, 0x00 },
        // 0x3D - =
        { 0x00, 0x00, 0xFC, 0x00, 0x00, 0xFC, 0x00, 0x00 },
        // 0x3E - >
        { 0x60, 0x30, 0x18, 0x0C, 0x18, 0x30, 0x60, 0x00 },
        // 0x3F - ?
        { 0x78, 0xCC, 0x0C, 0x18, 0x30, 0x00, 0x30, 0x00 },
        
        // 0x40 - @
        { 0x7C, 0xC6, 0xDE, 0xDE, 0xDE, 0xC0, 0x78, 0x00 },
        // 0x41 - A
        { 0x30, 0x78, 0xCC, 0xCC, 0xFC, 0xCC, 0xCC, 0x00 },
        // 0x42 - B
        { 0xFC, 0x66, 0x66, 0x7C, 0x66, 0x66, 0xFC, 0x00 },
        // 0x43 - C
        { 0x3C, 0x66, 0xC0, 0xC0, 0xC0, 0x66, 0x3C, 0x00 },
        // 0x44 - D
        { 0xF8, 0x6C, 0x66, 0x66, 0x66, 0x6C, 0xF8, 0x00 },
        // 0x45 - E
        { 0xFE, 0x62, 0x68, 0x78, 0x68, 0x62, 0xFE, 0x00 },
        // 0x46 - F
        { 0xFE, 0x62, 0x68, 0x78, 0x68, 0x60, 0xF0, 0x00 },
        // 0x47 - G
        { 0x3C, 0x66, 0xC0, 0xC0, 0xCE, 0x66, 0x3E, 0x00 },
        // 0x48 - H
        { 0xCC, 0xCC, 0xCC, 0xFC, 0xCC, 0xCC, 0xCC, 0x00 },
        // 0x49 - I
        { 0x78, 0x30, 0x30, 0x30, 0x30, 0x30, 0x78, 0x00 },
        // 0x4A - J
        { 0x1E, 0x0C, 0x0C, 0x0C, 0xCC, 0xCC, 0x78, 0x00 },
        // 0x4B - K
        { 0xE6, 0x66, 0x6C, 0x78, 0x6C, 0x66, 0xE6, 0x00 },
        // 0x4C - L
        { 0xF0, 0x60, 0x60, 0x60, 0x62, 0x66, 0xFE, 0x00 },
        // 0x4D - M
        { 0xC6, 0xEE, 0xFE, 0xFE, 0xD6, 0xC6, 0xC6, 0x00 },
        // 0x4E - N
        { 0xC6, 0xE6, 0xF6, 0xDE, 0xCE, 0xC6, 0xC6, 0x00 },
        // 0x4F - O
        { 0x38, 0x6C, 0xC6, 0xC6, 0xC6, 0x6C, 0x38, 0x00 },
        
        // 0x50 - P
        { 0xFC, 0x66, 0x66, 0x7C, 0x60, 0x60, 0xF0, 0x00 },
        // 0x51 - Q
        { 0x78, 0xCC, 0xCC, 0xCC, 0xDC, 0x78, 0x1C, 0x00 },
        // 0x52 - R
        { 0xFC, 0x66, 0x66, 0x7C, 0x6C, 0x66, 0xE6, 0x00 },
        // 0x53 - S
        { 0x78, 0xCC, 0xE0, 0x70, 0x1C, 0xCC, 0x78, 0x00 },
        // 0x54 - T
        { 0xFC, 0xB4, 0x30, 0x30, 0x30, 0x30, 0x78, 0x00 },
        // 0x55 - U
        { 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xFC, 0x00 },
        // 0x56 - V
        { 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0x78, 0x30, 0x00 },
        // 0x57 - W
        { 0xC6, 0xC6, 0xC6, 0xD6, 0xFE, 0xEE, 0xC6, 0x00 },
        // 0x58 - X
        { 0xC6, 0xC6, 0x6C, 0x38, 0x38, 0x6C, 0xC6, 0x00 },
        // 0x59 - Y
        { 0xCC, 0xCC, 0xCC, 0x78, 0x30, 0x30, 0x78, 0x00 },
        // 0x5A - Z
        { 0xFE, 0xC6, 0x8C, 0x18, 0x32, 0x66, 0xFE, 0x00 },
        // 0x5B - [
        { 0x78, 0x60, 0x60, 0x60, 0x60, 0x60, 0x78, 0x00 },
        // 0x5C - \
        { 0xC0, 0x60, 0x30, 0x18, 0x0C, 0x06, 0x02, 0x00 },
        // 0x5D - ]
        { 0x78, 0x18, 0x18, 0x18, 0x18, 0x18, 0x78, 0x00 },
        // 0x5E - ^
        { 0x10, 0x38, 0x6C, 0xC6, 0x00, 0x00, 0x00, 0x00 },
        // 0x5F - _
        { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF },
        
        // 0x60 - `
        { 0x30, 0x30, 0x18, 0x00, 0x00, 0x00, 0x00, 0x00 },
        // 0x61 - a
        { 0x00, 0x00, 0x78, 0x0C, 0x7C, 0xCC, 0x76, 0x00 },
        // 0x62 - b
        { 0xE0, 0x60, 0x60, 0x7C, 0x66, 0x66, 0xDC, 0x00 },
        // 0x63 - c
        { 0x00, 0x00, 0x78, 0xCC, 0xC0, 0xCC, 0x78, 0x00 },
        // 0x64 - d
        { 0x1C, 0x0C, 0x0C, 0x7C, 0xCC, 0xCC, 0x76, 0x00 },
        // 0x65 - e
        { 0x00, 0x00, 0x78, 0xCC, 0xFC, 0xC0, 0x78, 0x00 },
        // 0x66 - f
        { 0x38, 0x6C, 0x60, 0xF0, 0x60, 0x60, 0xF0, 0x00 },
        // 0x67 - g
        { 0x00, 0x00, 0x76, 0xCC, 0xCC, 0x7C, 0x0C, 0xF8 },
        // 0x68 - h
        { 0xE0, 0x60, 0x6C, 0x76, 0x66, 0x66, 0xE6, 0x00 },
        // 0x69 - i
        { 0x30, 0x00, 0x70, 0x30, 0x30, 0x30, 0x78, 0x00 },
        // 0x6A - j
        { 0x0C, 0x00, 0x0C, 0x0C, 0x0C, 0xCC, 0xCC, 0x78 },
        // 0x6B - k
        { 0xE0, 0x60, 0x66, 0x6C, 0x78, 0x6C, 0xE6, 0x00 },
        // 0x6C - l
        { 0x70, 0x30, 0x30, 0x30, 0x30, 0x30, 0x78, 0x00 },
        // 0x6D - m
        { 0x00, 0x00, 0xCC, 0xFE, 0xFE, 0xD6, 0xC6, 0x00 },
        // 0x6E - n
        { 0x00, 0x00, 0xF8, 0xCC, 0xCC, 0xCC, 0xCC, 0x00 },
        // 0x6F - o
        { 0x00, 0x00, 0x78, 0xCC, 0xCC, 0xCC, 0x78, 0x00 },
        
        // 0x70 - p
        { 0x00, 0x00, 0xDC, 0x66, 0x66, 0x7C, 0x60, 0xF0 },
        // 0x71 - q
        { 0x00, 0x00, 0x76, 0xCC, 0xCC, 0x7C, 0x0C, 0x1E },
        // 0x72 - r
        { 0x00, 0x00, 0xDC, 0x76, 0x66, 0x60, 0xF0, 0x00 },
        // 0x73 - s
        { 0x00, 0x00, 0x7C, 0xC0, 0x78, 0x0C, 0xF8, 0x00 },
        // 0x74 - t
        { 0x10, 0x30, 0x7C, 0x30, 0x30, 0x34, 0x18, 0x00 },
        // 0x75 - u
        { 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0xCC, 0x76, 0x00 },
        // 0x76 - v
        { 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0x78, 0x30, 0x00 },
        // 0x77 - w
        { 0x00, 0x00, 0xC6, 0xD6, 0xFE, 0xFE, 0x6C, 0x00 },
        // 0x78 - x
        { 0x00, 0x00, 0xC6, 0x6C, 0x38, 0x6C, 0xC6, 0x00 },
        // 0x79 - y
        { 0x00, 0x00, 0xCC, 0xCC, 0xCC, 0x7C, 0x0C, 0xF8 },
        // 0x7A - z
        { 0x00, 0x00, 0xFC, 0x98, 0x30, 0x64, 0xFC, 0x00 },
        // 0x7B - {
        { 0x1C, 0x30, 0x30, 0xE0, 0x30, 0x30, 0x1C, 0x00 },
        // 0x7C - |
        { 0x18, 0x18, 0x18, 0x00, 0x18, 0x18, 0x18, 0x00 },
        // 0x7D - }
        { 0xE0, 0x30, 0x30, 0x1C, 0x30, 0x30, 0xE0, 0x00 },
        // 0x7E - ~
        { 0x76, 0xDC, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
        // 0x7F - DEL (carr�)
        { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }
    };
    
    /// <summary>
    /// Obtient les donn�es bitmap d'un caract�re
    /// </summary>
    /// <param name="charCode">Code ASCII du caract�re (0-127)</param>
    /// <returns>Tableau de 8 octets repr�sentant le caract�re</returns>
    public static byte[] GetCharacterData(byte charCode)
    {
        if (charCode >= CharCount)
            charCode = 0x7F; // Caract�re par d�faut (carr� plein)
            
        byte[] charData = new byte[CharHeight];
        for (int i = 0; i < CharHeight; i++)
        {
            charData[i] = _fontData[charCode, i];
        }
        return charData;
    }
    
    /// <summary>
    /// Obtient un pixel sp�cifique d'un caract�re
    /// </summary>
    /// <param name="charCode">Code ASCII du caract�re</param>
    /// <param name="x">Position X dans le caract�re (0-7)</param>
    /// <param name="y">Position Y dans le caract�re (0-7)</param>
    /// <returns>true si le pixel est allum�, false sinon</returns>
    public static bool GetPixel(byte charCode, int x, int y)
    {
        if (charCode >= CharCount || x < 0 || x >= CharWidth || y < 0 || y >= CharHeight)
            return false;
            
        byte rowData = _fontData[charCode, y];
        return (rowData & (0x80 >> x)) != 0;
    }
}