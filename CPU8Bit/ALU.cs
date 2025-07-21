namespace FSCPU;

/// <summary>
/// Unité Arithmétique et Logique du processeur 8 bits
/// </summary>
public class ALU
{
    private readonly CPU8Bit _cpu;
    
    public ALU(CPU8Bit cpu)
    {
        _cpu = cpu ?? throw new ArgumentNullException(nameof(cpu));
    }
    
    /// <summary>
    /// Addition de deux valeurs 8 bits
    /// </summary>
    public void Add(ref byte destination, byte source)
    {
        int result = destination + source;
        destination = (byte)(result & 0xFF);
        
        // Mise à jour des flags
        _cpu.SR.UpdateFlags(result);
        
        // Overflow flag spécial pour l'addition signée
        bool overflow = ((destination ^ source) & 0x80) == 0 && 
                       ((destination ^ (byte)result) & 0x80) != 0;
        _cpu.SR.SetOverflowFlag(overflow);
    }
    
    /// <summary>
    /// Soustraction de deux valeurs 8 bits
    /// </summary>
    public void Subtract(ref byte destination, byte source)
    {
        int result = destination - source;
        destination = (byte)(result & 0xFF);
        
        // Mise à jour des flags
        _cpu.SR.UpdateFlags(result);
        
        // Pour la soustraction, le carry flag indique un emprunt
        _cpu.SR.SetCarryFlag(result < 0);
    }
    
    /// <summary>
    /// Opération AND logique
    /// </summary>
    public void And(ref byte destination, byte source)
    {
        destination &= source;
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.UpdateNegativeFlag(destination);
        _cpu.SR.SetCarryFlag(false); // AND efface toujours le carry
    }
    
    /// <summary>
    /// Opération OR logique
    /// </summary>
    public void Or(ref byte destination, byte source)
    {
        destination |= source;
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.UpdateNegativeFlag(destination);
        _cpu.SR.SetCarryFlag(false); // OR efface toujours le carry
    }
    
    /// <summary>
    /// Opération XOR logique
    /// </summary>
    public void Xor(ref byte destination, byte source)
    {
        destination ^= source;
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.UpdateNegativeFlag(destination);
        _cpu.SR.SetCarryFlag(false); // XOR efface toujours le carry
    }
    
    /// <summary>
    /// Décalage à gauche (Shift Left)
    /// </summary>
    public void ShiftLeft(ref byte value)
    {
        bool carryOut = (value & 0x80) != 0;
        value <<= 1;
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.UpdateNegativeFlag(value);
        _cpu.SR.SetCarryFlag(carryOut);
    }
    
    /// <summary>
    /// Décalage à droite (Shift Right)
    /// </summary>
    public void ShiftRight(ref byte value)
    {
        bool carryOut = (value & 0x01) != 0;
        value >>= 1;
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.UpdateNegativeFlag(value);
        _cpu.SR.SetCarryFlag(carryOut);
    }
    
    /// <summary>
    /// Incrémentation d'une valeur
    /// </summary>
    public void Increment(ref byte value)
    {
        int result = value + 1;
        value = (byte)(result & 0xFF);
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.UpdateNegativeFlag(value);
        _cpu.SR.SetCarryFlag(result > 0xFF);
    }
    
    /// <summary>
    /// Décrémentation d'une valeur
    /// </summary>
    public void Decrement(ref byte value)
    {
        int result = value - 1;
        value = (byte)(result & 0xFF);
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.UpdateNegativeFlag(value);
        _cpu.SR.SetCarryFlag(result < 0);
    }
    
    /// <summary>
    /// Compare deux valeurs (soustraction sans stocker le résultat)
    /// </summary>
    public void Compare(byte value1, byte value2)
    {
        int result = value1 - value2;
        
        _cpu.SR.UpdateZeroFlag((byte)(result & 0xFF));
        _cpu.SR.UpdateNegativeFlag((byte)(result & 0xFF));
        _cpu.SR.SetCarryFlag(result < 0);
    }

    /// <summary>
    /// Compare deux valeurs 16 bits (soustraction sans stocker le résultat)
    /// </summary>
    public void Compare(ushort value1, ushort value2)
    {
        int result = value1 - value2;

        // Pour la comparaison 16 bits, vérifie si le résultat complet est zéro
        _cpu.SR.UpdateZeroFlag((ushort)(result & 0xFFFF));
        
        // Pour les valeurs 16 bits, le flag négatif est déterminé par le bit 15 du résultat
        // MAIS : Pour une comparaison non signée, on vérifie le signe réel du résultat, pas le motif de bits
        _cpu.SR.SetCarryFlag(result < 0);  // Le carry indique un emprunt (résultat < 0)
        
        // Pour le flag négatif dans la comparaison non signée 16 bits :
        // Un résultat est "négatif" s'il serait négatif dans un contexte signé
        // Cela signifie : le bit 15 du résultat 16 bits doit être vérifié
        bool bit15Set = ((result & 0x8000) != 0);
        
        // Crée un byte fictif où le bit 7 représente l'état de notre bit 15
        byte fakeByte = bit15Set ? (byte)0x80 : (byte)0x00;
        _cpu.SR.UpdateNegativeFlag(fakeByte);
    }

    // === OPÉRATIONS 16-BITS POUR REGISTRES DA/DB ===

    /// <summary>
    /// Addition de deux valeurs 16 bits
    /// </summary>
    public void Add16(ref ushort destination, ushort source)
    {
        int result = destination + source;
        destination = (ushort)(result & 0xFFFF);
        
        // Mise à jour des flags basé sur le résultat 16 bits
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.SetCarryFlag(result > 0xFFFF);
        _cpu.SR.UpdateNegativeFlag((byte)((destination >> 8) & 0xFF)); // Le byte haut détermine le signe
    }
    
    /// <summary>
    /// Soustraction de deux valeurs 16 bits
    /// </summary>
    public void Subtract16(ref ushort destination, ushort source)
    {
        int result = destination - source;
        destination = (ushort)(result & 0xFFFF);
        
        // Mise à jour des flags basé sur le résultat 16 bits
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.SetCarryFlag(result < 0);
        _cpu.SR.UpdateNegativeFlag((byte)((destination >> 8) & 0xFF)); // Le byte haut détermine le signe
    }
    
    /// <summary>
    /// Incrémentation d'une valeur 16 bits
    /// </summary>
    public void Increment16(ref ushort value)
    {
        int result = value + 1;
        value = (ushort)(result & 0xFFFF);
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.SetCarryFlag(result > 0xFFFF);
        _cpu.SR.UpdateNegativeFlag((byte)((value >> 8) & 0xFF)); // Le byte haut détermine le signe
    }
    
    /// <summary>
    /// Décrémentation d'une valeur 16 bits
    /// </summary>
    public void Decrement16(ref ushort value)
    {
        int result = value - 1;
        value = (ushort)(result & 0xFFFF);
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.SetCarryFlag(result < 0);
        _cpu.SR.UpdateNegativeFlag((byte)((value >> 8) & 0xFF)); // Le byte haut détermine le signe
    }
}