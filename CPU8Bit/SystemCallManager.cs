namespace FSCPU;

/// <summary>
/// Gestionnaire d'appels système (syscalls) pour l'émulateur
/// Permet aux programmes assembleur d'utiliser des fonctions de haut niveau
/// comme l'affichage, sans recourir aux fonctions C# directes
/// </summary>
public class SystemCallManager
{
    private readonly Memory _memory;
    private readonly Graphics.VideoController _videoController;
    
    // Codes d'appels système (à placer dans le registre A avant SYS)
    public const byte SYSCALL_PRINT_CHAR = 0x01;      // Affiche un caractère
    public const byte SYSCALL_PRINT_STRING = 0x02;    // Affiche une chaîne
    public const byte SYSCALL_CLEAR_SCREEN = 0x03;    // Efface l'écran
    public const byte SYSCALL_SET_CURSOR = 0x04;      // Positionne le curseur
    public const byte SYSCALL_SET_COLOR = 0x05;       // Change les couleurs
    public const byte SYSCALL_GET_CHAR = 0x10;        // Lit un caractère (clavier)
    public const byte SYSCALL_SET_PIXEL = 0x20;       // Allume un pixel
    public const byte SYSCALL_GET_PIXEL = 0x21;       // Lit un pixel
    
    // État du système
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
    /// Exécute un appel système
    /// </summary>
    /// <param name="cpu">Instance du CPU pour accéder aux registres</param>
    /// <returns>True si l'appel système a été traité, False sinon</returns>
    public bool ExecuteSystemCall(CPU8Bit cpu)
    {
        byte syscallCode = cpu.A; // Le code d'appel système est dans A
        
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
                return false; // Appel système non reconnu
        }
    }
    
    /// <summary>
    /// SYSCALL 0x01: Affiche un caractère à la position du curseur
    /// Entrée: B = caractère à afficher
    /// </summary>
    private bool PrintChar(CPU8Bit cpu)
    {
        char character = (char)cpu.B;
        
        // Gestion des caractères spéciaux
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
                // Afficher le caractère normal
                if (character >= 32 && character < 127) // Caractères imprimables
                {
                    _videoController.DrawTextCharacter(_cursorX, _cursorY, (byte)character, 
                        _foregroundColor, _backgroundColor);
                    _cursorX++;
                }
                break;
        }
        
        // Gestion du débordement
        if (_cursorX >= Graphics.VideoController.CharsPerLine)
        {
            _cursorX = 0;
            _cursorY++;
        }
        
        if (_cursorY >= Graphics.VideoController.LinesOfText)
        {
            // Scroll simple : effacer l'écran et remettre en haut
            _cursorY = 0;
            _videoController.ClearScreen(_backgroundColor);
        }
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x02: Affiche une chaîne de caractères
    /// Entrée: BC = adresse de la chaîne (B=high, C=low), D = longueur max
    /// </summary>
    private bool PrintString(CPU8Bit cpu)
    {
        ushort stringAddress = (ushort)((cpu.B << 8) | cpu.C);
        int maxLength = cpu.D;
        
        for (int i = 0; i < maxLength; i++)
        {
            byte charByte = _memory.ReadByte((ushort)(stringAddress + i));
            
            // Arrêter sur le caractère nul
            if (charByte == 0)
                break;
                
            // Utiliser PrintChar pour chaque caractère
            cpu.B = charByte;
            PrintChar(cpu);
        }
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x03: Efface l'écran
    /// Entrée: B = couleur de fond (optionnel)
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
    /// Entrée: B = X, C = Y
    /// </summary>
    private bool SetCursor(CPU8Bit cpu)
    {
        _cursorX = Math.Min((int)cpu.B, Graphics.VideoController.CharsPerLine - 1);
        _cursorY = Math.Min((int)cpu.C, Graphics.VideoController.LinesOfText - 1);
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x05: Change les couleurs
    /// Entrée: B = couleur de premier plan, C = couleur de fond
    /// </summary>
    private bool SetColor(CPU8Bit cpu)
    {
        _foregroundColor = (byte)(cpu.B & 0x0F);
        _backgroundColor = (byte)(cpu.C & 0x0F);
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x20: Allume un pixel
    /// Entrée: BC = X (B=high, C=low), D = Y, A après syscall = couleur
    /// </summary>
    private bool SetPixel(CPU8Bit cpu)
    {
        int x = (cpu.B << 8) | cpu.C;
        int y = cpu.D;
        // Réutiliser A qui contenait le code syscall, maintenant pour la couleur
        byte color = cpu.A;
        
        _videoController.SetPixel(x, y, color);
        
        return true;
    }
    
    /// <summary>
    /// SYSCALL 0x21: Lit un pixel
    /// Entrée: BC = X (B=high, C=low), D = Y
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
    /// Informations de debug sur le système d'appels
    /// </summary>
    public string GetSystemCallInfo()
    {
        return $"System Call Manager:\n" +
               $"  Curseur: ({_cursorX}, {_cursorY})\n" +
               $"  Couleurs: FG={_foregroundColor}, BG={_backgroundColor}\n" +
               $"  Appels disponibles: 0x01-0x05, 0x10, 0x20-0x21";
    }
}