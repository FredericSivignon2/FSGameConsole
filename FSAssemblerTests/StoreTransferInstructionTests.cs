using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions de stockage et de transfert
    /// STA, STB, STC, STD, STDA, STDB, MOV, SWP
    /// </summary>
    public class StoreTransferInstructionTests
    {
        private readonly Assembler _assembler;

        public StoreTransferInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Store Instructions - 8-bit

        [Fact]
        public void STA_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STA 0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x50); // STA opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void STB_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STB 1000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x53); // STB opcode
            result[1].Should().Be(232);  // 1000 & 0xFF
            result[2].Should().Be(3);    // 1000 >> 8
        }

        [Fact]
        public void STC_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STC $8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x54); // STC opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x80); // High byte
        }

        [Fact]
        public void STD_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STD 0xFFFF" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x55); // STD opcode
            result[1].Should().Be(0xFF); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        #endregion

        #region Store Instructions - 16-bit

        [Fact]
        public void STDA_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STDA 0xC000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x51); // STDA opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0xC0); // High byte
        }

        [Fact]
        public void STDB_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STDB $A000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x52); // STDB opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0xA0); // High byte
        }

        #endregion

        #region Transfer Instructions - MOV

        [Fact]
        public void MOV_A_B_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA0); // MOV A,B opcode
        }

        [Fact]
        public void MOV_A_C_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA1); // MOV A,C opcode
        }

        [Fact]
        public void MOV_B_A_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV B,A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA2); // MOV B,A opcode
        }

        [Fact]
        public void MOV_B_C_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV B,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA3); // MOV B,C opcode
        }

        [Fact]
        public void MOV_C_A_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV C,A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA4); // MOV C,A opcode
        }

        [Fact]
        public void MOV_C_B_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV C,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA5); // MOV C,B opcode
        }

        #endregion

        #region Transfer Instructions - SWP

        [Fact]
        public void SWP_A_B_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA6); // SWP A,B opcode
        }

        [Fact]
        public void SWP_A_C_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA7); // SWP A,C opcode
        }

        #endregion

        #region Format and Case Tests

        [Fact]
        public void StoreInstruction_WithDifferentFormats_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "STA 0x1000",
                "STB $2000",
                "STC 3000"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(9);
            
            // STA 0x1000
            result[0].Should().Be(0x50);
            result[1].Should().Be(0x00);
            result[2].Should().Be(0x10);
            
            // STB $2000
            result[3].Should().Be(0x53);
            result[4].Should().Be(0x00);
            result[5].Should().Be(0x20);
            
            // STC 3000
            result[6].Should().Be(0x54);
            result[7].Should().Be(184); // 3000 & 0xFF
            result[8].Should().Be(11);  // 3000 >> 8
        }

        [Fact]
        public void TransferInstruction_WithSpaces_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "MOV  A , B",
                "SWP  A , C"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xA0); // MOV A,B
            result[1].Should().Be(0xA7); // SWP A,C
        }

        [Fact]
        public void StoreTransferInstructions_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "sta 100",
                "MOV a,b",
                "swp A,C"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x50); // STA
            result[1].Should().Be(100);  // Low byte
            result[2].Should().Be(0x00); // High byte
            result[3].Should().Be(0xA0); // MOV A,B
            result[4].Should().Be(0xA7); // SWP A,C
        }

        #endregion

        #region Label Tests

        [Fact]
        public void StoreInstruction_WithLabel_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "NOP",
                "NOP",
                "STA TARGET",
                "TARGET:",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x00); // NOP
            result[2].Should().Be(0x50); // STA opcode
            result[3].Should().Be(0x05); // Low byte of address 5
            result[4].Should().Be(0x00); // High byte of address 5
            result[5].Should().Be(0x01); // HALT
        }

        #endregion

        #region Error Cases

        [Fact]
        public void StoreInstruction_WithoutAddress_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "STA" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires an address*");
        }

        [Fact]
        public void MOV_WithoutOperands_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "MOV" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires two registers*");
        }

        [Fact]
        public void MOV_WithInvalidFormat_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "MOV A B" }; // Missing comma

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*MOV instruction requires two registers*");
        }

        [Fact]
        public void MOV_WithInvalidRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "MOV D,E" }; // Unsupported registers

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid MOV registers*");
        }

        [Fact]
        public void SWP_WithoutOperands_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SWP" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires two registers*");
        }

        [Fact]
        public void SWP_WithInvalidFormat_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SWP A B" }; // Missing comma

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*SWP instruction requires two registers*");
        }

        [Fact]
        public void SWP_WithUnsupportedRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SWP B,C" }; // Only A,B and A,C supported

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*only A,B and A,C supported*");
        }

        [Fact]
        public void StoreInstruction_WithUndefinedLabel_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "STA UNDEFINED_LABEL" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Label non défini*");
        }

        #endregion

        #region Mixed Instructions Test

        [Fact]
        public void MultipleStoreTransferInstructions_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "STA 0x1000",
                "STB 0x2000",
                "STDA 0x3000",
                "MOV A,B",
                "MOV B,C",
                "SWP A,C"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(12);
            
            // STA 0x1000
            result[0].Should().Be(0x50);
            result[1].Should().Be(0x00);
            result[2].Should().Be(0x10);
            
            // STB 0x2000
            result[3].Should().Be(0x53);
            result[4].Should().Be(0x00);
            result[5].Should().Be(0x20);
            
            // STDA 0x3000
            result[6].Should().Be(0x51);
            result[7].Should().Be(0x00);
            result[8].Should().Be(0x30);
            
            // MOV A,B
            result[9].Should().Be(0xA0);
            
            // MOV B,C
            result[10].Should().Be(0xA3);
            
            // SWP A,C
            result[11].Should().Be(0xA7);
        }

        #endregion
    }
}