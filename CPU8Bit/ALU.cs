namespace FSCPU;

/// <summary>
/// Unit� Arithm�tique et Logique du processeur 8 bits
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
        
        // Mise � jour des flags
        _cpu.SR.UpdateFlags(result);
        
        // Overflow flag sp�cial pour l'addition sign�e
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
        
        // Mise � jour des flags
        _cpu.SR.UpdateFlags(result);
        
        // Pour la soustraction, le carry flag indique un emprunt
        _cpu.SR.SetCarryFlag(result < 0);
    }
    
    /// <summary>
    /// Op�ration AND logique
    /// </summary>
    public void And(ref byte destination, byte source)
    {
        destination &= source;
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.UpdateNegativeFlag(destination);
        _cpu.SR.SetCarryFlag(false); // AND efface toujours le carry
    }
    
    /// <summary>
    /// Op�ration OR logique
    /// </summary>
    public void Or(ref byte destination, byte source)
    {
        destination |= source;
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.UpdateNegativeFlag(destination);
        _cpu.SR.SetCarryFlag(false); // OR efface toujours le carry
    }
    
    /// <summary>
    /// Op�ration XOR logique
    /// </summary>
    public void Xor(ref byte destination, byte source)
    {
        destination ^= source;
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.UpdateNegativeFlag(destination);
        _cpu.SR.SetCarryFlag(false); // XOR efface toujours le carry
    }
    
    /// <summary>
    /// D�calage � gauche (Shift Left)
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
    /// D�calage � droite (Shift Right)
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
    /// Incr�mentation d'une valeur
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
    /// D�cr�mentation d'une valeur
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
    /// Compare deux valeurs (soustraction sans stocker le r�sultat)
    /// </summary>
    public void Compare(byte value1, byte value2)
    {
        int result = value1 - value2;
        
        _cpu.SR.UpdateZeroFlag((byte)(result & 0xFF));
        _cpu.SR.UpdateNegativeFlag((byte)(result & 0xFF));
        _cpu.SR.SetCarryFlag(result < 0);
    }

    /// <summary>
    /// Compare deux valeurs 16 bits (soustraction sans stocker le r�sultat)
    /// </summary>
    public void Compare(ushort value1, ushort value2)
    {
        int result = value1 - value2;

        // Pour la comparaison 16 bits, v�rifie si le r�sultat complet est z�ro
        _cpu.SR.UpdateZeroFlag((ushort)(result & 0xFFFF));
        
        // Pour les valeurs 16 bits, le flag n�gatif est d�termin� par le bit 15 du r�sultat
        // MAIS : Pour une comparaison non sign�e, on v�rifie le signe r�el du r�sultat, pas le motif de bits
        _cpu.SR.SetCarryFlag(result < 0);  // Le carry indique un emprunt (r�sultat < 0)
        
        // Pour le flag n�gatif dans la comparaison non sign�e 16 bits :
        // Un r�sultat est "n�gatif" s'il serait n�gatif dans un contexte sign�
        // Cela signifie : le bit 15 du r�sultat 16 bits doit �tre v�rifi�
        bool bit15Set = ((result & 0x8000) != 0);
        
        // Cr�e un byte fictif o� le bit 7 repr�sente l'�tat de notre bit 15
        byte fakeByte = bit15Set ? (byte)0x80 : (byte)0x00;
        _cpu.SR.UpdateNegativeFlag(fakeByte);
    }

    // === OP�RATIONS 16-BITS POUR REGISTRES DA/DB ===

    /// <summary>
    /// Addition de deux valeurs 16 bits
    /// </summary>
    public void Add16(ref ushort destination, ushort source)
    {
        int result = destination + source;
        destination = (ushort)(result & 0xFFFF);
        
        // Mise � jour des flags bas� sur le r�sultat 16 bits
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.SetCarryFlag(result > 0xFFFF);
        _cpu.SR.UpdateNegativeFlag((byte)((destination >> 8) & 0xFF)); // Le byte haut d�termine le signe
    }
    
    /// <summary>
    /// Soustraction de deux valeurs 16 bits
    /// </summary>
    public void Subtract16(ref ushort destination, ushort source)
    {
        int result = destination - source;
        destination = (ushort)(result & 0xFFFF);
        
        // Mise � jour des flags bas� sur le r�sultat 16 bits
        _cpu.SR.UpdateZeroFlag(destination);
        _cpu.SR.SetCarryFlag(result < 0);
        _cpu.SR.UpdateNegativeFlag((byte)((destination >> 8) & 0xFF)); // Le byte haut d�termine le signe
    }
    
    /// <summary>
    /// Incr�mentation d'une valeur 16 bits
    /// </summary>
    public void Increment16(ref ushort value)
    {
        int result = value + 1;
        value = (ushort)(result & 0xFFFF);
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.SetCarryFlag(result > 0xFFFF);
        _cpu.SR.UpdateNegativeFlag((byte)((value >> 8) & 0xFF)); // Le byte haut d�termine le signe
    }
    
    /// <summary>
    /// D�cr�mentation d'une valeur 16 bits
    /// </summary>
    public void Decrement16(ref ushort value)
    {
        int result = value - 1;
        value = (ushort)(result & 0xFFFF);
        
        _cpu.SR.UpdateZeroFlag(value);
        _cpu.SR.SetCarryFlag(result < 0);
        _cpu.SR.UpdateNegativeFlag((byte)((value >> 8) & 0xFF)); // Le byte haut d�termine le signe
    }
}