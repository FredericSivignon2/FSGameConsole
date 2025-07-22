using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour la classe Assembler
    /// Tests des fonctionnalités de base et de l'initialisation
    /// </summary>
    public class AssemblerBasicTests
    {
        private readonly Assembler _assembler;

        public AssemblerBasicTests()
        {
            _assembler = new Assembler();
        }

        [Fact]
        public void Constructor_ShouldInitializeCorrectly()
        {
            // Arrange & Act
            var assembler = new Assembler();

            // Assert
            assembler.Should().NotBeNull();
        }

        [Fact]
        public void AssembleLines_WithEmptyArray_ShouldReturnEmptyArray()
        {
            // Arrange
            string[] lines = Array.Empty<string>();

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void AssembleLines_WithOnlyComments_ShouldReturnEmptyArray()
        {
            // Arrange
            string[] lines = 
            {
                "; This is a comment",
                "   ; Another comment",
                "; Yet another comment"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void AssembleLines_WithOnlyWhitespace_ShouldReturnEmptyArray()
        {
            // Arrange
            string[] lines = 
            {
                "",
                "   ",
                "\t\t",
                "  \t  "
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void AssembleLines_WithMixedCommentsAndWhitespace_ShouldReturnEmptyArray()
        {
            // Arrange
            string[] lines = 
            {
                "; Comment",
                "",
                "  ; Indented comment",
                "\t",
                "; Final comment"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void AssembleLines_WithSingleNOP_ShouldReturnCorrectOpcode()
        {
            // Arrange
            string[] lines = { "NOP" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x00); // NOP opcode
        }

        [Fact]
        public void AssembleLines_WithSingleHALT_ShouldReturnCorrectOpcode()
        {
            // Arrange
            string[] lines = { "HALT" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x01); // HALT opcode
        }

        [Fact]
        public void AssembleLines_WithMultipleBasicInstructions_ShouldReturnCorrectOpcodes()
        {
            // Arrange
            string[] lines = 
            {
                "NOP",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x01); // HALT
        }

        [Fact]
        public void AssembleLines_WithCaseInsensitiveInstructions_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "nop",
                "NoP",
                "HALT",
                "halt"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0x00);
            result[1].Should().Be(0x00);
            result[2].Should().Be(0x01);
            result[3].Should().Be(0x01);
        }

        [Fact]
        public void AssembleLines_WithCommentAfterInstruction_ShouldIgnoreComment()
        {
            // Arrange
            string[] lines = 
            {
                "NOP ; This is a comment",
                "HALT ; Another comment"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x00);
            result[1].Should().Be(0x01);
        }

        [Fact]
        public void AssembleLines_WithInvalidInstruction_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "INVALID_INSTRUCTION" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Unknown instruction*");
        }

        [Fact]
        public void AssembleFile_WithNonExistentFile_ShouldThrowException()
        {
            // Arrange
            string nonExistentFile = "nonexistent.fs8";

            // Act & Assert
            var action = () => _assembler.AssembleFile(nonExistentFile);
            action.Should().Throw<FileNotFoundException>()
                  .WithMessage($"*{nonExistentFile}*");
        }

        [Fact]
        public void AssembleLines_WithMultipleInstructions_ShouldAssembleSequentially()
        {
            // Arrange
            string[] lines = 
            {
                "NOP",
                "HALT",
                "SYS",
                "NOP"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x01); // HALT
            result[2].Should().Be(0xF0); // SYS
            result[3].Should().Be(0x00); // NOP
        }

        [Fact]
        public void AssembleLines_WithTabsAndSpaces_ShouldHandleWhitespace()
        {
            // Arrange
            string[] lines = 
            {
                "\tNOP",
                "  HALT  ",
                "\t  SYS\t"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x00);
            result[1].Should().Be(0x01);
            result[2].Should().Be(0xF0);
        }

        [Fact]
        public void AssemblerException_ShouldHaveCorrectMessage()
        {
            // Arrange
            string message = "Test error message";

            // Act
            var exception = new AssemblerException(message);

            // Assert
            exception.Message.Should().Be(message);
            exception.Should().BeOfType<AssemblerException>();
            exception.Should().BeAssignableTo<Exception>();
        }

        [Fact]
        public void AssemblerException_WithInnerException_ShouldPreserveInnerException()
        {
            // Arrange
            var innerException = new InvalidOperationException("Inner error");
            string message = "Outer error";

            // Act
            var exception = new AssemblerException(message, innerException);

            // Assert
            exception.Message.Should().Be(message);
            exception.InnerException.Should().Be(innerException);
        }
    }
}