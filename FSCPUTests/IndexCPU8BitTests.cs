using CPU8Bit;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Tests unitaires pour les instructions d'index du CPU8Bit
    /// Tests des nouvelles fonctionnalités d'adressage indexé avec IDX1, IDX2, IDY1, IDY2
    /// </summary>
    public class IndexCPU8BitTests
    {
        private readonly CPU8Bit.CPU8Bit _cpu;
        private readonly Memory _memory;

        public IndexCPU8BitTests()
        {
            _memory = new Memory();
            _cpu = new CPU8Bit.CPU8Bit(_memory);
        }

        #region Index Register Load Instructions Tests

        [Fact]
        public void LDIX1_ImmediateValue_ShouldLoadCorrectly()
        {
            // Arrange - Place program in RAM
            _memory.WriteByte(0x0000, 0x1A); // LDIX1 #imm16
            _memory.WriteByte(0x0001, 0x34); // Low byte
            _memory.WriteByte(0x0002, 0x12); // High byte
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDX.Should().Be(0x1234);
            _cpu.PC.Should().Be(3);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void LDIY1_ImmediateValue_ShouldLoadCorrectly()
        {
            // Arrange - Place program in RAM
            _memory.WriteByte(0x0000, 0x1B); // LDIY1 #imm16
            _memory.WriteByte(0x0001, 0xFF); // Low byte
            _memory.WriteByte(0x0002, 0xFF); // High byte
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDY.Should().Be(0xFFFF);
            _cpu.PC.Should().Be(3);
            _cpu.SR.Zero.Should().BeFalse();
        }

        #endregion

        #region Auto-Increment/Decrement Tests

        [Fact]
        public void LDAIX1_PostIncrement_ShouldLoadAndIncrement()
        {
            // Arrange - Setup index register and memory
            _cpu.IDX = 0x2000;
            _memory.WriteByte(0x2000, 0x42); // Value at current IDX1
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xC4); // LDAIX1+
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x42);        // Value loaded
            _cpu.IDX.Should().Be(0x2001);   // IDX1 incremented
            _cpu.PC.Should().Be(1);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void STAIY1_PostIncrement_ShouldStoreAndIncrement()
        {
            // Arrange - Setup registers
            _cpu.A = 0x33;
            _cpu.IDY = 0x3000;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xC7); // STAIY1+
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _memory.ReadByte(0x3000).Should().Be(0x33); // Value stored at original address
            _cpu.IDY.Should().Be(0x3001);              // IDY1 incremented
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void LDAIX1_PostDecrement_ShouldLoadAndDecrement()
        {
            // Arrange - Setup index register and memory
            _cpu.IDX = 0x2005;
            _memory.WriteByte(0x2005, 0x66); // Value at current IDX1
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xC8); // LDAIX1-
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x66);        // Value loaded
            _cpu.IDX.Should().Be(0x2004);   // IDX1 decremented
            _cpu.PC.Should().Be(1);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void STAIY1_PostDecrement_ShouldStoreAndDecrement()
        {
            // Arrange - Setup registers
            _cpu.A = 0x44;
            _cpu.IDY = 0x4010;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xCB); // STAIY1-
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _memory.ReadByte(0x4010).Should().Be(0x44); // Value stored at original address
            _cpu.IDY.Should().Be(0x400F);              // IDY1 decremented
            _cpu.PC.Should().Be(1);
        }

        #endregion

        #region Index Register Arithmetic Tests

        [Fact]
        public void INCIX1_ShouldIncrementIDX1()
        {
            // Arrange - Setup index register
            _cpu.IDX = 0x1234;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xE0); // INCIX1
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDX.Should().Be(0x1235);
            _cpu.PC.Should().Be(1);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void DECIY1_ShouldDecrementIDY1()
        {
            // Arrange - Setup index register
            _cpu.IDY = 0x5000;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xE3); // DECIY1
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDY.Should().Be(0x4FFF);
            _cpu.PC.Should().Be(1);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void ADDIX1_ImmediateValue_ShouldAddToIDX1()
        {
            // Arrange - Setup index register
            _cpu.IDX = 0x1000;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xE8); // ADDIX1 #imm16
            _memory.WriteByte(0x0001, 0x00); // Low byte of 0x0500
            _memory.WriteByte(0x0002, 0x05); // High byte
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDX.Should().Be(0x1500); // 0x1000 + 0x0500
            _cpu.PC.Should().Be(3);
            _cpu.SR.Zero.Should().BeFalse();
        }

        #endregion

        #region Index Register Transfer Tests

        [Fact]
        public void SWPIX1IY1_ShouldSwapIDX1AndIDY1()
        {
            // Arrange - Setup both registers with different values
            _cpu.IDX = 0xAAAA;
            _cpu.IDY = 0xBBBB;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xF9); // SWPIX1IY1
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDX.Should().Be(0xBBBB); // IDX1 should now have original IDY1 value
            _cpu.IDY.Should().Be(0xAAAA); // IDY1 should now have original IDX1 value
            _cpu.PC.Should().Be(1);
            _cpu.SR.Zero.Should().BeFalse();
        }

        #endregion

        #region Integration Tests with Index Instructions

        [Fact]
        public void ArrayCopyProgram_UsingIndexInstructions_ShouldWorkCorrectly()
        {
            // Arrange - Programme de copie de tableau utilisant les instructions d'index
            byte[] program = 
            {                                 // pos  size
                0x1A, 0x0F, 0x00,             //  0    3   LDIX1 #SOURCE (0x0019)
                0x1B, 0x30, 0x00,             //  3    3   LDIY1 #DEST (0x0030)
                0x12, 0x05,                   //  6    2   LDC #5 (counter)
                // COPY_LOOP: (position 8)
                0xC4,                         //  8    1   LDAIX1+ (load and increment source)
                0xC7,                         //  9    1   STAIY1+ (store and increment dest)
                0x2E,                         // 10    1   DEC C (decrement counter)
                0x42, 0x08, 0x00,             // 11    3   JNZ COPY_LOOP (jump if not zero)
                0x01,                         // 14    1   HALT
                // SOURCE data at 0x0019
                0x11, 0x22, 0x33, 0x44, 0x55  // 15   5   Source data
            };

            // Load program starting at address 0x0000 (not ROM)
            for (int i = 0; i < program.Length; i++)
            {
                _memory.WriteByte((ushort)i, program[i]);
            }

            // IMPORTANT: Set PC to 0x0000 to run in RAM, not ROM
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Execute the complete program
            int cycleCount = 0;
            const int maxCycles = 100; // Safety limit to prevent infinite loop
            
            while (_cpu.IsRunning && cycleCount < maxCycles)
            {
                _cpu.ExecuteCycle();
                cycleCount++;
            }

            // Safety check - if we hit max cycles, the program likely got stuck
            if (cycleCount >= maxCycles)
            {
                // Debug info to understand what's happening
                throw new InvalidOperationException(
                    $"Program execution took too many cycles. " +
                    $"PC: 0x{_cpu.PC:X4}, A: {_cpu.A}, " +
                    $"IDX1: 0x{_cpu.IDX:X4}, IDY1: 0x{_cpu.IDY:X4}, " +
                    $"Zero flag: {_cpu.SR.Zero}");
            }

            // Assert - Check that data was copied correctly
            _memory.ReadByte(0x0030).Should().Be(0x11); // DEST[0]
            _memory.ReadByte(0x0031).Should().Be(0x22); // DEST[1]
            _memory.ReadByte(0x0032).Should().Be(0x33); // DEST[2]
            _memory.ReadByte(0x0033).Should().Be(0x44); // DEST[3]
            _memory.ReadByte(0x0034).Should().Be(0x55); // DEST[4]

            // Check that index registers were updated correctly
            _cpu.IDX.Should().Be(0x0014); // Source pointer after copying 5 bytes (0x19 + 5)
            _cpu.IDY.Should().Be(0x0035); // Dest pointer after copying 5 bytes (0x30 + 5)
            _cpu.C.Should().Be(0);         // Counter should be zero
        }

        [Fact]
        public void MultipleIndexRegisters_ShouldWorkIndependently()
        {
            // Arrange - Test que les 4 registres d'index fonctionnent indépendamment
            byte[] program = 
            {                                 // pos  size
                0x1A, 0x00, 0x10,             //  0    3   LDIX1 #0x1000
                0x1B, 0x00, 0x30,             //  6    3   LDIY1 #0x3000
                // Test increment operations
                0xE0,                         // 12    1   INCIX1
                0xE2,                         // 14    1   INCIY1
                0x01                          // 16    1   HALT
            };

            // Load program
            for (int i = 0; i < program.Length; i++)
            {
                _memory.WriteByte((ushort)i, program[i]);
            }

            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act - Execute the complete program
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert - Check that all index registers were loaded and incremented correctly
            _cpu.IDX.Should().Be(0x1001); // 0x1000 + 1
            _cpu.IDY.Should().Be(0x3001); // 0x3000 + 1
        }

        #endregion

        #region Edge Cases and Error Conditions

        [Fact]
        public void IndexRegister_WithZeroValue_ShouldSetZeroFlag()
        {
            // Arrange
            _memory.WriteByte(0x0000, 0x1A); // LDIX1 #imm16
            _memory.WriteByte(0x0001, 0x00); // Low byte
            _memory.WriteByte(0x0002, 0x00); // High byte
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDX.Should().Be(0x0000);
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void IndexRegister_IncrementOverflow_ShouldWrapAround()
        {
            // Arrange - Set IDX1 to maximum value
            _cpu.IDX = 0xFFFF;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xE0); // INCIX1
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDX.Should().Be(0x0000); // Should wrap around
            _cpu.SR.Zero.Should().BeTrue(); // Zero flag should be set
        }

        [Fact]
        public void IndexRegister_DecrementUnderflow_ShouldWrapAround()
        {
            // Arrange - Set IDY1 to minimum value
            _cpu.IDY = 0x0000;
            
            // Place instruction in RAM
            _memory.WriteByte(0x0000, 0xE3); // DECIY1
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IDY.Should().Be(0xFFFF); // Should wrap around to maximum
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void SimpleIndexTest_ShouldWork()
        {
            // Arrange - Test simple des instructions d'index
            _memory.WriteByte(0x1000, 0x42); // Données test à l'adresse 0x1000
            
            // Programme simple
            _memory.WriteByte(0x0000, 0x1A); // LDIX1 #0x1000
            _memory.WriteByte(0x0001, 0x00); // Low byte
            _memory.WriteByte(0x0002, 0x10); // High byte
            _memory.WriteByte(0x0003, 0xC4); // LDAIX1+ (load A from IDX1, then increment IDX1)
            _memory.WriteByte(0x0004, 0x01); // HALT
            
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle(); // LDIX1 #0x1000
            _cpu.ExecuteCycle(); // LDAIX1+
            _cpu.ExecuteCycle(); // HALT

            // Assert
            _cpu.IDX.Should().Be(0x1001); // Should be incremented
            _cpu.A.Should().Be(0x42);      // Should contain the loaded value
            _cpu.IsRunning.Should().BeFalse(); // Should be halted
        }

        #endregion
    }
}