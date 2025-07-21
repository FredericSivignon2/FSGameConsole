namespace FSCPU;

/// <summary>
/// Registre de statut contenant les flags du processeur
/// </summary>
public class StatusRegister
{
    private byte _value;
    
    // Flags individuels
    public bool Zero => (_value & 0x01) != 0;      // Bit 0: Résultat = 0
    public bool Carry => (_value & 0x02) != 0;     // Bit 1: Retenue
    public bool Overflow => (_value & 0x04) != 0;  // Bit 2: Débordement
    public bool Negative => (_value & 0x08) != 0;  // Bit 3: Résultat négatif
    
    /// <summary>
    /// Valeur complète du registre de statut
    /// </summary>
    public byte Value 
    { 
        get => _value; 
        set => _value = value; 
    }
    
    /// <summary>
    /// Remet tous les flags à zéro
    /// </summary>
    public void Reset()
    {
        _value = 0;
    }
    
    /// <summary>
    /// Met à jour le flag Zero basé sur une valeur
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
    /// Met à jour le flag Carry
    /// </summary>
    public void SetCarryFlag(bool value)
    {
        SetFlag(1, value);
    }
    
    /// <summary>
    /// Met à jour le flag Overflow
    /// </summary>
    public void SetOverflowFlag(bool value)
    {
        SetFlag(2, value);
    }
    
    /// <summary>
    /// Met à jour le flag Negative basé sur une valeur
    /// </summary>
    public void UpdateNegativeFlag(byte value)
    {
        SetFlag(3, (value & 0x80) != 0);
    }
    
    /// <summary>
    /// Met à jour tous les flags arithmétiques basés sur un résultat
    /// </summary>
    public void UpdateFlags(int result)
    {
        // Zero flag
        SetFlag(0, (result & 0xFF) == 0);
        
        // Carry flag (si le résultat dépasse 8 bits)
        SetFlag(1, result > 0xFF || result < 0);
        
        // Negative flag (bit 7 du résultat)
        SetFlag(3, ((result & 0xFF) & 0x80) != 0);
    }
    
    /// <summary>
    /// Définit ou efface un flag spécifique
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