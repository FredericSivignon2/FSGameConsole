namespace FSCPU;

/// <summary>
/// Gestionnaire d'appels syst�me (syscalls) pour l'�mulateur
/// Permet aux programmes assembleur d'utiliser des fonctions de haut niveau
/// comme l'affichage, sans recourir aux fonctions C# directes
/// </summary>
public class SystemCallManager
{
    private readonly Memory _memory;
    private readonly Graphics.VideoController _videoController;
    
    // Codes d'appels syst�me (� placer dans le registre A avant SYS)
    public const byte SYSCALL_PRINT_CHAR = 0x01;      // Affiche un caract�re
    public const byte SYSCALL_PRINT_STRING = 0x02;    // Affiche une cha�ne
    public const byte SYSCALL_CLEAR_SCREEN = 0x03;    // Efface l'�cran
    public const byte SYSCALL_SET_CURSOR = 0x04;      // Positionne le curseur
    public const byte SYSCALL_SET_COLOR = 0x05;       // Change les couleurs
    public const byte SYSCALL_GET_CHAR = 0x10;        // Lit un caract�re (clavier)
    public const byte SYSCALL_SET_PIXEL = 0x20;       // Allume un pixel
    public const byte SYSCALL_GET_PIXEL = 0x21;       // Lit un pixel
    
    // �tat du syst�me
    private int _cursorX = 0;
    private int _cursorY = 0;
    private byte _foregroundColor = 15; // Blanc
    private byte _backgroundColor = 0;  // Noir
    
    public SystemCallManager(Memory memory, Graphics.VideoController videoController)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _videoController = videoController ?? throw new ArgumentNullException(nameof(videoController));
    }
    
    /// <summary>
    /// Ex�cute un appel syst�me
    /// </summary>
    /// <param name="cpu">Instance du CPU pour acc�der aux registres</param>
    /// <returns>True si l'appel syst�me a �t� trait�, False sinon</returns>
    public bool ExecuteSystemCall(CPU8Bit cpu)
    {
        byte syscallCode = cpu.A; // Le code d'appel syst�me est dans A
        
        switch (syscallCode)
        {
            case SYSCALL_PRINT_CHAR:
                return PrintChar(cpu);
                
            case SYSCALL_PRINT_STRING:
                return PrintString(cpu);
                
            case SYSCALL_CLEAR_SCREEN:
                return ClearScreen(cpu);
                
            case SYSCALL_SET_CURSOR:
                return SetCursor(cpu);
                
            case SYSCALL_SET_COLOR:
                return SetColor(cpu);
                
            case SYSCALL_SET_PIXEL:
                return SetPixel(cpu);
                
            case SYSCALL_GET_PIXEL:
                return GetPixel(cpu);
                
            default:
                return false; // Appel syst�me non reconnu
        }
    }
    
    /// <summary>
    /// SYSCALL 0x01: Affiche un caract�re � la position du curseur
    /// Entr�e: B = caract�re � afficher
    /// </summary>
    private bool PrintChar(CPU8Bit cpu)
    {
        char character = (char)cpu.B;
        
        // Gestion des caract�res sp�ciaux
        switch (character)
        {
            case '\n': // Nouvelle ligne
                _cursorX = 0;
                _cursorY++;
                break;
                
            case '\r': // Retour chariot
                _cursorX = 0;
                break;
                
            case '\t': // Tabulation
                _cursorX = (_cursorX + 4) & ~3; // Aligner sur 4
                break;
                
            default:
                // Afficher le caract�re normal
                if (character >= 32 && character < 127) // Caract�res imprimables
                {
                    _videoController.DrawTextCharacter(_cursorX, _cursorY, (byte)character, 
                        _foregroundColor, _backgroundColor);
                    _cursorX++;
                }
                break;
        }
        
        // Gestion du d�bordement
        if (_cursorX >= Graphics.VideoController.CharsPerLine)
        {
            _cursorX = 0;
            _cursorY++;
        }
        
        if (_cursorY >= Graphics.VideoController.LinesOfText)
        {
            // Scroll simple : effacer l'�cran et remettre en haut
            _cursorY = 0;
            _videoController.ClearScreen(_backgroundColor);
        }
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x02: Affiche une cha�ne de caract�res
    /// Entr�e: BC = adresse de la cha�ne (B=high, C=low), D = longueur max
    /// </summary>
    private bool PrintString(CPU8Bit cpu)
    {
        ushort stringAddress = (ushort)((cpu.B << 8) | cpu.C);
        int maxLength = cpu.D;
        
        for (int i = 0; i < maxLength; i++)
        {
            byte charByte = _memory.ReadByte((ushort)(stringAddress + i));
            
            // Arr�ter sur le caract�re nul
            if (charByte == 0)
                break;
                
            // Utiliser PrintChar pour chaque caract�re
            cpu.B = charByte;
            PrintChar(cpu);
        }
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x03: Efface l'�cran
    /// Entr�e: B = couleur de fond (optionnel)
    /// </summary>
    private bool ClearScreen(CPU8Bit cpu)
    {
        byte bgColor = cpu.B != 0 ? cpu.B : _backgroundColor;
        _videoController.ClearScreen(bgColor);
        
        _cursorX = 0;
        _cursorY = 0;
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x04: Positionne le curseur
    /// Entr�e: B = X, C = Y
    /// </summary>
    private bool SetCursor(CPU8Bit cpu)
    {
        _cursorX = Math.Min((int)cpu.B, Graphics.VideoController.CharsPerLine - 1);
        _cursorY = Math.Min((int)cpu.C, Graphics.VideoController.LinesOfText - 1);
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x05: Change les couleurs
    /// Entr�e: B = couleur de premier plan, C = couleur de fond
    /// </summary>
    private bool SetColor(CPU8Bit cpu)
    {
        _foregroundColor = (byte)(cpu.B & 0x0F);
        _backgroundColor = (byte)(cpu.C & 0x0F);
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x20: Allume un pixel
    /// Entr�e: BC = X (B=high, C=low), D = Y, A apr�s syscall = couleur
    /// </summary>
    private bool SetPixel(CPU8Bit cpu)
    {
        int x = (cpu.B << 8) | cpu.C;
        int y = cpu.D;
        // R�utiliser A qui contenait le code syscall, maintenant pour la couleur
        byte color = cpu.A;
        
        _videoController.SetPixel(x, y, color);
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x21: Lit un pixel
    /// Entr�e: BC = X (B=high, C=low), D = Y
    /// Sortie: A = couleur du pixel
    /// </summary>
    private bool GetPixel(CPU8Bit cpu)
    {
        int x = (cpu.B << 8) | cpu.C;
        int y = cpu.D;
        
        byte color = _videoController.GetPixel(x, y);
        cpu.A = color;
        
        return true;
    }
    
    /// <summary>
    /// Obtient la position actuelle du curseur
    /// </summary>
    public (int X, int Y) GetCursorPosition() => (_cursorX, _cursorY);
    
    /// <summary>
    /// Obtient les couleurs actuelles
    /// </summary>
    public (byte Foreground, byte Background) GetColors() => (_foregroundColor, _backgroundColor);
    
    /// <summary>
    /// Informations de debug sur le syst�me d'appels
    /// </summary>
    public string GetSystemCallInfo()
    {
        return $"System Call Manager:\n" +
               $"  Curseur: ({_cursorX}, {_cursorY})\n" +
               $"  Couleurs: FG={_foregroundColor}, BG={_backgroundColor}\n" +
               $"  Appels disponibles: 0x01-0x05, 0x10, 0x20-0x21";
    }
}