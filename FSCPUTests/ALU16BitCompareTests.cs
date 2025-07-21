using FSCPU;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Unit tests for ALU 16-bit compare operations to verify correct flag handling
    /// </summary>
    public class ALU16BitCompareTests
    {
        private Memory _memory;
        private CPU8Bit _cpu;
        private ALU _alu;

        public ALU16BitCompareTests()
        {
            _memory = new Memory(0x10000);
            _cpu = new CPU8Bit(_memory);
            _alu = _cpu.ALU;
        }

        [Fact]
        public void Compare_16Bit_EqualValues_ShouldSetZeroFlag()
        {
            // Arrange
            ushort value1 = 0x1234;
            ushort value2 = 0x1234;

            // Act
            _alu.Compare(value1, value2);

            // Assert
            _cpu.SR.Zero.Should().BeTrue();  // Equal values should set Zero flag
            _cpu.SR.Carry.Should().BeFalse(); // No borrow needed
            _cpu.SR.Negative.Should().BeFalse(); // Result is zero, so not negative
        }

        [Fact]
        public void Compare_16Bit_Value1Greater_ShouldClearFlags()
        {
            // Arrange
            ushort value1 = 0x2000;
            ushort value2 = 0x1000;

            // Act
            _alu.Compare(value1, value2);

            // Assert
            _cpu.SR.Zero.Should().BeFalse();  // Different values
            _cpu.SR.Carry.Should().BeFalse(); // No borrow (value1 > value2)
            _cpu.SR.Negative.Should().BeFalse(); // Positive result
        }

        [Fact]
        public void Compare_16Bit_Value1Smaller_ShouldSetCarryFlag()
        {
            // Arrange
            ushort value1 = 0x1000;
            ushort value2 = 0x2000;

            // Act
            _alu.Compare(value1, value2);

            // Assert
            _cpu.SR.Zero.Should().BeFalse(); // Different values
            _cpu.SR.Carry.Should().BeTrue(); // Borrow needed (value1 < value2)
            _cpu.SR.Negative.Should().BeTrue(); // Negative result (bit 15 set)
        }

        [Fact]
        public void Compare_16Bit_ResultWithBit15Set_ShouldSetNegativeFlag()
        {
            // Arrange
            ushort value1 = 0x7000; // Smaller value
            ushort value2 = 0xF000; // Larger value (result will have bit 15 set)

            // Act
            _alu.Compare(value1, value2);

            // Assert
            _cpu.SR.Zero.Should().BeFalse();
            _cpu.SR.Carry.Should().BeTrue(); // Borrow occurred
            _cpu.SR.Negative.Should().BeTrue(); // Bit 15 of result is set
        }

        [Fact]
        public void Compare_16Bit_HighByteZero_ShouldNotSetNegativeFlag()
        {
            // This tests the fix for the original bug where (result & 0xFFFF) was cast to byte
            // Arrange
            ushort value1 = 0x0100; // 256 in decimal
            ushort value2 = 0x0000;

            // Act
            _alu.Compare(value1, value2);

            // Assert
            _cpu.SR.Zero.Should().BeFalse(); // 256 - 0 = 256, not zero
            _cpu.SR.Carry.Should().BeFalse(); // No borrow
            _cpu.SR.Negative.Should().BeFalse(); // Positive result (bit 15 clear)
        }

        [Theory]
        [InlineData(0x0000, 0x0000, true, false, false)]  // Equal: Zero=true, Carry=false, Negative=false
        [InlineData(0x1000, 0x0500, false, false, false)] // Greater: Zero=false, Carry=false, Negative=false  
        [InlineData(0x0500, 0x1000, false, true, true)]   // Less: Zero=false, Carry=true, Negative=true
        [InlineData(0x8000, 0x4000, false, false, false)] // High bit in value1: Zero=false, Carry=false, Negative=false
        [InlineData(0x4000, 0x8000, false, true, true)]   // High bit in result: Zero=false, Carry=true, Negative=true
        public void Compare_16Bit_VariousCases_ShouldSetFlagsCorrectly(
            ushort value1, ushort value2, bool expectedZero, bool expectedCarry, bool expectedNegative)
        {
            // Act
            _alu.Compare(value1, value2);

            // Assert
            _cpu.SR.Zero.Should().Be(expectedZero);
            _cpu.SR.Carry.Should().Be(expectedCarry);
            _cpu.SR.Negative.Should().Be(expectedNegative);
        }
    }
}