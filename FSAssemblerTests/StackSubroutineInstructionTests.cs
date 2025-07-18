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
        public void PUSH16_DA_ShouldGenerateCorrectOpcode()
        {
            // Arrange
            string[] lines = { "PUSH16 DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x72); // PUSH16 DA opcode
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
            result[0].Should().Be(0x73); // POP16 DA opcode
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
            result[0].Should().Be(0x76); // PUSH16 DB opcode
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
            result[0].Should().Be(0x77); // POP16 DB opcode
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
                "MAIN:",         //   -    0   Label
                "PUSH A",        //   0    1   PUSH A
                "CALL FUNC",     //   1    3   CALL FUNC
                "POP A",         //   4    1   POP A
                "HALT",          //   5    1   HALT
                "FUNC:",         //   -    0   Label (position 6)
                "PUSH B",        //   6    1   PUSH B
                "NOP",           //   7    1   NOP
                "POP B",         //   8    1   POP B
                "RET"            //   9    1   RET
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(10);
            //   0    1   PUSH A
            result[0].Should().Be(0x70);
            //   1    3   CALL FUNC
            result[1].Should().Be(0x60); result[2].Should().Be(0x06); result[3].Should().Be(0x00);
            //   4    1   POP A
            result[4].Should().Be(0x71);
            //   5    1   HALT
            result[5].Should().Be(0x01);
            //   6    1   PUSH B (at FUNC)
            result[6].Should().Be(0x74);
            //   7    1   NOP
            result[7].Should().Be(0x00);
            //   8    1   POP B
            result[8].Should().Be(0x75);
            //   9    1   RET
            result[9].Should().Be(0x61);
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
                "push16 da",
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
            result[2].Should().Be(0x72); // PUSH16 DA
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
                "PUSH16  DA"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x70); // PUSH A
            result[1].Should().Be(0x75); // POP B
            result[2].Should().Be(0x72); // PUSH16 DA
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
                  .WithInnerException<AssemblerException>()
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
                  .WithInnerException<AssemblerException>()
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
                  .WithInnerException<AssemblerException>()
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
                  .WithInnerException<AssemblerException>()
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
                  .WithInnerException<AssemblerException>()
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
                  .WithInnerException<AssemblerException>()
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

        [Fact]
        public void PUSH16_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH16 A" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid PUSH16 register*");
        }

        [Fact]
        public void POP16_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "POP16 B" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid POP16 register*");
        }

        [Fact]
        public void PUSH16_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH16" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires a register*");
        }

        [Fact]
        public void POP16_WithoutRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "POP16" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires a register*");
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
                "PUSH16 DA",
                "CALL 0x1000",
                "POP16 DA",
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
            result[2].Should().Be(0x72); // PUSH16 DA
            result[3].Should().Be(0x60); // CALL opcode
            result[4].Should().Be(0x00); // Low byte of address
            result[5].Should().Be(0x10); // High byte of address
            result[6].Should().Be(0x73); // POP16 DA
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
                "CALL FUNC1",    //   0    3   CALL FUNC1
                "HALT",          //   3    1   HALT
                "FUNC1:",        //   -    0   Label (position 4)
                "PUSH A",        //   4    1   PUSH A
                "CALL FUNC2",    //   5    3   CALL FUNC2
                "POP A",         //   8    1   POP A
                "RET",           //   9    1   RET
                "FUNC2:",        //   -    0   Label (position 10)
                "NOP",           //  10    1   NOP
                "RET"            //  11    1   RET
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(12);
            //   0    3   CALL FUNC1
            result[0].Should().Be(0x60); result[1].Should().Be(0x04); result[2].Should().Be(0x00);
            //   3    1   HALT
            result[3].Should().Be(0x01);
            //   4    1   PUSH A
            result[4].Should().Be(0x70);
            //   5    3   CALL FUNC2
            result[5].Should().Be(0x60); result[6].Should().Be(0x0A); result[7].Should().Be(0x00);
            //   8    1   POP A
            result[8].Should().Be(0x71);
            //   9    1   RET
            result[9].Should().Be(0x61);
            //  10    1   NOP (at FUNC2)
            result[10].Should().Be(0x00);
            //  11    1   RET
            result[11].Should().Be(0x61);
        }

        #endregion
    }
}