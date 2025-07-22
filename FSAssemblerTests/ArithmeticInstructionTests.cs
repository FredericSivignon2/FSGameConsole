using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions arithmétiques
    /// ADD, SUB, ADD16, SUB16, INC, DEC, INC16, DEC16, CMP
    /// </summary>
    public class ArithmeticInstructionTests
    {
        private readonly Assembler _assembler;

        public ArithmeticInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Basic Arithmetic Instructions

        [Fact]
        public void ADD_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "ADD" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x20); // ADD opcode
        }

        [Fact]
        public void SUB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "SUB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x21); // SUB opcode
        }

        [Fact]
        public void ADD16_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "ADD16" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x22); // ADD16 opcode
        }

        [Fact]
        public void SUB16_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "SUB16" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x23); // SUB16 opcode
        }

        #endregion

        #region Increment/Decrement Instructions - 8-bit

        [Fact]
        public void INC_A_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "INC A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x28); // INC A opcode
        }

        [Fact]
        public void DEC_A_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "DEC A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x29); // DEC A opcode
        }

        [Fact]
        public void INC_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "INC B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2A); // INC B opcode
        }

        [Fact]
        public void DEC_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "DEC B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2B); // DEC B opcode
        }

        [Fact]
        public void INC_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "INC C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2D); // INC C opcode
        }

        [Fact]
        public void DEC_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "DEC C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2E); // DEC C opcode
        }

        #endregion

        #region Increment/Decrement Instructions - 16-bit

        [Fact]
        public void INC16_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "INC16 DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x24); // INC16 DA opcode
        }

        [Fact]
        public void DEC16_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "DEC16 DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x25); // DEC16 DA opcode
        }

        [Fact]
        public void INC16_DB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "INC16 DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x26); // INC16 DB opcode
        }

        [Fact]
        public void DEC16_DB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "DEC16 DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x27); // DEC16 DB opcode
        }

        #endregion

        #region Compare Instructions

        [Fact]
        public void CMP_A_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "CMP A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2C); // CMP A,B opcode
        }

        [Fact]
        public void CMP_A_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "CMP A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2F); // CMP A,C opcode
        }

        [Fact]
        public void CMP_WithSpaces_ShouldWork()
        {
            // Arrange
            string[] lines = { "CMP  A , B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2C); // CMP A,B opcode
        }

        [Fact]
        public void CMP_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = { "cmp a,c" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2F); // CMP A,C opcode
        }

        #endregion

        #region Error Cases

        [Fact]
        public void ADD_WithParameter_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "ADD 42" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*takes no parameters*");
        }

        [Fact]
        public void SUB_WithParameter_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SUB #10" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*takes no parameters*");
        }

        [Fact]
        public void INC_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "INC" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires a register*");
        }

        [Fact]
        public void DEC_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "DEC X" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid DEC register*");
        }

        [Fact]
        public void INC16_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "INC16 A" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid INC16 register*");
        }

        [Fact]
        public void DEC16_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "DEC16" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires a 16-bit register*");
        }

        [Fact]
        public void CMP_WithoutOperands_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*CMP instruction requires either two registers*");
        }

        [Fact]
        public void CMP_WithInvalidFormat_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP A B" }; // Missing comma

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*CMP instruction requires either two registers (CMP A,B)*");
        }

        [Fact]
        public void CMP_WithUnsupportedRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP B,A" }; // Only A,B and A,C supported

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*only A,B and A,C supported*");
        }

        #endregion

        #region Mixed Instructions Test

        [Fact]
        public void MultipleArithmeticInstructions_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "ADD",
                "SUB",
                "INC A",
                "DEC B",
                "ADD16",
                "CMP A,C"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result[0].Should().Be(0x20); // ADD
            result[1].Should().Be(0x21); // SUB
            result[2].Should().Be(0x28); // INC A
            result[3].Should().Be(0x2B); // DEC B
            result[4].Should().Be(0x22); // ADD16
            result[5].Should().Be(0x2F); // CMP A,C
        }

        #endregion
    }
}