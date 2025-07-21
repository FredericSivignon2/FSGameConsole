namespace FSCPU;

/// <summary>
/// Classe repr�sentant la m�moire du syst�me (RAM/ROM)
/// Organisation authentique style Amstrad CPC:
/// 0x0000-0x7FFF : RAM programme (32KB)
/// 0x8000-0xBFFF : M�moire vid�o bitmap unifi�e (16KB) ? Style CPC authentique
/// 0xC000-0xF3FF : RAM �tendue (13KB)
/// 0xF400-0xF7FF : ROM BOOT (1KB) ? **ZONE ROM PROT�G�E**
/// 0xF800-0xFFFF : BIOS/Syst�me (2KB)
/// </summary>
public class Memory
{
    private readonly byte[] _memory;
    private readonly int _size;
    private readonly byte[] _bootRom;
    
    // Zones m�moire pour le mode CPC authentique
    public int BitmapMemoryStart { get; }
    public int BitmapMemorySize { get; }
    public int StackStart { get; }
    
    // Zone ROM
    public int RomStart => RomManager.ROM_BOOT_START;
    public int RomSize => RomManager.ROM_BOOT_SIZE;
    public int RomEnd => RomManager.ROM_BOOT_END;
    
    // Propri�t�s obsol�tes pour compatibilit� avec les tests existants
    [Obsolete("Plus de m�moire texte s�par�e - utiliser directement la m�moire bitmap")]
    public int VideoMemoryStart => BitmapMemoryStart;
    
    [Obsolete("Plus de m�moire texte s�par�e - utiliser directement la m�moire bitmap")]
    public int VideoMemorySize => 2000; // Valeur fictive pour compatibilit� tests
    
    // Suppression compl�te des propri�t�s de m�moire texte
    // Plus de TextMemoryStart ni TextMemorySize !
    
    public Memory(int size = 0x10000) // 64KB par d�faut
    {
        _size = size;
        _memory = new byte[size];
        
        // Charger la ROM de boot
        _bootRom = RomManager.GetBootRom();
        
        // Configuration CPC authentique - m�moire vid�o bitmap unifi�e
        // Zone bitmap unifi�e style CPC : 16KB � partir de 0x8000
        BitmapMemoryStart = 0x8000;
        BitmapMemorySize = 0x4000; // 16KB (comme un vrai CPC)
        
        // Zone de pile : avant la fin de la m�moire
        StackStart = 0xFFFF;
        
        // Initialiser avec la ROM
        LoadBootRom();
    }
    
    /// <summary>
    /// Charge la ROM de boot dans la zone m�moire appropri�e
    /// </summary>
    private void LoadBootRom()
    {
        // Copier la ROM dans la zone m�moire correspondante
        for (int i = 0; i < _bootRom.Length && (RomStart + i) < _size; i++)
        {
            _memory[RomStart + i] = _bootRom[i];
        }
    }
    
    /// <summary>
    /// Taille totale de la m�moire
    /// </summary>
    public int Size => _size;
    
    /// <summary>
    /// Lit un octet � l'adresse sp�cifi�e
    /// </summary>
    public byte ReadByte(ushort address)
    {
        if (address >= _size)
            throw new ArgumentOutOfRangeException(nameof(address), $"Adresse {address:X4} hors limites");
        
        // Si c'est une adresse ROM, lire depuis la ROM (toujours fra�che)
        if (RomManager.IsRomAddress(address))
        {
            return RomManager.ReadRomByte(address);
        }
        
        return _memory[address];
    }
    
    /// <summary>
    /// �crit un octet � l'adresse sp�cifi�e
    /// </summary>
    public void WriteByte(ushort address, byte value)
    {
        if (address >= _size)
            throw new ArgumentOutOfRangeException(nameof(address), $"Adresse {address:X4} hors limites");
        
        // Protection contre l'�criture en ROM
        if (RomManager.IsRomAddress(address))
        {
            // Ignorer silencieusement les tentatives d'�criture en ROM
            // (comme le feraient de vrais circuits de protection ROM)
            return;
        }
        
        _memory[address] = value;
    }
    
    /// <summary>
    /// Lit un mot (16 bits) � l'adresse sp�cifi�e (little-endian)
    /// </summary>
    public ushort ReadWord(ushort address)
    {
        byte low = ReadByte(address);
        byte high = ReadByte((ushort)(address + 1));
        return (ushort)(low | (high << 8));
    }
    
    /// <summary>
    /// �crit un mot (16 bits) � l'adresse sp�cifi�e (little-endian)
    /// </summary>
    public void WriteWord(ushort address, ushort value)
    {
        WriteByte(address, (byte)(value & 0xFF));
        WriteByte((ushort)(address + 1), (byte)(value >> 8));
    }
    
    /// <summary>
    /// Charge un programme en m�moire � partir de l'adresse 0
    /// </summary>
    public void LoadProgram(byte[] program, ushort startAddress = 0)
    {
        if (program == null)
            throw new ArgumentNullException(nameof(program));
        
        if (startAddress + program.Length > _size)
            throw new ArgumentException("Le programme est trop grand pour la m�moire");
        
        // V�rifier qu'on n'�crase pas la ROM
        ushort endAddress = (ushort)(startAddress + program.Length - 1);
        if (startAddress <= RomEnd && endAddress >= RomStart)
        {
            throw new ArgumentException("Le programme ne peut pas �craser la zone ROM");
        }
        
        Array.Copy(program, 0, _memory, startAddress, program.Length);
    }
    
    /// <summary>
    /// Efface toute la m�moire RAM (mais pas la ROM)
    /// </summary>
    public void Clear()
    {
        // Effacer toute la m�moire
        Array.Clear(_memory, 0, _size);
        
        // Recharger la ROM qui a �t� effac�e
        LoadBootRom();
    }
    
    /// <summary>
    /// Reboot complet du syst�me - comme un vrai reset mat�riel
    /// </summary>
    public void Reboot()
    {
        // Effacer compl�tement la m�moire
        Array.Clear(_memory, 0, _size);
        
        // Recharger la ROM
        LoadBootRom();
    }
    
    /// <summary>
    /// Obtient une copie d'une section de la m�moire
    /// </summary>
    public byte[] GetMemorySection(ushort startAddress, int length)
    {
        if (startAddress + length > _size)
            throw new ArgumentOutOfRangeException("Section demand�e hors limites");
        
        byte[] section = new byte[length];
        for (int i = 0; i < length; i++)
        {
            section[i] = ReadByte((ushort)(startAddress + i));
        }
        return section;
    }
    
    /// <summary>
    /// Obtient la zone m�moire bitmap pour l'affichage
    /// </summary>
    public byte[] GetBitmapMemory()
    {
        return GetMemorySection((ushort)BitmapMemoryStart, BitmapMemorySize);
    }
    
    /// <summary>
    /// Obtient la zone ROM
    /// </summary>
    public byte[] GetRomMemory()
    {
        return GetMemorySection((ushort)RomStart, RomSize);
    }
    
    /// <summary>
    /// Obtient la zone m�moire vid�o (compatibilit� avec l'ancien syst�me)
    /// MAINTENANT POINTE VERS LA M�MOIRE BITMAP UNIFI�E
    /// </summary>
    [Obsolete("Utiliser GetBitmapMemory() � la place")]
    public byte[] GetVideoMemory()
    {
        // Pour la compatibilit� avec les tests, on retourne une section 
        // de la m�moire bitmap qui pourrait contenir du "texte virtuel"
        return GetMemorySection((ushort)BitmapMemoryStart, Math.Min(2000, BitmapMemorySize));
    }
    
    /// <summary>
    /// M�thode obsol�te - plus de zone texte s�par�e dans l'approche CPC authentique
    /// </summary>
    [Obsolete("Plus de m�moire texte s�par�e - utiliser VideoController.DrawText() directement")]
    public void WriteTextToTextMemory(string text, int x = 0, int y = 0, int screenWidth = 40)
    {
        // Ne fait plus rien - les programmes doivent utiliser VideoController directement
        // ou �crire directement dans la m�moire bitmap
    }
    
    /// <summary>
    /// M�thode obsol�te pour compatibilit� avec les tests
    /// </summary>
    [Obsolete("Plus de m�moire texte s�par�e - utiliser VideoController.DrawText() directement")]
    public void WriteTextToVideo(string text, int x = 0, int y = 0, int screenWidth = 80)
    {
        // Pour la compatibilit� avec les tests uniquement
        // �crire directement dans la m�moire bitmap comme le ferait un vrai CPC
        if (string.IsNullOrEmpty(text)) return;
        
        int address = BitmapMemoryStart + (y * screenWidth) + x;
        for (int i = 0; i < text.Length && address < BitmapMemoryStart + BitmapMemorySize; i++, address++)
        {
            WriteByte((ushort)address, (byte)text[i]);
        }
    }
    
    /// <summary>
    /// �crit un pixel dans la m�moire bitmap (style CPC authentique)
    /// </summary>
    /// <param name="x">Position X (0-319)</param>
    /// <param name="y">Position Y (0-199)</param>
    /// <param name="colorIndex">Index de couleur (0-15)</param>
    public void SetPixel(int x, int y, byte colorIndex)
    {
        if (x < 0 || x >= 320 || y < 0 || y >= 200)
            return;
            
        colorIndex &= 0x0F; // Limiter � 16 couleurs
        
        int pixelIndex = y * 320 + x;
        int byteIndex = pixelIndex / 2;
        bool isHighNibble = (pixelIndex % 2) == 0;
        
        if (byteIndex < BitmapMemorySize)
        {
            ushort address = (ushort)(BitmapMemoryStart + byteIndex);
            byte currentByte = ReadByte(address);
            
            if (isHighNibble)
            {
                currentByte = (byte)((currentByte & 0x0F) | (colorIndex << 4));
            }
            else
            {
                currentByte = (byte)((currentByte & 0xF0) | colorIndex);
            }
            
            WriteByte(address, currentByte);
        }
    }
    
    /// <summary>
    /// Lit un pixel depuis la m�moire bitmap
    /// </summary>
    /// <param name="x">Position X (0-319)</param>
    /// <param name="y">Position Y (0-199)</param>
    /// <returns>Index de couleur (0-15)</returns>
    public byte GetPixel(int x, int y)
    {
        if (x < 0 || x >= 320 || y < 0 || y >= 200)
            return 0;
            
        int pixelIndex = y * 320 + x;
        int byteIndex = pixelIndex / 2;
        bool isHighNibble = (pixelIndex % 2) == 0;
        
        if (byteIndex < BitmapMemorySize)
        {
            ushort address = (ushort)(BitmapMemoryStart + byteIndex);
            byte currentByte = ReadByte(address);
            
            if (isHighNibble)
            {
                return (byte)((currentByte & 0xF0) >> 4);
            }
            else
            {
                return (byte)(currentByte & 0x0F);
            }
        }
        
        return 0;
    }
    
    /// <summary>
    /// Efface la m�moire bitmap avec une couleur
    /// </summary>
    /// <param name="colorIndex">Index de couleur (0-15)</param>
    public void ClearBitmap(byte colorIndex = 0)
    {
        colorIndex &= 0x0F;
        byte fillValue = (byte)((colorIndex << 4) | colorIndex);
        
        for (int i = 0; i < BitmapMemorySize; i++)
        {
            WriteByte((ushort)(BitmapMemoryStart + i), fillValue);
        }
    }
    
    /// <summary>
    /// Obtient une repr�sentation textuelle d'une zone m�moire pour debug
    /// </summary>
    public string GetMemoryDump(ushort startAddress, int length)
    {
        var sb = new System.Text.StringBuilder();
        
        for (int i = 0; i < length; i += 16)
        {
            sb.AppendFormat("{0:X4}: ", startAddress + i);
            
            // Affichage hexad�cimal
            for (int j = 0; j < 16 && i + j < length; j++)
            {
                sb.AppendFormat("{0:X2} ", ReadByte((ushort)(startAddress + i + j)));
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Obtient des informations sur l'organisation m�moire CPC authentique
    /// </summary>
    public string GetMemoryLayout()
    {
        return $"Organisation m�moire style Amstrad CPC (Taille: {_size:X4}):\n" +
               $"0x0000-0x7FFF : RAM Programme (32KB)\n" +
               $"0x{BitmapMemoryStart:X4}-0x{BitmapMemoryStart + BitmapMemorySize - 1:X4} : M�moire Vid�o Bitmap Unifi�e ({BitmapMemorySize} octets)\n" +
               $"0x{BitmapMemoryStart + BitmapMemorySize:X4}-0x{RomStart - 1:X4} : RAM �tendue\n" +
               $"0x{RomStart:X4}-0x{RomEnd:X4} : ROM BOOT ({RomSize} octets) [PROT�G�E]\n" +
               $"0x{RomEnd + 1:X4}-0xFFFF : BIOS/Pile";
    }
}