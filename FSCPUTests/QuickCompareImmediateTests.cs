using FSCPU;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Quick test to verify that the new compare immediate instructions (0xD0-0xD7) work correctly
    /// </summary>
    public class QuickCompareImmediateTests
    {
        private Memory _memory;
        private FSCPU.CPU8Bit _cpu;

        public QuickCompareImmediateTests()
        {
            _memory = new Memory(0x10000);
            _cpu = new FSCPU.CPU8Bit(_memory);
        }

        [Fact]
        public void CMP_A_Immediate_ShouldWorkCorrectly()
        {
            // Arrange - Place program in RAM
            _cpu.A = 0x42;
            _memory.WriteByte(0x0000, 0xD0); // CMP A,#imm
            _memory.WriteByte(0x0001, 0x42); // Compare with same value
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x42); // A unchanged
            _cpu.PC.Should().Be(2);   // PC advanced by 2
            _cpu.SR.Zero.Should().BeTrue(); // Equal values should set Zero flag
            _cpu.SR.Carry.Should().BeFalse(); // No borrow needed
        }

        [Fact]
        public void CMP_DA_Immediate16_ShouldWorkCorrectly()
        {
            // Arrange - Place program in RAM
            _cpu.DA = 0x1234;
            _memory.WriteByte(0x0000, 0xD6); // CMP DA,#imm16
            _memory.WriteByte(0x0001, 0x34); // Low byte
            _memory.WriteByte(0x0002, 0x12); // High byte
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.DA.Should().Be(0x1234); // DA unchanged
            _cpu.PC.Should().Be(3);      // PC advanced by 3
            _cpu.SR.Zero.Should().BeTrue(); // Equal values should set Zero flag
        }

        [Fact]
        public void CMP_B_Immediate_LessThan_ShouldSetCarryFlag()
        {
            // Arrange - Place program in RAM
            _cpu.B = 0x05;
            _memory.WriteByte(0x0000, 0xD1); // CMP B,#imm
            _memory.WriteByte(0x0001, 0x10); // Compare B(5) with 16
            _cpu.PC = 0x0000;
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.B.Should().Be(0x05); // B unchanged
            _cpu.PC.Should().Be(2);   // PC advanced by 2
            _cpu.SR.Zero.Should().BeFalse(); // Different values
            _cpu.SR.Carry.Should().BeTrue(); // B < immediate should set carry
        }
    }
}