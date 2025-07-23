using FluentAssertions;
using FSAssembler;
using System.Reflection;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions logiques
    /// AND, OR, XOR, NOT, SHL, SHR
    /// </summary>
    public class LogicalInstructionTests
    {
        private readonly Assembler _assembler;

        public LogicalInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Binary Logical Instructions

        [Fact]
        public void AND_A_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "AND A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x30); // AND A,B opcode
        }

        [Fact]
        public void AND_A_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "AND A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x36); // AND A,C opcode
        }

        [Fact]
        public void OR_A_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "OR A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x31); // OR A,B opcode
        }

        [Fact]
        public void OR_A_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "OR A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x37); // OR A,C opcode
        }

        [Fact]
        public void XOR_A_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "XOR A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x32); // XOR A,B opcode
        }

        [Fact]
        public void XOR_A_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "XOR A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x38); // XOR A,C opcode
        }

        #endregion

        #region Unary Logical Instructions

        [Fact]
        public void NOT_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "NOT" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x33); // NOT opcode
        }

        [Fact]
        public void SHR_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "SHR" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x35); // SHR opcode
        }

        #endregion

        #region Shift Instructions

        [Fact]
        public void SHL_A_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "SHL A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x34); // SHL A opcode
        }

        [Fact]
        public void SHL_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "SHL B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x39); // SHL B opcode
        }

        #endregion

        #region Format and Case Tests

        [Fact]
        public void LogicalInstruction_WithSpaces_ShouldWork()
        {
            // Arrange
            string[] lines = { "AND  A , B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x30); // AND A,B opcode
        }

        [Fact]
        public void LogicalInstruction_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "and a,b",
                "OR A,C",
                "xor a,c",
                "NOT",
                "shl a"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x30); // AND A,B
            result[1].Should().Be(0x37); // OR A,C
            result[2].Should().Be(0x38); // XOR A,C
            result[3].Should().Be(0x33); // NOT
            result[4].Should().Be(0x34); // SHL A
        }

        #endregion

        #region Error Cases

        [Fact]
        public void LogicalInstruction_WithoutOperands_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "AND" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires two registers*");
        }

        [Fact]
        public void LogicalInstruction_WithInvalidFormat_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "AND A B" }; // Missing comma

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*AND instruction requires two registers*");
        }

        [Fact]
        public void LogicalInstruction_WithUnsupportedRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "AND B,C" }; // Only A,B and A,C supported

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid AND registers*");
        }

        [Fact]
        public void OR_WithInvalidRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "OR C,B" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid OR registers*");
        }

        [Fact]
        public void XOR_WithInvalidRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "XOR B,B" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid XOR registers*");
        }

        [Fact]
        public void NOT_WithParameter_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "NOT A" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*takes no parameters*");
        }

        [Fact]
        public void SHR_WithParameter_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SHR A" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*takes no parameters*");
        }

        [Fact]
        public void SHL_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SHL" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires a register*");
        }

        [Fact]
        public void SHL_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SHL C" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*only A and B supported*");
        }

        #endregion

        #region Mixed Instructions Test

        [Fact]
        public void MultipleLogicalInstructions_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "AND A,B",
                "OR A,C",
                "XOR A,B",
                "NOT",
                "SHL A",
                "SHR",
                "AND A,C"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(7);
            result[0].Should().Be(0x30); // AND A,B
            result[1].Should().Be(0x37); // OR A,C
            result[2].Should().Be(0x32); // XOR A,B
            result[3].Should().Be(0x33); // NOT
            result[4].Should().Be(0x34); // SHL A
            result[5].Should().Be(0x35); // SHR
            result[6].Should().Be(0x36); // AND A,C
        }

        #endregion
    }
}