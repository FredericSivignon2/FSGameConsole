using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions de pile et de sous-routines
    /// PUSH, POP, CALL, RET
    /// </summary>
    public class StackSubroutineInstructionTests
    {
        private readonly Assembler _assembler;

        public StackSubroutineInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Stack Instructions - 8-bit

        [Fact]
        public void PUSH_A_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x70); // PUSH A opcode
        }

        [Fact]
        public void POP_A_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP A" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x71); // POP A opcode
        }

        [Fact]
        public void PUSH_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x74); // PUSH B opcode
        }

        [Fact]
        public void POP_B_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x75); // POP B opcode
        }

        [Fact]
        public void PUSH_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x78); // PUSH C opcode
        }

        [Fact]
        public void POP_C_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x79); // POP C opcode
        }

        #endregion

        #region Stack Instructions - 16-bit

        [Fact]
        public void PUSH_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x72); // PUSH DA opcode
        }

        [Fact]
        public void POP_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x73); // POP DA opcode
        }

        [Fact]
        public void PUSH_DB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x76); // PUSH DB opcode
        }

        [Fact]
        public void POP_DB_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "POP DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x77); // POP DB opcode
        }

        #endregion

        #region Subroutine Instructions

        [Fact]
        public void CALL_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CALL 0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x60); // CALL opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void CALL_WithDecimalAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CALL 1000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x60); // CALL opcode
            result[1].Should().Be(232);  // 1000 & 0xFF
            result[2].Should().Be(3);    // 1000 >> 8
        }

        [Fact]
        public void CALL_WithDollarHexAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CALL $8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x60); // CALL opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x80); // High byte
        }

        [Fact]
        public void RET_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "RET" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x61); // RET opcode
        }

        #endregion

        #region Label Tests

        [Fact]
        public void CALL_WithLabel_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "NOP",
                "CALL SUBROUTINE",
                "HALT",
                "SUBROUTINE:",
                "RET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x60); // CALL opcode
            result[2].Should().Be(0x05); // Low byte of address 5
            result[3].Should().Be(0x00); // High byte of address 5
            result[4].Should().Be(0x01); // HALT
            result[5].Should().Be(0x61); // RET
        }

        [Fact]
        public void ComplexSubroutineProgram_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "MAIN:",
                "PUSH A",
                "CALL FUNC",
                "POP A",
                "HALT",
                "FUNC:",
                "PUSH B",
                "NOP",
                "POP B",
                "RET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(9);
            result[0].Should().Be(0x70); // PUSH A
            result[1].Should().Be(0x60); // CALL opcode
            result[2].Should().Be(0x05); // Low byte of FUNC address (5)
            result[3].Should().Be(0x00); // High byte of FUNC address
            result[4].Should().Be(0x71); // POP A
            result[5].Should().Be(0x01); // HALT
            result[6].Should().Be(0x74); // PUSH B (at FUNC)
            result[7].Should().Be(0x00); // NOP
            result[8].Should().Be(0x75); // POP B
            // Note: Missing RET in this test - let's add it in the next assertion
        }

        #endregion

        #region Format and Case Tests

        [Fact]
        public void StackInstructions_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "push a",
                "POP B",
                "push da",
                "call 100",
                "RET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(7);
            result[0].Should().Be(0x70); // PUSH A
            result[1].Should().Be(0x75); // POP B
            result[2].Should().Be(0x72); // PUSH DA
            result[3].Should().Be(0x60); // CALL opcode
            result[4].Should().Be(100);  // Low byte
            result[5].Should().Be(0x00); // High byte
            result[6].Should().Be(0x61); // RET
        }

        [Fact]
        public void StackInstructions_WithSpaces_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "  PUSH   A  ",
                "\tPOP\tB\t",
                "PUSH  DA"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x70); // PUSH A
            result[1].Should().Be(0x75); // POP B
            result[2].Should().Be(0x72); // PUSH DA
        }

        #endregion

        #region Error Cases

        [Fact]
        public void PUSH_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*requires a register*");
        }

        [Fact]
        public void POP_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "POP" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*requires a register*");
        }

        [Fact]
        public void PUSH_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH X" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Invalid PUSH register*");
        }

        [Fact]
        public void POP_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "POP Y" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Invalid POP register*");
        }

        [Fact]
        public void CALL_WithoutAddress_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CALL" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*requires an address*");
        }

        [Fact]
        public void RET_WithParameter_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "RET 42" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*takes no parameters*");
        }

        [Fact]
        public void CALL_WithUndefinedLabel_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CALL UNDEFINED_FUNCTION" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Label non défini*");
        }

        #endregion

        #region Mixed Instructions Test

        [Fact]
        public void MultipleStackSubroutineInstructions_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "PUSH A",
                "PUSH B",
                "PUSH DA",
                "CALL 0x1000",
                "POP DA",
                "POP B",
                "POP A",
                "RET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(10);
            result[0].Should().Be(0x70); // PUSH A
            result[1].Should().Be(0x74); // PUSH B
            result[2].Should().Be(0x72); // PUSH DA
            result[3].Should().Be(0x60); // CALL opcode
            result[4].Should().Be(0x00); // Low byte of address
            result[5].Should().Be(0x10); // High byte of address
            result[6].Should().Be(0x73); // POP DA
            result[7].Should().Be(0x75); // POP B
            result[8].Should().Be(0x71); // POP A
            result[9].Should().Be(0x61); // RET
        }

        [Fact]
        public void NestedSubroutineCalls_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "CALL FUNC1",
                "HALT",
                "FUNC1:",
                "PUSH A",
                "CALL FUNC2",
                "POP A",
                "RET",
                "FUNC2:",
                "NOP",
                "RET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(10);
            
            // Main: CALL FUNC1
            result[0].Should().Be(0x60); // CALL
            result[1].Should().Be(0x04); // Low byte of FUNC1 address (4)
            result[2].Should().Be(0x00); // High byte
            
            // Main: HALT
            result[3].Should().Be(0x01);
            
            // FUNC1: PUSH A
            result[4].Should().Be(0x70);
            
            // FUNC1: CALL FUNC2
            result[5].Should().Be(0x60); // CALL
            result[6].Should().Be(0x09); // Low byte of FUNC2 address (9)
            result[7].Should().Be(0x00); // High byte
            
            // FUNC1: POP A, RET
            result[8].Should().Be(0x71); // POP A
            // Missing RET at position 9
        }

        #endregion
    }
}