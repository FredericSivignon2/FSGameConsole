using CPU8Bit;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Tests unitaires pour la classe ALU du processeur 8 bits
    /// </summary>
    public class ALUTests
    {
        private CPU8Bit.CPU8Bit _cpu;
        private Memory _memory;
        private ALU _alu;

        public ALUTests()
        {
            _memory = new Memory(0x1000);
            _cpu = new CPU8Bit.CPU8Bit(_memory);
            _alu = _cpu.ALU;
        }

        [Fact]
        public void Add_ShouldAddTwoNumbers()
        {
            // Arrange
            byte destination = 10;
            byte source = 5;

            // Act
            _alu.Add(ref destination, source);

            // Assert
            destination.Should().Be(15);
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
            _cpu.SR.Negative.Should().BeFalse();
        }

        [Fact]
        public void Add_ShouldSetZeroFlagWhenResultIsZero()
        {
            // Arrange
            byte destination = 0;
            byte source = 0;

            // Act
            _alu.Add(ref destination, source);

            // Assert
            destination.Should().Be(0);
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void Add_ShouldSetCarryFlagOnOverflow()
        {
            // Arrange
            byte destination = 200;
            byte source = 100; // 200 + 100 = 300 > 255

            // Act
            _alu.Add(ref destination, source);

            // Assert
            destination.Should().Be(44); // 300 & 0xFF = 44
            _cpu.SR.Carry.Should().BeTrue();
        }

        [Fact]
        public void Add_ShouldSetNegativeFlagWhenBit7IsSet()
        {
            // Arrange
            byte destination = 127;
            byte source = 1; // 127 + 1 = 128 (0x80)

            // Act
            _alu.Add(ref destination, source);

            // Assert
            destination.Should().Be(128);
            _cpu.SR.Negative.Should().BeTrue();
        }

        [Fact]
        public void Subtract_ShouldSubtractTwoNumbers()
        {
            // Arrange
            byte destination = 15;
            byte source = 5;

            // Act
            _alu.Subtract(ref destination, source);

            // Assert
            destination.Should().Be(10);
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void Subtract_ShouldSetZeroFlagWhenResultIsZero()
        {
            // Arrange
            byte destination = 10;
            byte source = 10;

            // Act
            _alu.Subtract(ref destination, source);

            // Assert
            destination.Should().Be(0);
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void Subtract_ShouldSetCarryFlagOnBorrow()
        {
            // Arrange
            byte destination = 5;
            byte source = 10; // 5 - 10 = -5 (underflow)

            // Act
            _alu.Subtract(ref destination, source);

            // Assert
            destination.Should().Be(251); // -5 & 0xFF = 251
            _cpu.SR.Carry.Should().BeTrue();
        }

        [Fact]
        public void And_ShouldPerformLogicalAnd()
        {
            // Arrange
            byte destination = 0xFF; // 11111111
            byte source = 0x0F;      // 00001111

            // Act
            _alu.And(ref destination, source);

            // Assert
            destination.Should().Be(0x0F); // 00001111
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void And_ShouldSetZeroFlagWhenResultIsZero()
        {
            // Arrange
            byte destination = 0xF0; // 11110000
            byte source = 0x0F;      // 00001111

            // Act
            _alu.And(ref destination, source);

            // Assert
            destination.Should().Be(0x00);
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void Or_ShouldPerformLogicalOr()
        {
            // Arrange
            byte destination = 0xF0; // 11110000
            byte source = 0x0F;      // 00001111

            // Act
            _alu.Or(ref destination, source);

            // Assert
            destination.Should().Be(0xFF); // 11111111
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void Or_ShouldSetNegativeFlagWhenBit7IsSet()
        {
            // Arrange
            byte destination = 0x80; // 10000000
            byte source = 0x00;      // 00000000

            // Act
            _alu.Or(ref destination, source);

            // Assert
            destination.Should().Be(0x80);
            _cpu.SR.Negative.Should().BeTrue();
        }

        [Fact]
        public void Xor_ShouldPerformLogicalXor()
        {
            // Arrange
            byte destination = 0xFF; // 11111111
            byte source = 0x0F;      // 00001111

            // Act
            _alu.Xor(ref destination, source);

            // Assert
            destination.Should().Be(0xF0); // 11110000
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void Xor_ShouldSetZeroFlagWhenValuesAreEqual()
        {
            // Arrange
            byte destination = 0x42;
            byte source = 0x42;

            // Act
            _alu.Xor(ref destination, source);

            // Assert
            destination.Should().Be(0x00);
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void ShiftLeft_ShouldShiftBitsLeft()
        {
            // Arrange
            byte value = 0x42; // 01000010

            // Act
            _alu.ShiftLeft(ref value);

            // Assert
            value.Should().Be(0x84); // 10000100
            _cpu.SR.Carry.Should().BeFalse(); // Bit 7 était 0
            _cpu.SR.Negative.Should().BeTrue(); // Nouveau bit 7 est 1
        }

        [Fact]
        public void ShiftLeft_ShouldSetCarryFlagWhenBit7IsSet()
        {
            // Arrange
            byte value = 0x80; // 10000000

            // Act
            _alu.ShiftLeft(ref value);

            // Assert
            value.Should().Be(0x00);
            _cpu.SR.Carry.Should().BeTrue(); // Bit 7 était 1
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void ShiftRight_ShouldShiftBitsRight()
        {
            // Arrange
            byte value = 0x42; // 01000010

            // Act
            _alu.ShiftRight(ref value);

            // Assert
            value.Should().Be(0x21); // 00100001
            _cpu.SR.Carry.Should().BeFalse(); // Bit 0 était 0
        }

        [Fact]
        public void ShiftRight_ShouldSetCarryFlagWhenBit0IsSet()
        {
            // Arrange
            byte value = 0x43; // 01000011

            // Act
            _alu.ShiftRight(ref value);

            // Assert
            value.Should().Be(0x21); // 00100001
            _cpu.SR.Carry.Should().BeTrue(); // Bit 0 était 1
        }

        [Fact]
        public void Increment_ShouldIncrementValue()
        {
            // Arrange
            byte value = 10;

            // Act
            _alu.Increment(ref value);

            // Assert
            value.Should().Be(11);
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void Increment_ShouldSetCarryFlagOnOverflow()
        {
            // Arrange
            byte value = 255;

            // Act
            _alu.Increment(ref value);

            // Assert
            value.Should().Be(0);
            _cpu.SR.Zero.Should().BeTrue();
            _cpu.SR.Carry.Should().BeTrue();
        }

        [Fact]
        public void Decrement_ShouldDecrementValue()
        {
            // Arrange
            byte value = 10;

            // Act
            _alu.Decrement(ref value);

            // Assert
            value.Should().Be(9);
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void Decrement_ShouldSetZeroFlagWhenResultIsZero()
        {
            // Arrange
            byte value = 1;

            // Act
            _alu.Decrement(ref value);

            // Assert
            value.Should().Be(0);
            _cpu.SR.Zero.Should().BeTrue();
            _cpu.SR.Carry.Should().BeFalse();
        }

        [Fact]
        public void Decrement_ShouldSetCarryFlagOnUnderflow()
        {
            // Arrange
            byte value = 0;

            // Act
            _alu.Decrement(ref value);

            // Assert
            value.Should().Be(255);
            _cpu.SR.Carry.Should().BeTrue();
            _cpu.SR.Negative.Should().BeTrue();
        }

        [Fact]
        public void Compare_ShouldSetFlagsBasedOnComparison()
        {
            // Arrange & Act & Assert

            // Cas 1: Valeurs égales
            _alu.Compare(10, 10);
            _cpu.SR.Zero.Should().BeTrue();
            _cpu.SR.Carry.Should().BeFalse();

            // Cas 2: Première valeur plus grande
            _alu.Compare(15, 10);
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeFalse();

            // Cas 3: Première valeur plus petite
            _alu.Compare(5, 10);
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeTrue();
        }

        [Theory]
        [InlineData(100, 50, 150, false, false)] // Addition normale
        [InlineData(200, 100, 44, true, false)]  // Overflow (300 & 0xFF = 44)
        [InlineData(0, 0, 0, false, true)]       // Résultat zéro
        [InlineData(127, 1, 128, false, false)]  // Résultat négatif (bit 7 = 1)
        public void Add_ShouldHandleVariousCases(byte dest, byte src, byte expectedResult, bool expectedCarry, bool expectedZero)
        {
            // Arrange
            byte destination = dest;

            // Act
            _alu.Add(ref destination, src);

            // Assert
            destination.Should().Be(expectedResult);
            _cpu.SR.Carry.Should().Be(expectedCarry);
            _cpu.SR.Zero.Should().Be(expectedZero);
        }

        [Theory]
        [InlineData(0xFF, 0x0F, 0x0F)] // AND avec masque
        [InlineData(0xF0, 0x0F, 0x00)] // AND résultant en zéro
        [InlineData(0xAA, 0x55, 0x00)] // Pattern alternant
        public void And_ShouldHandleVariousPatterns(byte dest, byte src, byte expected)
        {
            // Arrange
            byte destination = dest;

            // Act
            _alu.And(ref destination, src);

            // Assert
            destination.Should().Be(expected);
        }
    }
}