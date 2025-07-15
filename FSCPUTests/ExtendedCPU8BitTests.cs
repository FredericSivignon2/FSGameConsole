using CPU8Bit;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Unit tests for new CPU8Bit instructions and 16-bit operations
    /// </summary>
    public class ExtendedCPU8BitTests
    {
        private Memory _memory;
        private CPU8Bit.CPU8Bit _cpu;

        public ExtendedCPU8BitTests()
        {
            _memory = new Memory(0x10000);
            _cpu = new CPU8Bit.CPU8Bit(_memory);
        }

        // === NEW REGISTER TESTS ===

        [Fact]
        public void NewRegisters_ShouldInitializeToZero()
        {
            // Assert
            _cpu.E.Should().Be(0);
            _cpu.F.Should().Be(0);
            _cpu.DA.Should().Be(0);
            _cpu.DB.Should().Be(0);
        }

        [Fact]
        public void GetRegister_ShouldSupportNewRegisters()
        {
            // Arrange
            _cpu.E = 50;
            _cpu.F = 75;

            // Act & Assert
            _cpu.GetRegister('E').Should().Be(50);
            _cpu.GetRegister('F').Should().Be(75);
        }

        [Fact]
        public void SetRegister_ShouldSupportNewRegisters()
        {
            // Act
            _cpu.SetRegister('E', 123);
            _cpu.SetRegister('F', 234);

            // Assert
            _cpu.E.Should().Be(123);
            _cpu.F.Should().Be(234);
        }

        [Fact]
        public void GetRegister16_ShouldWorkCorrectly()
        {
            // Arrange
            _cpu.DA = 0x1234;
            _cpu.DB = 0x5678;
            _cpu.PC = 0xABCD;
            _cpu.SP = 0xEF01;

            // Act & Assert
            _cpu.GetRegister16("DA").Should().Be(0x1234);
            _cpu.GetRegister16("DB").Should().Be(0x5678);
            _cpu.GetRegister16("PC").Should().Be(0xABCD);
            _cpu.GetRegister16("SP").Should().Be(0xEF01);
        }

        [Fact]
        public void SetRegister16_ShouldWorkCorrectly()
        {
            // Act
            _cpu.SetRegister16("DA", 0x1234);
            _cpu.SetRegister16("DB", 0x5678);

            // Assert
            _cpu.DA.Should().Be(0x1234);
            _cpu.DB.Should().Be(0x5678);
        }

        // === 16-BIT LOAD INSTRUCTIONS TESTS ===

        [Fact]
        public void LDDA16_Instruction_ShouldLoad16BitImmediateValue()
        {
            // Arrange - Place program in RAM
            _memory.WriteByte(0x0000, 0x16); // LDDA #imm16
            _memory.WriteWord(0x0001, 0x1234); // 16-bit value
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DA.Should().Be(0x1234);
            _cpu.PC.Should().Be(3);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void LDDB16_Instruction_ShouldLoad16BitImmediateValue()
        {
            // Arrange - Place program in RAM
            _memory.WriteByte(0x0000, 0x17); // LDDB #imm16
            _memory.WriteWord(0x0001, 0x5678); // 16-bit value
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DB.Should().Be(0x5678);
            _cpu.PC.Should().Be(3);
        }

        [Fact]
        public void LDDA_FromMemory_ShouldLoadFromAddress()
        {
            // Arrange - Place program and data in RAM
            _memory.WriteWord(0x2000, 0xABCD); // Data at memory address
            _memory.WriteByte(0x0000, 0x18); // LDDA addr
            _memory.WriteWord(0x0001, 0x2000); // Address to load from
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DA.Should().Be(0xABCD);
            _cpu.PC.Should().Be(3);
        }

        // === 16-BIT ARITHMETIC TESTS ===

        [Fact]
        public void ADD16_Instruction_ShouldAdd16BitValues()
        {
            // Arrange
            _cpu.DA = 0x1000;
            _cpu.DB = 0x0234;
            _memory.WriteByte(0x0000, 0x22); // ADD16 DA,DB
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DA.Should().Be(0x1234);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void SUB16_Instruction_ShouldSubtract16BitValues()
        {
            // Arrange
            _cpu.DA = 0x1234;
            _cpu.DB = 0x0234;
            _memory.WriteByte(0x0000, 0x23); // SUB16 DA,DB
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DA.Should().Be(0x1000);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void INC16_DA_ShouldIncrement16BitValue()
        {
            // Arrange
            _cpu.DA = 0x1234;
            _memory.WriteByte(0x0000, 0x24); // INC16 DA
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DA.Should().Be(0x1235);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void DEC16_DB_ShouldDecrement16BitValue()
        {
            // Arrange
            _cpu.DB = 0x1234;
            _memory.WriteByte(0x0000, 0x27); // DEC16 DB
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DB.Should().Be(0x1233);
            _cpu.PC.Should().Be(1);
        }

        // === 8-BIT INCREMENT/DECREMENT TESTS ===

        [Fact]
        public void INC_A_ShouldIncrementRegisterA()
        {
            // Arrange
            _cpu.A = 10;
            _memory.WriteByte(0x0000, 0x28); // INC A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(11);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void DEC_B_ShouldDecrementRegisterB()
        {
            // Arrange
            _cpu.B = 10;
            _memory.WriteByte(0x0000, 0x2B); // DEC B
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.B.Should().Be(9);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void CMP_Instruction_ShouldCompareValues()
        {
            // Arrange
            _cpu.A = 5;
            _cpu.B = 5;
            _memory.WriteByte(0x0000, 0x2C); // CMP A,B
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.SR.Zero.Should().BeTrue(); // A == B should set Zero flag
            _cpu.PC.Should().Be(1);
        }

        // === CONDITIONAL JUMP TESTS ===

        [Fact]
        public void JZ_Instruction_ShouldJumpWhenZeroFlagSet()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(0); // Set Zero flag
            _memory.WriteByte(0x0000, 0x41); // JZ
            _memory.WriteWord(0x0001, 0x1000); // Jump address
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x1000);
        }

        [Fact]
        public void JZ_Instruction_ShouldNotJumpWhenZeroFlagClear()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(1); // Clear Zero flag
            _memory.WriteByte(0x0000, 0x41); // JZ
            _memory.WriteWord(0x0001, 0x1000); // Jump address
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(3); // Should continue to next instruction
        }

        [Fact]
        public void JNZ_Instruction_ShouldJumpWhenZeroFlagClear()
        {
            // Arrange
            _cpu.SR.UpdateZeroFlag(1); // Clear Zero flag
            _memory.WriteByte(0x0000, 0x42); // JNZ
            _memory.WriteWord(0x0001, 0x1000); // Jump address
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x1000);
        }

        // === STACK OPERATION TESTS ===

        [Fact]
        public void PUSH_POP_A_ShouldWorkCorrectly()
        {
            // Arrange
            _cpu.A = 0x42;
            ushort initialSP = _cpu.SP;
            
            // PUSH A
            _memory.WriteByte(0x0000, 0x70); // PUSH A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Push
            _cpu.ExecuteCycle();

            // Assert - After Push
            _cpu.SP.Should().Be((ushort)(initialSP - 1));
            _memory.ReadByte(_cpu.SP).Should().Be(0x42);

            // Arrange - POP A
            _cpu.A = 0x00; // Clear A
            _memory.WriteByte(0x0001, 0x71); // POP A
            _cpu.PC = 0x0001;

            // Act - Pop
            _cpu.ExecuteCycle();

            // Assert - After Pop
            _cpu.A.Should().Be(0x42);
            _cpu.SP.Should().Be(initialSP);
        }

        [Fact]
        public void PUSH16_POP16_DA_ShouldWorkCorrectly()
        {
            // Arrange
            _cpu.DA = 0x1234;
            ushort initialSP = _cpu.SP;
            
            // PUSH16 DA
            _memory.WriteByte(0x0000, 0x72); // PUSH16 DA
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Push
            _cpu.ExecuteCycle();

            // Assert - After Push
            _cpu.SP.Should().Be((ushort)(initialSP - 2));
            _memory.ReadWord(_cpu.SP).Should().Be(0x1234);

            // Arrange - POP16 DA
            _cpu.DA = 0x0000; // Clear DA
            _memory.WriteByte(0x0001, 0x73); // POP16 DA
            _cpu.PC = 0x0001;

            // Act - Pop
            _cpu.ExecuteCycle();

            // Assert - After Pop
            _cpu.DA.Should().Be(0x1234);
            _cpu.SP.Should().Be(initialSP);
        }

        // === EXTENDED LOGICAL OPERATIONS TESTS ===

        [Fact]
        public void XOR_Instruction_ShouldPerformLogicalXor()
        {
            // Arrange
            _cpu.A = 0xFF;
            _cpu.B = 0x0F;
            _memory.WriteByte(0x0000, 0x32); // XOR A,B
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0xF0);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void NOT_Instruction_ShouldPerformLogicalNot()
        {
            // Arrange
            _cpu.A = 0x0F;
            _memory.WriteByte(0x0000, 0x33); // NOT A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0xF0);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void SHL_Instruction_ShouldShiftLeft()
        {
            // Arrange
            _cpu.A = 0x01;
            _memory.WriteByte(0x0000, 0x34); // SHL A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x02);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void SHR_Instruction_ShouldShiftRight()
        {
            // Arrange
            _cpu.A = 0x02;
            _memory.WriteByte(0x0000, 0x35); // SHR A
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x01);
            _cpu.PC.Should().Be(1);
        }

        // === STORE INSTRUCTIONS TESTS ===

        [Fact]
        public void STDA_Instruction_ShouldStore16BitValue()
        {
            // Arrange
            _cpu.DA = 0x1234;
            _memory.WriteByte(0x0000, 0x51); // STDA
            _memory.WriteWord(0x0001, 0x2000); // Address
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _memory.ReadWord(0x2000).Should().Be(0x1234);
            _cpu.PC.Should().Be(3);
        }

        [Fact]
        public void STB_Instruction_ShouldStoreBAtAddress()
        {
            // Arrange
            _cpu.B = 0x42;
            _memory.WriteByte(0x0000, 0x53); // STB
            _memory.WriteWord(0x0001, 0x2000); // Address
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _memory.ReadByte(0x2000).Should().Be(0x42);
            _cpu.PC.Should().Be(3);
        }

        // === COMPLEX PROGRAM TEST WITH NEW INSTRUCTIONS ===

        [Fact]
        public void ComplexProgram_WithNewInstructions_ShouldExecuteCorrectly()
        {
            // Arrange - Program that uses 16-bit operations
            _memory.WriteByte(0x0000, 0x16); // LDDA #0x1000
            _memory.WriteWord(0x0001, 0x1000);
            _memory.WriteByte(0x0003, 0x17); // LDDB #0x0234  
            _memory.WriteWord(0x0004, 0x0234);
            _memory.WriteByte(0x0006, 0x22); // ADD16 DA,DB (DA = 0x1234)
            _memory.WriteByte(0x0007, 0x51); // STDA $3000
            _memory.WriteWord(0x0008, 0x3000);
            _memory.WriteByte(0x000A, 0x01); // HALT
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Execute complete program
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert
            _cpu.DA.Should().Be(0x1234);         // DA = 0x1000 + 0x0234
            _cpu.DB.Should().Be(0x0234);         // DB unchanged
            _memory.ReadWord(0x3000).Should().Be(0x1234); // Result stored in memory
            _cpu.PC.Should().Be(0x000B);         // PC after HALT
            _cpu.IsRunning.Should().BeFalse();   // CPU stopped
        }
    }
}