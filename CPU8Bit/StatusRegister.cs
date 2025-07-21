namespace FSCPU;

/// <summary>
/// Registre de statut contenant les flags du processeur
/// </summary>
public class StatusRegister
{
    private byte _value;
    
    // Flags individuels
    public bool Zero => (_value & 0x01) != 0;      // Bit 0: R�sultat = 0
    public bool Carry => (_value & 0x02) != 0;     // Bit 1: Retenue
    public bool Overflow => (_value & 0x04) != 0;  // Bit 2: D�bordement
    public bool Negative => (_value & 0x08) != 0;  // Bit 3: R�sultat n�gatif
    
    /// <summary>
    /// Valeur compl�te du registre de statut
    /// </summary>
    public byte Value 
    { 
        get => _value; 
        set => _value = value; 
    }
    
    /// <summary>
    /// Remet tous les flags � z�ro
    /// </summary>
    public void Reset()
    {
        _value = 0;
    }
    
    /// <summary>
    /// Met � jour le flag Zero bas� sur une valeur
    /// </summary>
    public void UpdateZeroFlag(byte value)
    {
        SetFlag(0, value == 0);
    }

    public void UpdateZeroFlag(ushort value)
    {
        SetFlag(0, value == 0);
    }

    /// <summary>
    /// Met � jour le flag Carry
    /// </summary>
    public void SetCarryFlag(bool value)
    {
        SetFlag(1, value);
    }
    
    /// <summary>
    /// Met � jour le flag Overflow
    /// </summary>
    public void SetOverflowFlag(bool value)
    {
        SetFlag(2, value);
    }
    
    /// <summary>
    /// Met � jour le flag Negative bas� sur une valeur
    /// </summary>
    public void UpdateNegativeFlag(byte value)
    {
        SetFlag(3, (value & 0x80) != 0);
    }
    
    /// <summary>
    /// Met � jour tous les flags arithm�tiques bas�s sur un r�sultat
    /// </summary>
    public void UpdateFlags(int result)
    {
        // Zero flag
        SetFlag(0, (result & 0xFF) == 0);
        
        // Carry flag (si le r�sultat d�passe 8 bits)
        SetFlag(1, result > 0xFF || result < 0);
        
        // Negative flag (bit 7 du r�sultat)
        SetFlag(3, ((result & 0xFF) & 0x80) != 0);
    }
    
    /// <summary>
    /// D�finit ou efface un flag sp�cifique
    /// </summary>
    private void SetFlag(int bitPosition, bool value)
    {
        if (value)
            _value |= (byte)(1 << bitPosition);
        else
            _value &= (byte)~(1 << bitPosition);
    }
    
    public override string ToString()
    {
        return $"Z:{(Zero ? 1 : 0)} C:{(Carry ? 1 : 0)} O:{(Overflow ? 1 : 0)} N:{(Negative ? 1 : 0)}";
    }
}