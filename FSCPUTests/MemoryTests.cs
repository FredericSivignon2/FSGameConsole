using FSCPU;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Tests unitaires pour la classe Memory du processeur 8 bits
    /// </summary>
    public class MemoryTests
    {
        [Fact]
        public void WriteByte_ShouldStoreAndReturnValue()
        {
            // Arrange
            var memory = new Memory(0x1000);

            // Act
            memory.WriteByte(0x0100, 0x42);
            memory.WriteByte(0x0200, 0xFF);

            // Assert
            memory.ReadByte(0x0100).Should().Be(0x42);
            memory.ReadByte(0x0200).Should().Be(0xFF);
        }

        [Fact]
        public void ReadByte_ShouldThrowForInvalidAddress()
        {
            // Arrange
            var memory = new Memory(0x1000);

            // Act & Assert
            memory.Invoking(m => m.ReadByte(0x1000))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Adresse 1000 hors limites*");
        }

        [Fact]
        public void WriteByte_ShouldThrowForInvalidAddress()
        {
            // Arrange
            var memory = new Memory(0x1000);

            // Act & Assert
            memory.Invoking(m => m.WriteByte(0x1000, 0x42))
                .Should().Throw<ArgumentOutOfRangeException>()
                .WithMessage("*Adresse 1000 hors limites*");
        }

        [Fact]
        public void ReadWord_ShouldReturnLittleEndianValue()
        {
            // Arrange
            var memory = new Memory(0x1000);
            memory.WriteByte(0x0100, 0x34); // Low byte
            memory.WriteByte(0x0101, 0x12); // High byte

            // Act
            ushort result = memory.ReadWord(0x0100);

            // Assert
            result.Should().Be(0x1234);
        }

        [Fact]
        public void WriteWord_ShouldStoreLittleEndianValue()
        {
            // Arrange
            var memory = new Memory(0x1000);

            // Act
            memory.WriteWord(0x0100, 0x1234);

            // Assert
            memory.ReadByte(0x0100).Should().Be(0x34); // Low byte
            memory.ReadByte(0x0101).Should().Be(0x12); // High byte
        }

        [Fact]
        public void LoadProgram_ShouldLoadAtStartAddress()
        {
            // Arrange
            var memory = new Memory(0x1000);
            byte[] program = { 0x10, 0x42, 0x20, 0x30 };

            // Act
            memory.LoadProgram(program);

            // Assert
            memory.ReadByte(0x0000).Should().Be(0x10);
            memory.ReadByte(0x0001).Should().Be(0x42);
            memory.ReadByte(0x0002).Should().Be(0x20);
            memory.ReadByte(0x0003).Should().Be(0x30);
        }

        [Fact]
        public void LoadProgram_ShouldLoadAtSpecifiedAddress()
        {
            // Arrange
            var memory = new Memory(0x1000);
            byte[] program = { 0x10, 0x42 };

            // Act
            memory.LoadProgram(program, 0x0200);

            // Assert
            memory.ReadByte(0x01FF).Should().Be(0x00); // Avant le programme
            memory.ReadByte(0x0200).Should().Be(0x10); // Début du programme
            memory.ReadByte(0x0201).Should().Be(0x42);
            memory.ReadByte(0x0202).Should().Be(0x00); // Après le programme
        }

        [Fact]
        public void LoadProgram_ShouldThrowForTooLargeProgram()
        {
            // Arrange
            var memory = new Memory(0x10); // Très petite mémoire
            byte[] program = new byte[0x20]; // Programme trop grand

            // Act & Assert
            memory.Invoking(m => m.LoadProgram(program))
                .Should().Throw<ArgumentException>()
                .WithMessage("Le programme est trop grand pour la mémoire");
        }

        [Fact]
        public void Clear_ShouldResetAllMemory()
        {
            // Arrange
            var memory = new Memory(0x1000);
            memory.WriteByte(0x0100, 0x42);
            memory.WriteByte(0x0200, 0xFF);

            // Act
            memory.Clear();

            // Assert
            memory.ReadByte(0x0100).Should().Be(0x00);
            memory.ReadByte(0x0200).Should().Be(0x00);
        }

        [Fact]
        public void GetMemorySection_ShouldReturnCorrectData()
        {
            // Arrange
            var memory = new Memory(0x1000);
            memory.WriteByte(0x0100, 0x10);
            memory.WriteByte(0x0101, 0x20);
            memory.WriteByte(0x0102, 0x30);

            // Act
            byte[] section = memory.GetMemorySection(0x0100, 3);

            // Assert
            section.Should().HaveCount(3);
            section[0].Should().Be(0x10);
            section[1].Should().Be(0x20);
            section[2].Should().Be(0x30);
        }


       
        [Fact]
        public void GetMemoryDump_ShouldReturnFormattedString()
        {
            // Arrange
            var memory = new Memory(0x1000);
            memory.WriteByte(0x0000, 0x10);
            memory.WriteByte(0x0001, 0x20);

            // Act
            string dump = memory.GetMemoryDump(0x0000, 32);

            // Assert
            dump.Should().Contain("0000:");
            dump.Should().Contain("10 20");
        }
    }
}