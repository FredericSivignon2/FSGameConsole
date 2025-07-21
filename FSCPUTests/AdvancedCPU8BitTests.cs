using FSCPU;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Unit tests for advanced FSCPU instructions - Memory loads, register transfers, and relative jumps
    /// </summary>
    public class AdvancedCPU8BitTests
    {
        private Memory _memory;
        private FSCPU.CPU8Bit _cpu;

        public AdvancedCPU8BitTests()
        {
            _memory = new Memory(0x10000);
            _cpu = new FSCPU.CPU8Bit(_memory);
        }

        // === MEMORY LOAD INSTRUCTIONS TESTS (0x80-0x85) ===

        [Fact]
        public void LDA_FromMemory_ShouldLoadFromAddress()
        {
            // Arrange - Place data in memory and program in RAM
            _memory.WriteByte(0x2000, 0x42); // Data at memory address
            _memory.WriteByte(0x0000, 0x90); // LDA addr
            _memory.WriteWord(0x0001, 0x2000); // Address to load from
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x42);
            _cpu.PC.Should().Be(3);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void LDB_FromMemory_ShouldLoadFromAddress()
        {
            // Arrange
            _memory.WriteByte(0x1500, 0x99); // Data at memory address
            _memory.WriteByte(0x0000, 0x91); // LDB addr
            _memory.WriteWord(0x0001, 0x1500); // Address to load from
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.B.Should().Be(0x99);
            _cpu.PC.Should().Be(3);
        }

        [Theory]
        [InlineData(0x92, 'C')] // LDC addr
        [InlineData(0x93, 'D')] // LDD addr
        [InlineData(0x94, 'E')] // LDE addr
        [InlineData(0x95, 'F')] // LDF addr
        public void MemoryLoadInstructions_ShouldLoadIntoCorrectRegister(byte opcode, char expectedRegister)
        {
            // Arrange
            _memory.WriteByte(0x3000, 0x55); // Data at memory address
            _memory.WriteByte(0x0000, opcode);
            _memory.WriteWord(0x0001, 0x3000); // Address to load from
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.GetRegister(expectedRegister).Should().Be(0x55);
            _cpu.PC.Should().Be(3);
        }

        [Fact]
        public void LDA_FromMemory_ShouldSetZeroFlag()
        {
            // Arrange
            _memory.WriteByte(0x2000, 0x00); // Zero data at memory address
            _memory.WriteByte(0x0000, 0x90); // LDA addr
            _memory.WriteWord(0x0001, 0x2000); // Address to load from
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x00);
            _cpu.SR.Zero.Should().BeTrue();
        }

        // === REGISTER TRANSFER INSTRUCTIONS TESTS (0xA0-0xA7) ===

        [Fact]
        public void MOV_A_B_ShouldMoveRegisterB_ToA()
        {
            // Arrange
            _cpu.B = 0x42;
            _cpu.A = 0x00;
            _memory.WriteByte(0x0000, 0xA0); // MOV A,B
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x42);
            _cpu.B.Should().Be(0x42); // B unchanged
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void MOV_A_C_ShouldMoveRegisterC_ToA()
        {
            // Arrange
            _cpu.C = 0x99;
            _cpu.A = 0x00;
            _memory.WriteByte(0x0000, 0xA1); // MOV A,C
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x99);
            _cpu.C.Should().Be(0x99); // C unchanged
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void MOV_B_A_ShouldMoveRegisterA_ToB()
        {
            // Arrange
            _cpu.A = 0x33;
            _cpu.B = 0x00;
            _memory.WriteByte(0x0000, 0xA2); // MOV B,A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.B.Should().Be(0x33);
            _cpu.A.Should().Be(0x33); // A unchanged
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void MOV_B_C_ShouldMoveRegisterC_ToB()
        {
            // Arrange
            _cpu.C = 0x77;
            _cpu.B = 0x00;
            _memory.WriteByte(0x0000, 0xA3); // MOV B,C
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.B.Should().Be(0x77);
            _cpu.C.Should().Be(0x77); // C unchanged
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void MOV_C_A_ShouldMoveRegisterA_ToC()
        {
            // Arrange
            _cpu.A = 0x88;
            _cpu.C = 0x00;
            _memory.WriteByte(0x0000, 0xA4); // MOV C,A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.C.Should().Be(0x88);
            _cpu.A.Should().Be(0x88); // A unchanged
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void MOV_C_B_ShouldMoveRegisterB_ToC()
        {
            // Arrange
            _cpu.B = 0xAA;
            _cpu.C = 0x00;
            _memory.WriteByte(0x0000, 0xA5); // MOV C,B
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.C.Should().Be(0xAA);
            _cpu.B.Should().Be(0xAA); // B unchanged
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void SWP_A_B_ShouldSwapRegisters()
        {
            // Arrange
            _cpu.A = 0x11;
            _cpu.B = 0x22;
            _memory.WriteByte(0x0000, 0xA6); // SWP A,B
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x22); // A now has B's old value
            _cpu.B.Should().Be(0x11); // B now has A's old value
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void SWP_A_C_ShouldSwapRegisters()
        {
            // Arrange
            _cpu.A = 0x33;
            _cpu.C = 0x44;
            _memory.WriteByte(0x0000, 0xA7); // SWP A,C
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x44); // A now has C's old value
            _cpu.C.Should().Be(0x33); // C now has A's old value
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void MOV_Instructions_ShouldUpdateZeroFlag()
        {
            // Arrange - Test MOV with zero value
            _cpu.B = 0x00;
            _cpu.A = 0xFF;
            _memory.WriteByte(0x0000, 0xA0); // MOV A,B (move 0 to A)
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x00);
            _cpu.SR.Zero.Should().BeTrue();
        }

        // === RELATIVE JUMP INSTRUCTIONS TESTS (0xC0-0xC3) ===

        [Fact]
        public void JR_ShouldJumpForward()
        {
            // Arrange
            _memory.WriteByte(0x0000, 0xC0); // JR
            _memory.WriteByte(0x0001, 0x10); // +16 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be((ushort)(0x0002 + 0x10)); // PC was 0x0002 after reading offset, then +16
        }

        [Fact]
        public void JR_ShouldJumpBackward()
        {
            // Arrange
            _memory.WriteByte(0x0020, 0xC0); // JR
            _memory.WriteByte(0x0021, 0xF0); // -16 offset (0xF0 = -16 in signed byte)
            _cpu.PC = 0x0020;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be((ushort)(0x0022 - 16)); // PC was 0x0022 after reading offset, then -16
        }

        [Fact]
        public void JRZ_ShouldJumpWhenZeroFlagSet()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(0); // Set Zero flag
            _memory.WriteByte(0x0000, 0xC1); // JRZ
            _memory.WriteByte(0x0001, 0x08); // +8 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be((ushort)(0x0002 + 0x08)); // Should jump
        }

        [Fact]
        public void JRZ_ShouldNotJumpWhenZeroFlagClear()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(1); // Clear Zero flag
            _memory.WriteByte(0x0000, 0xC1); // JRZ
            _memory.WriteByte(0x0001, 0x08); // +8 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x0002); // Should not jump, just continue
        }

        [Fact]
        public void JRNZ_ShouldJumpWhenZeroFlagClear()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(1); // Clear Zero flag
            _memory.WriteByte(0x0000, 0xC2); // JRNZ
            _memory.WriteByte(0x0001, 0x05); // +5 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be((ushort)(0x0002 + 0x05)); // Should jump
        }

        [Fact]
        public void JRNZ_ShouldNotJumpWhenZeroFlagSet()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(0); // Set Zero flag
            _memory.WriteByte(0x0000, 0xC2); // JRNZ
            _memory.WriteByte(0x0001, 0x05); // +5 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x0002); // Should not jump
        }

        [Fact]
        public void JRC_ShouldJumpWhenCarryFlagSet()
        {
            // Arrange
            _cpu.SR.SetCarryFlag(true); // Set Carry flag
            _memory.WriteByte(0x0000, 0xC3); // JRC
            _memory.WriteByte(0x0001, 0x0C); // +12 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be((ushort)(0x0002 + 0x0C)); // Should jump
        }

        [Fact]
        public void JRC_ShouldNotJumpWhenCarryFlagClear()
        {
            // Arrange
            _cpu.SR.SetCarryFlag(false); // Clear Carry flag
            _memory.WriteByte(0x0000, 0xC3); // JRC
            _memory.WriteByte(0x0001, 0x0C); // +12 offset
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x0002); // Should not jump
        }

        // === COMPLEX PROGRAM TESTS WITH NEW INSTRUCTIONS ===

        [Fact]
        public void ComplexProgram_WithNewInstructions_ShouldExecuteCorrectly()
        {
            // Arrange - Program using memory loads, register transfers, and relative jumps
            // Load values from memory, transfer between registers, and use relative jump
            
            // Put test data in memory
            _memory.WriteByte(0x3000, 0x10); // Data for A
            _memory.WriteByte(0x3001, 0x20); // Data for B
            
            _memory.WriteByte(0x0000, 0x90); // LDA $3000
            _memory.WriteWord(0x0001, 0x3000);
            _memory.WriteByte(0x0003, 0x91); // LDB $3001  
            _memory.WriteWord(0x0004, 0x3001);
            _memory.WriteByte(0x0006, 0xA4); // MOV C,A (C = A = 0x10)
            _memory.WriteByte(0x0007, 0x20); // ADD A,B (A = 0x10 + 0x20 = 0x30)
            _memory.WriteByte(0x0008, 0xC0); // JR +2 (skip next instruction)
            _memory.WriteByte(0x0009, 0x02);
            _memory.WriteByte(0x000A, 0x10); // LDA #0xFF (should be skipped)
            _memory.WriteByte(0x000B, 0xFF);
            _memory.WriteByte(0x000C, 0xA6); // SWP A,B (A=0x20, B=0x30)
            _memory.WriteByte(0x000D, 0x01); // HALT
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Execute complete program
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert
            _cpu.A.Should().Be(0x20);         // After swap, A has old B value
            _cpu.B.Should().Be(0x30);         // After swap, B has old A value (0x30 from addition)
            _cpu.C.Should().Be(0x10);         // C has original A value
            _cpu.PC.Should().Be(0x000E);      // PC after HALT
            _cpu.IsRunning.Should().BeFalse(); // CPU stopped
        }

        [Fact]
        public void RelativeJump_Loop_ShouldWorkCorrectly()
        {
            // Arrange - Simple loop using relative jumps
            _memory.WriteByte(0x0000, 0x10); // LDA #5 (counter)
            _memory.WriteByte(0x0001, 0x05);
            // LOOP at 0x0002:
            _memory.WriteByte(0x0002, 0x29); // DEC A
            _memory.WriteByte(0x0003, 0xC1); // JRZ +2 (jump to HALT if zero)
            _memory.WriteByte(0x0004, 0x02);
            _memory.WriteByte(0x0005, 0xC0); // JR -5 (back to LOOP at 0x0002)
            _memory.WriteByte(0x0006, 0xFB); // -5 in signed byte (to go from 0x0007 to 0x0002)
            _memory.WriteByte(0x0007, 0x01); // HALT
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Execute several cycles manually to avoid infinite loop
            _cpu.ExecuteCycle(); // LDA #5 (A = 5)
            _cpu.A.Should().Be(5);
            _cpu.PC.Should().Be(0x0002);
            
            _cpu.ExecuteCycle(); // DEC A (A becomes 4)
            _cpu.A.Should().Be(4);
            _cpu.PC.Should().Be(0x0003);
            
            _cpu.ExecuteCycle(); // JRZ +2 (should not jump, A != 0)
            _cpu.PC.Should().Be(0x0005); // Should continue to next instruction
            
            _cpu.ExecuteCycle(); // JR -5 (jump back to DEC A at 0x0002)
            _cpu.PC.Should().Be(0x0002); // Should be back at DEC A
            
            _cpu.ExecuteCycle(); // DEC A (A becomes 3)

            // Assert
            _cpu.A.Should().Be(3);
            _cpu.PC.Should().Be(0x0003); // At JRZ instruction
        }
    }
}