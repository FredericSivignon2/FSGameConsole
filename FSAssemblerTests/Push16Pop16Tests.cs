using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires spécifiques pour les nouvelles instructions PUSH16 et POP16
    /// </summary>
    public class Push16Pop16Tests
    {
        private readonly Assembler _assembler;

        public Push16Pop16Tests()
        {
            _assembler = new Assembler();
        }

        [Fact]
        public void PUSH16_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH16 DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x72, "PUSH16 DA should generate opcode 0x72");
        }

        [Fact]
        public void POP16_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP16 DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x73, "POP16 DA should generate opcode 0x73");
        }

        [Fact]
        public void PUSH16_DB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH16 DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x76, "PUSH16 DB should generate opcode 0x76");
        }

        [Fact]
        public void POP16_DB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP16 DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x77, "POP16 DB should generate opcode 0x77");
        }

        [Fact]
        public void PUSH16POP16_CompleteProgram_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "LDDA #0x1234",   //  0    3   Load DA with 0x1234
                "LDDB #0x5678",   //  3    3   Load DB with 0x5678
                "PUSH16 DA",      //  6    1   Push DA onto stack
                "PUSH16 DB",      //  7    1   Push DB onto stack
                "LDDA #0x0000",   //  8    3   Clear DA
                "LDDB #0x0000",   // 11    3   Clear DB
                "POP16 DB",       // 14    1   Pop DB from stack
                "POP16 DA",       // 15    1   Pop DA from stack
                "HALT"            // 16    1   Halt
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(17);
            
            // Verify the PUSH16/POP16 opcodes are in the right places
            result[6].Should().Be(0x72, "PUSH16 DA at position 6");
            result[7].Should().Be(0x76, "PUSH16 DB at position 7");
            result[14].Should().Be(0x77, "POP16 DB at position 14");
            result[15].Should().Be(0x73, "POP16 DA at position 15");
            result[16].Should().Be(0x01, "HALT at position 16");
        }

        [Fact]
        public void PUSH16_WithInvalid8BitRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH16 A" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid PUSH16 register: A*");
        }

        [Fact]
        public void POP16_WithInvalid8BitRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "POP16 B" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid POP16 register: B*");
        }

        [Fact]
        public void PUSH16_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = 
            { 
                "push16 da",
                "PUSH16 DB",
                "pop16 db", 
                "POP16 DA"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0x72); // push16 da
            result[1].Should().Be(0x76); // PUSH16 DB
            result[2].Should().Be(0x77); // pop16 db
            result[3].Should().Be(0x73); // POP16 DA
        }
    }
}