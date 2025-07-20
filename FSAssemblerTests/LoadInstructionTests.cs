using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions de chargement (Load Instructions)
    /// LDA, LDB, LDC, LDD, LDE, LDF, LDDA, LDDB
    /// </summary>
    public class LoadInstructionTests
    {
        private readonly Assembler _assembler;

        public LoadInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region 8-bit Load Instructions - Immediate Mode

        [Fact]
        public void LDA_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDA #42" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x10); // LDA immediate opcode
            result[1].Should().Be(42);   // Immediate value
        }

        [Fact]
        public void LDB_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDB #255" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x11); // LDB immediate opcode
            result[1].Should().Be(255);  // Immediate value
        }

        [Fact]
        public void LDC_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDC #0" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x12); // LDC immediate opcode
            result[1].Should().Be(0);    // Immediate value
        }

        [Fact]
        public void LDD_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDD #128" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x13); // LDD immediate opcode
            result[1].Should().Be(128);  // Immediate value
        }

        [Fact]
        public void LDE_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDE #15" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x14); // LDE immediate opcode
            result[1].Should().Be(15);   // Immediate value
        }

        [Fact]
        public void LDF_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDF #200" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x15); // LDF immediate opcode
            result[1].Should().Be(200);  // Immediate value
        }

        #endregion

        #region 8-bit Load Instructions - Memory Mode

        [Fact]
        public void LDA_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDA 0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x90); // LDA memory opcode
            result[1].Should().Be(0x34); // Low byte of address (little-endian)
            result[2].Should().Be(0x12); // High byte of address
        }

        [Fact]
        public void LDB_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDB 1000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x91); // LDB memory opcode
            result[1].Should().Be(232);  // 1000 & 0xFF = 232
            result[2].Should().Be(3);    // 1000 >> 8 = 3
        }

        [Fact]
        public void LDC_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDC $ABCD" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x92); // LDC memory opcode
            result[1].Should().Be(0xCD); // Low byte
            result[2].Should().Be(0xAB); // High byte
        }

        [Fact]
        public void LDD_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDD 0" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x93); // LDD memory opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x00); // High byte
        }

        [Fact]
        public void LDE_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDE 65535" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x94); // LDE memory opcode
            result[1].Should().Be(0xFF); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        [Fact]
        public void LDF_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDF 0x8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x95); // LDF memory opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x80); // High byte
        }

        #endregion

        #region 16-bit Load Instructions - Immediate Mode

        [Fact]
        public void LDDA_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDDA #0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x16); // LDDA immediate opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void LDDB_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDDB #65535" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x17); // LDDB immediate opcode
            result[1].Should().Be(0xFF); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        #endregion

        #region 16-bit Load Instructions - Memory Mode

        [Fact]
        public void LDDA_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDDA 0x8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x18); // LDDA memory opcode
            result[1].Should().Be(0x00); // Low byte of address
            result[2].Should().Be(0x80); // High byte of address
        }

        [Fact]
        public void LDDB_MemoryAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDDB $C000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x19); // LDDB memory opcode
            result[1].Should().Be(0x00); // Low byte of address
            result[2].Should().Be(0xC0); // High byte of address
        }

        #endregion

        #region Error Cases

        [Fact]
        public void LoadInstruction_WithoutOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "LDA" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires one operand*");
        }

        [Fact]
        public void LoadInstruction_WithTooManyOperands_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "LDA #42 #43" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>();
        }

        [Fact]
        public void LoadInstruction_WithInvalidImmediateValue_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "LDA #256" }; // Value too large for byte

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<OverflowException>();
        }

        [Fact]
        public void LoadInstruction16_WithInvalidImmediateValue_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "LDDA #65536" }; // Value too large for 16-bit

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                 .WithInnerException<OverflowException>();
        }

        #endregion

        #region Value Parsing Tests

        [Fact]
        public void LoadInstruction_WithHexValue_ShouldParseCorrectly()
        {
            // Arrange
            string[] lines = { "LDA #0xFF" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x10); // LDA immediate opcode
            result[1].Should().Be(255);  // 0xFF = 255
        }

        [Fact]
        public void LoadInstruction_WithDollarHexValue_ShouldParseCorrectly()
        {
            // Arrange
            string[] lines = { "LDA #$80" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x10); // LDA immediate opcode
            result[1].Should().Be(128);  // $80 = 128
        }

        [Fact]
        public void LoadInstruction_WithCharacterValue_ShouldParseCorrectly()
        {
            // Arrange
            string[] lines = { "LDA #'A'" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x10); // LDA immediate opcode
            result[1].Should().Be(65);   // 'A' = 65
        }

        #endregion
    }
}