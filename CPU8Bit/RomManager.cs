namespace CPU8Bit;

/// <summary>
/// Gestionnaire de ROM pour le système de BOOT
/// Version authentique avec appels système pour l'affichage
/// Plus de code C# pour l'affichage - tout passe par l'assembleur !
/// </summary>
public class RomManager
{
    /// <summary>
    /// Adresse de début de la ROM BOOT
    /// </summary>
    public const ushort ROM_BOOT_START = 0xF400;
    
    /// <summary>
    /// Taille de la ROM BOOT (1KB)
    /// </summary>
    public const int ROM_BOOT_SIZE = 0x400;
    
    /// <summary>
    /// Adresse de fin de la ROM BOOT
    /// </summary>
    public const ushort ROM_BOOT_END = ROM_BOOT_START + ROM_BOOT_SIZE - 1;
    
    /// <summary>
    /// Programme de démarrage intégré en ROM - Version authentique avec syscalls
    /// Cette ROM utilise exclusivement les appels système pour l'affichage
    /// AUCUN code C# direct - tout est authentique !
    /// </summary>
    private static readonly byte[] _bootRom = new byte[]
    {
        // === PROGRAMME DE BOOT ROM AUTHENTIQUE ===
        // Utilise uniquement les appels système (SYS) pour l'affichage
        
        // Effacer l'écran en noir
        0x10, 0x03,        // LDA #3 (SYSCALL_CLEAR_SCREEN)
        0x11, 0x00,        // LDB #0 (couleur noire)
        0xF0,              // SYS (appel système)
        
        // Définir les couleurs : jaune sur noir
        0x10, 0x05,        // LDA #5 (SYSCALL_SET_COLOR)
        0x11, 0x0E,        // LDB #14 (jaune)
        0x12, 0x00,        // LDC #0 (noir)
        0xF0,              // SYS
        
        // Positionner le curseur en (0,0)
        0x10, 0x04,        // LDA #4 (SYSCALL_SET_CURSOR)
        0x11, 0x00,        // LDB #0 (X=0)
        0x12, 0x00,        // LDC #0 (Y=0)
        0xF0,              // SYS
        
        // Afficher "FSGameConsole BOOT" caractère par caractère
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x46,        // LDB #'F'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x53,        // LDB #'S'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x47,        // LDB #'G'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x61,        // LDB #'a'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x6D,        // LDB #'m'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x65,        // LDB #'e'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x43,        // LDB #'C'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x6F,        // LDB #'o'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x6E,        // LDB #'n'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x73,        // LDB #'s'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x6F,        // LDB #'o'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x6C,        // LDB #'l'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x65,        // LDB #'e'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x20,        // LDB #' '
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x42,        // LDB #'B'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x4F,        // LDB #'O'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x4F,        // LDB #'O'
        0xF0,              // SYS
        
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x54,        // LDB #'T'
        0xF0,              // SYS
        
        // Nouvelle ligne
        0x10, 0x01,        // LDA #1 (SYSCALL_PRINT_CHAR)
        0x11, 0x0A,        // LDB #'\n'
        0xF0,              // SYS
        
        // Changer couleur pour vert
        0x10, 0x05,        // LDA #5 (SYSCALL_SET_COLOR)
        0x11, 0x0A,        // LDB #10 (vert clair)
        0x12, 0x00,        // LDC #0 (noir)
        0xF0,              // SYS
        
        // Afficher "ROM v3.0 - Timing authentique"
        0x10, 0x01, 0x11, 0x52, 0xF0,  // 'R'
        0x10, 0x01, 0x11, 0x4F, 0xF0,  // 'O'
        0x10, 0x01, 0x11, 0x4D, 0xF0,  // 'M'
        0x10, 0x01, 0x11, 0x20, 0xF0,  // ' '
        0x10, 0x01, 0x11, 0x76, 0xF0,  // 'v'
        0x10, 0x01, 0x11, 0x33, 0xF0,  // '3'
        0x10, 0x01, 0x11, 0x2E, 0xF0,  // '.'
        0x10, 0x01, 0x11, 0x30, 0xF0,  // '0'
        0x10, 0x01, 0x11, 0x20, 0xF0,  // ' '
        0x10, 0x01, 0x11, 0x2D, 0xF0,  // '-'
        0x10, 0x01, 0x11, 0x20, 0xF0,  // ' '
        0x10, 0x01, 0x11, 0x54, 0xF0,  // 'T'
        0x10, 0x01, 0x11, 0x69, 0xF0,  // 'i'
        0x10, 0x01, 0x11, 0x6D, 0xF0,  // 'm'
        0x10, 0x01, 0x11, 0x69, 0xF0,  // 'i'
        0x10, 0x01, 0x11, 0x6E, 0xF0,  // 'n'
        0x10, 0x01, 0x11, 0x67, 0xF0,  // 'g'
        0x10, 0x01, 0x11, 0x20, 0xF0,  // ' '
        0x10, 0x01, 0x11, 0x61, 0xF0,  // 'a'
        0x10, 0x01, 0x11, 0x75, 0xF0,  // 'u'
        0x10, 0x01, 0x11, 0x74, 0xF0,  // 't'
        0x10, 0x01, 0x11, 0x68, 0xF0,  // 'h'
        0x10, 0x01, 0x11, 0x65, 0xF0,  // 'e'
        0x10, 0x01, 0x11, 0x6E, 0xF0,  // 'n'
        0x10, 0x01, 0x11, 0x74, 0xF0,  // 't'
        0x10, 0x01, 0x11, 0x69, 0xF0,  // 'i'
        0x10, 0x01, 0x11, 0x71, 0xF0,  // 'q'
        0x10, 0x01, 0x11, 0x75, 0xF0,  // 'u'
        0x10, 0x01, 0x11, 0x65, 0xF0,  // 'e'
        
        // Double nouvelle ligne
        0x10, 0x01, 0x11, 0x0A, 0xF0,  // '\n'
        0x10, 0x01, 0x11, 0x0A, 0xF0,  // '\n'
        
        // Changer couleur pour blanc
        0x10, 0x05,        // LDA #5 (SYSCALL_SET_COLOR)
        0x11, 0x0F,        // LDB #15 (blanc)
        0x12, 0x00,        // LDC #0 (noir)
        0xF0,              // SYS
        
        // "Ready..."
        0x10, 0x01, 0x11, 0x52, 0xF0,  // 'R'
        0x10, 0x01, 0x11, 0x65, 0xF0,  // 'e'
        0x10, 0x01, 0x11, 0x61, 0xF0,  // 'a'
        0x10, 0x01, 0x11, 0x64, 0xF0,  // 'd'
        0x10, 0x01, 0x11, 0x79, 0xF0,  // 'y'
        0x10, 0x01, 0x11, 0x2E, 0xF0,  // '.'
        0x10, 0x01, 0x11, 0x2E, 0xF0,  // '.'
        0x10, 0x01, 0x11, 0x2E, 0xF0,  // '.'
        
        // Charger un HALT par défaut en 0x0000
        0x10, 0x01,        // LDA #0x01 (HALT)
        0x50, 0x00, 0x00,  // STA $0000
        
        // La ROM se termine ici - 100% authentique !
        0x01               // HALT - Terminer l'exécution de la ROM
    };
    
    /// <summary>
    /// Obtient les données de la ROM de boot
    /// </summary>
    /// <returns>Données de la ROM</returns>
    public static byte[] GetBootRom()
    {
        // Créer un tableau de la taille complète de la ROM
        byte[] fullRom = new byte[ROM_BOOT_SIZE];
        
        // Copier le programme de boot au début
        Array.Copy(_bootRom, 0, fullRom, 0, Math.Min(_bootRom.Length, ROM_BOOT_SIZE));
        
        return fullRom;
    }
    
    /// <summary>
    /// Vérifie si une adresse fait partie de la ROM
    /// </summary>
    /// <param name="address">Adresse à vérifier</param>
    /// <returns>True si l'adresse est dans la ROM</returns>
    public static bool IsRomAddress(ushort address)
    {
        return address >= ROM_BOOT_START && address <= ROM_BOOT_END;
    }
    
    /// <summary>
    /// Lit un octet depuis la ROM
    /// </summary>
    /// <param name="address">Adresse dans la ROM</param>
    /// <returns>Octet lu</returns>
    public static byte ReadRomByte(ushort address)
    {
        if (!IsRomAddress(address))
            throw new ArgumentOutOfRangeException(nameof(address), "Adresse hors de la ROM");
            
        int offset = address - ROM_BOOT_START;
        return offset < _bootRom.Length ? _bootRom[offset] : (byte)0x00;
    }
    
    /// <summary>
    /// Obtient des informations sur la ROM
    /// </summary>
    /// <returns>Informations de debug</returns>
    public static string GetRomInfo()
    {
        return $"ROM BOOT Authentique avec Syscalls:\n" +
               $"  Adresse: 0x{ROM_BOOT_START:X4} - 0x{ROM_BOOT_END:X4}\n" +
               $"  Taille: {ROM_BOOT_SIZE} octets\n" +
               $"  Programme: {_bootRom.Length} octets utilisés\n" +
               $"  Version: FSGameConsole Boot ROM v3.0\n" +
               $"  Caractéristiques: Timing authentique + Syscalls purs\n" +
               $"  AUCUN code C# direct - 100% assembleur !";
    }
}