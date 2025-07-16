using Xunit;
using FluentAssertions;
using CPU8Bit;

namespace FSCPUTests;

/// <summary>
/// Tests for index register instructions (IDX1, IDX2, IDY1, IDY2)
/// These are new Z80-style index registers for enhanced array and table manipulation
/// </summary>
public class IndexRegisterTests
{
    private CPU8Bit.CPU8Bit _cpu;
    private Memory _memory;

    public IndexRegisterTests()
    {
        _memory = new Memory();
        _cpu = new CPU8Bit.CPU8Bit(_memory);
        _cpu.Start(false); // Start without clock for controlled testing
    }

    #region Index Register Load Instructions

    //[Fact]
    //public void LDIDX1_Should_Load_16Bit_Immediate_Value()
    //{
    //    // Arrange
    //    byte[] program = { 0x7A, 0x34, 0x12 }; // LDIDX1 #0x1234 (little-endian)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x1234);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 3));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void LDIDX2_Should_Load_16Bit_Immediate_Value()
    //{
    //    // Arrange
    //    byte[] program = { 0x7B, 0x78, 0x56 }; // LDIDX2 #0x5678 (little-endian)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX2.Should().Be(0x5678);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 3));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void LDIDY1_Should_Load_16Bit_Immediate_Value()
    //{
    //    // Arrange
    //    byte[] program = { 0x7C, 0xAB, 0xCD }; // LDIDY1 #0xCDAB (little-endian)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY1.Should().Be(0xCDAB);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 3));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void LDIDY2_Should_Load_16Bit_Immediate_Value()
    //{
    //    // Arrange
    //    byte[] program = { 0x7D, 0xEF, 0x12 }; // LDIDY2 #0x12EF (little-endian)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY2.Should().Be(0x12EF);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 3));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void Index_Register_Load_Should_Update_Zero_Flag_When_Zero()
    //{
    //    // Arrange
    //    byte[] program = { 0x7A, 0x00, 0x00 }; // LDIDX1 #0x0000
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x0000);
    //    _cpu.SR.Zero.Should().BeTrue();
    //}

    #endregion

    #region Index Register Increment/Decrement Instructions

    //[Fact]
    //public void INCIDX1_Should_Increment_IDX1_Register()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1234;
    //    byte[] program = { 0x7E }; // INCIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x1235);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void DECIDX1_Should_Decrement_IDX1_Register()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1234;
    //    byte[] program = { 0x7F }; // DECIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x1233);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void INCIDX2_Should_Increment_IDX2_Register()
    //{
    //    // Arrange
    //    _cpu.IDX2 = 0x5678;
    //    byte[] program = { 0x86 }; // INCIDX2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX2.Should().Be(0x5679);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void DECIDX2_Should_Decrement_IDX2_Register()
    //{
    //    // Arrange
    //    _cpu.IDX2 = 0x5678;
    //    byte[] program = { 0x87 }; // DECIDX2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX2.Should().Be(0x5677);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void INCIDY1_Should_Increment_IDY1_Register()
    //{
    //    // Arrange
    //    _cpu.IDY1 = 0x9ABC;
    //    byte[] program = { 0x88 }; // INCIDY1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY1.Should().Be(0x9ABD);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void DECIDY1_Should_Decrement_IDY1_Register()
    //{
    //    // Arrange
    //    _cpu.IDY1 = 0x9ABC;
    //    byte[] program = { 0x89 }; // DECIDY1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY1.Should().Be(0x9ABB);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void INCIDY2_Should_Increment_IDY2_Register()
    //{
    //    // Arrange
    //    _cpu.IDY2 = 0xDEF0;
    //    byte[] program = { 0x8A }; // INCIDY2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY2.Should().Be(0xDEF1);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void DECIDY2_Should_Decrement_IDY2_Register()
    //{
    //    // Arrange
    //    _cpu.IDY2 = 0xDEF0;
    //    byte[] program = { 0x8B }; // DECIDY2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY2.Should().Be(0xDEEF);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void Index_Register_Increment_Should_Handle_Overflow()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0xFFFF;
    //    byte[] program = { 0x7E }; // INCIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x0000);
    //    _cpu.SR.Zero.Should().BeTrue();
    //}

    //[Fact]
    //public void Index_Register_Decrement_Should_Handle_Underflow()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x0000;
    //    byte[] program = { 0x7F }; // DECIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0xFFFF);
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    #endregion

    #region Indexed Memory Access Instructions

    //[Fact]
    //public void LDA_IDX1_Should_Load_A_From_Memory_Pointed_By_IDX1()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1000;
    //    _memory.WriteByte(0x1000, 0x42);
    //    byte[] program = { 0x8C }; // LDA (IDX1)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.A.Should().Be(0x42);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void LDA_IDX2_Should_Load_A_From_Memory_Pointed_By_IDX2()
    //{
    //    // Arrange
    //    _cpu.IDX2 = 0x2000;
    //    _memory.WriteByte(0x2000, 0x84);
    //    byte[] program = { 0x8D }; // LDA (IDX2)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.A.Should().Be(0x84);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void LDA_IDY1_Should_Load_A_From_Memory_Pointed_By_IDY1()
    //{
    //    // Arrange
    //    _cpu.IDY1 = 0x3000;
    //    _memory.WriteByte(0x3000, 0xAB);
    //    byte[] program = { 0x8E }; // LDA (IDY1)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.A.Should().Be(0xAB);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void LDA_IDY2_Should_Load_A_From_Memory_Pointed_By_IDY2()
    //{
    //    // Arrange
    //    _cpu.IDY2 = 0x4000;
    //    _memory.WriteByte(0x4000, 0xCD);
    //    byte[] program = { 0x8F }; // LDA (IDY2)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.A.Should().Be(0xCD);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void STA_IDX1_Should_Store_A_To_Memory_Pointed_By_IDX1()
    //{
    //    // Arrange
    //    _cpu.A = 0x55;
    //    _cpu.IDX1 = 0x1500;
    //    byte[] program = { 0x90 }; // STA (IDX1)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _memory.ReadByte(0x1500).Should().Be(0x55);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void STA_IDX2_Should_Store_A_To_Memory_Pointed_By_IDX2()
    //{
    //    // Arrange
    //    _cpu.A = 0xAA;
    //    _cpu.IDX2 = 0x2500;
    //    byte[] program = { 0x91 }; // STA (IDX2)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _memory.ReadByte(0x2500).Should().Be(0xAA);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void STA_IDY1_Should_Store_A_To_Memory_Pointed_By_IDY1()
    //{
    //    // Arrange
    //    _cpu.A = 0x33;
    //    _cpu.IDY1 = 0x3500;
    //    byte[] program = { 0x92 }; // STA (IDY1)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _memory.ReadByte(0x3500).Should().Be(0x33);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void STA_IDY2_Should_Store_A_To_Memory_Pointed_By_IDY2()
    //{
    //    // Arrange
    //    _cpu.A = 0x77;
    //    _cpu.IDY2 = 0x4500;
    //    byte[] program = { 0x93 }; // STA (IDY2)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _memory.ReadByte(0x4500).Should().Be(0x77);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void Indexed_Load_Should_Update_Zero_Flag_When_Zero()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1000;
    //    _memory.WriteByte(0x1000, 0x00);
    //    byte[] program = { 0x8C }; // LDA (IDX1)
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.A.Should().Be(0x00);
    //    _cpu.SR.Zero.Should().BeTrue();
    //}

    #endregion

    #region Index Register Stack Instructions

    //[Fact]
    //public void PUSHIDX1_Should_Push_IDX1_To_Stack()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1234;
    //    _cpu.SP = 0x8000;
    //    byte[] program = { 0x94 }; // PUSHIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.SP.Should().Be(0x7FFE);
    //    _memory.ReadWord(0x7FFE).Should().Be(0x1234);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void POPIDX1_Should_Pop_From_Stack_To_IDX1()
    //{
    //    // Arrange
    //    _cpu.SP = 0x7FFE;
    //    _memory.WriteWord(0x7FFE, 0x5678);
    //    byte[] program = { 0x95 }; // POPIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x5678);
    //    _cpu.SP.Should().Be(0x8000);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void PUSHIDX2_Should_Push_IDX2_To_Stack()
    //{
    //    // Arrange
    //    _cpu.IDX2 = 0x9ABC;
    //    _cpu.SP = 0x8000;
    //    byte[] program = { 0x96 }; // PUSHIDX2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.SP.Should().Be(0x7FFE);
    //    _memory.ReadWord(0x7FFE).Should().Be(0x9ABC);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void POPIDX2_Should_Pop_From_Stack_To_IDX2()
    //{
    //    // Arrange
    //    _cpu.SP = 0x7FFE;
    //    _memory.WriteWord(0x7FFE, 0xDEF0);
    //    byte[] program = { 0x97 }; // POPIDX2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX2.Should().Be(0xDEF0);
    //    _cpu.SP.Should().Be(0x8000);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void PUSHIDY1_Should_Push_IDY1_To_Stack()
    //{
    //    // Arrange
    //    _cpu.IDY1 = 0x1357;
    //    _cpu.SP = 0x8000;
    //    byte[] program = { 0x98 }; // PUSHIDY1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.SP.Should().Be(0x7FFE);
    //    _memory.ReadWord(0x7FFE).Should().Be(0x1357);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void POPIDY1_Should_Pop_From_Stack_To_IDY1()
    //{
    //    // Arrange
    //    _cpu.SP = 0x7FFE;
    //    _memory.WriteWord(0x7FFE, 0x2468);
    //    byte[] program = { 0x99 }; // POPIDY1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY1.Should().Be(0x2468);
    //    _cpu.SP.Should().Be(0x8000);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void PUSHIDY2_Should_Push_IDY2_To_Stack()
    //{
    //    // Arrange
    //    _cpu.IDY2 = 0xACE1;
    //    _cpu.SP = 0x8000;
    //    byte[] program = { 0x9A }; // PUSHIDY2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.SP.Should().Be(0x7FFE);
    //    _memory.ReadWord(0x7FFE).Should().Be(0xACE1);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //}

    //[Fact]
    //public void POPIDY2_Should_Pop_From_Stack_To_IDY2()
    //{
    //    // Arrange
    //    _cpu.SP = 0x7FFE;
    //    _memory.WriteWord(0x7FFE, 0xBDF2);
    //    byte[] program = { 0x9B }; // POPIDY2
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDY2.Should().Be(0xBDF2);
    //    _cpu.SP.Should().Be(0x8000);
    //    _cpu.PC.Should().Be((ushort)(_memory.RomStart + 1));
    //    _cpu.SR.Zero.Should().BeFalse();
    //}

    //[Fact]
    //public void Index_Register_Pop_Should_Update_Zero_Flag_When_Zero()
    //{
    //    // Arrange
    //    _cpu.SP = 0x7FFE;
    //    _memory.WriteWord(0x7FFE, 0x0000);
    //    byte[] program = { 0x95 }; // POPIDX1
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x0000);
    //    _cpu.SR.Zero.Should().BeTrue();
    //}

    #endregion

    #region Register Access Methods

    //[Fact]
    //public void GetRegister16_Should_Return_Index_Registers()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1234;
    //    _cpu.IDX2 = 0x5678;
    //    _cpu.IDY1 = 0x9ABC;
    //    _cpu.IDY2 = 0xDEF0;

    //    // Act & Assert
    //    _cpu.GetRegister16("IDX1").Should().Be(0x1234);
    //    _cpu.GetRegister16("IDX2").Should().Be(0x5678);
    //    _cpu.GetRegister16("IDY1").Should().Be(0x9ABC);
    //    _cpu.GetRegister16("IDY2").Should().Be(0xDEF0);
    //}

    //[Fact]
    //public void SetRegister16_Should_Set_Index_Registers()
    //{
    //    // Act
    //    _cpu.SetRegister16("IDX1", 0x1111);
    //    _cpu.SetRegister16("IDX2", 0x2222);
    //    _cpu.SetRegister16("IDY1", 0x3333);
    //    _cpu.SetRegister16("IDY2", 0x4444);

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x1111);
    //    _cpu.IDX2.Should().Be(0x2222);
    //    _cpu.IDY1.Should().Be(0x3333);
    //    _cpu.IDY2.Should().Be(0x4444);
    //}

    //[Fact]
    //public void Reset_Should_Clear_All_Index_Registers()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1234;
    //    _cpu.IDX2 = 0x5678;
    //    _cpu.IDY1 = 0x9ABC;
    //    _cpu.IDY2 = 0xDEF0;

    //    // Act
    //    _cpu.Reset();

    //    // Assert
    //    _cpu.IDX1.Should().Be(0x0000);
    //    _cpu.IDX2.Should().Be(0x0000);
    //    _cpu.IDY1.Should().Be(0x0000);
    //    _cpu.IDY2.Should().Be(0x0000);
    //}

    #endregion

    #region Complex Scenarios

    //[Fact]
    //public void Array_Processing_With_Index_Registers_Should_Work()
    //{
    //    // Arrange: Set up an array at 0x2000 with values [10, 20, 30]
    //    _memory.WriteByte(0x2000, 10);
    //    _memory.WriteByte(0x2001, 20);
    //    _memory.WriteByte(0x2002, 30);
        
    //    _cpu.IDX1 = 0x2000; // Point to start of array
    //    _cpu.A = 0;

    //    byte[] program = {
    //        0x8C,       // LDA (IDX1) - Load first element
    //        0x7E,       // INCIDX1 - Point to next element
    //        0x8C,       // LDA (IDX1) - Load second element
    //        0x7E,       // INCIDX1 - Point to next element
    //        0x8C        // LDA (IDX1) - Load third element
    //    };
    //    _memory.LoadProgram(program);

    //    // Act
    //    _cpu.ExecuteCycle(); // Load first element (10)
    //    byte first = _cpu.A;
        
    //    _cpu.ExecuteCycle(); // Increment index
    //    _cpu.ExecuteCycle(); // Load second element (20)
    //    byte second = _cpu.A;
        
    //    _cpu.ExecuteCycle(); // Increment index
    //    _cpu.ExecuteCycle(); // Load third element (30)
    //    byte third = _cpu.A;

    //    // Assert
    //    first.Should().Be(10);
    //    second.Should().Be(20);
    //    third.Should().Be(30);
    //    _cpu.IDX1.Should().Be(0x2002);
    //}

    [Fact]
    public void String_Copy_With_Index_Registers_Should_Work()
    {
        // Arrange: Source string at 0x3000: "ABC"
        _memory.WriteByte(0x3000, (byte)'A');
        _memory.WriteByte(0x3001, (byte)'B');
        _memory.WriteByte(0x3002, (byte)'C');
        _memory.WriteByte(0x3003, 0); // Null terminator
        
        byte[] program = {
            0x8C,       // LDA (IDX1) - Load from source
            0x92,       // STA (IDY1) - Store to destination
            0x7E,       // INCIDX1 - Increment source pointer
            0x88,       // INCIDY1 - Increment destination pointer
            0x8C,       // LDA (IDX1) - Load next char
            0x92,       // STA (IDY1) - Store next char
            0x7E,       // INCIDX1
            0x88,       // INCIDY1
            0x8C,       // LDA (IDX1) - Load next char
            0x92,       // STA (IDY1) - Store next char
            0x7E,       // INCIDX1
            0x88,       // INCIDY1
            0x8C,       // LDA (IDX1) - Load null terminator
            0x92        // STA (IDY1) - Store null terminator
        };
        _memory.LoadProgram(program);

        // Act - Copy the string
        for (int i = 0; i < 14; i++)
        {
            _cpu.ExecuteCycle();
        }

        // Assert
        _memory.ReadByte(0x4000).Should().Be((byte)'A');
        _memory.ReadByte(0x4001).Should().Be((byte)'B');
        _memory.ReadByte(0x4002).Should().Be((byte)'C');
        _memory.ReadByte(0x4003).Should().Be(0);
    }

    //[Fact]
    //public void Index_Register_Stack_Operations_Should_Preserve_Values()
    //{
    //    // Arrange
    //    _cpu.IDX1 = 0x1111;
    //    _cpu.IDX2 = 0x2222;
    //    _cpu.IDY1 = 0x3333;
    //    _cpu.IDY2 = 0x4444;
        
    //    byte[] program = {
    //        0x94, // PUSHIDX1
    //        0x96, // PUSHIDX2
    //        0x98, // PUSHIDY1
    //        0x9A, // PUSHIDY2
    //        0x9B, // POPIDY2
    //        0x99, // POPIDY1
    //        0x97, // POPIDX2
    //        0x95  // POPIDX1
    //    };
    //    _memory.LoadProgram(program);

    //    // Act - Push all then pop all in reverse order
    //    for (int i = 0; i < 8; i++)
    //    {
    //        _cpu.ExecuteCycle();
    //    }

    //    // Assert - Values should be restored correctly
    //    _cpu.IDX1.Should().Be(0x1111);
    //    _cpu.IDX2.Should().Be(0x2222);
    //    _cpu.IDY1.Should().Be(0x3333);
    //    _cpu.IDY2.Should().Be(0x4444);
    //}

    #endregion
}