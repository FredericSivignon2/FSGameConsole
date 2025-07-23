using FluentAssertions;
using FSAssembler;
using System.Reflection;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions de saut
    /// JMP, JZ, JNZ, JC, JNC, JN, JNN, JR, JRZ, JRNZ, JRC
    /// </summary>
    public class JumpInstructionTests
    {
        private readonly Assembler _assembler;

        public JumpInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Absolute Jump Instructions

        [Fact]
        public void JMP_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JMP 0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x40); // JMP opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void JZ_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JZ 1000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x41); // JZ opcode
            result[1].Should().Be(232);  // 1000 & 0xFF
            result[2].Should().Be(3);    // 1000 >> 8
        }

        [Fact]
        public void JNZ_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JNZ $8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x42); // JNZ opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x80); // High byte
        }

        [Fact]
        public void JC_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JC 0xFFFF" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x43); // JC opcode
            result[1].Should().Be(0xFF); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        [Fact]
        public void JNC_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JNC 0" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x44); // JNC opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x00); // High byte
        }

        [Fact]
        public void JN_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JN 32768" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x45); // JN opcode
            result[1].Should().Be(0x00); // Low byte of 32768
            result[2].Should().Be(0x80); // High byte of 32768
        }

        [Fact]
        public void JNN_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JNN $C000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x46); // JNN opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0xC0); // High byte
        }

        #endregion

        #region Relative Jump Instructions

        [Fact]
        public void JR_WithPositiveOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JR +10" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC0); // JR opcode
            result[1].Should().Be(10);   // Positive offset
        }

        [Fact]
        public void JR_WithNegativeOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JR -5" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC0); // JR opcode
            result[1].Should().Be(251);  // -5 as signed byte (256-5)
        }

        [Fact]
        public void JRZ_WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JRZ 20" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC1); // JRZ opcode
            result[1].Should().Be(20);   // Offset
        }

        [Fact]
        public void JRNZ_WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JRNZ -10" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC2); // JRNZ opcode
            result[1].Should().Be(246);  // -10 as signed byte (256-10)
        }

        [Fact]
        public void JRC_WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JRC 127" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC3); // JRC opcode
            result[1].Should().Be(127);  // Maximum positive offset
        }

        [Fact]
        public void JR_WithHexOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JR 0x10" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC0); // JR opcode
            result[1].Should().Be(16);   // 0x10 = 16
        }

        [Fact]
        public void JR_WithDollarHexOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "JR $20" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xC0); // JR opcode
            result[1].Should().Be(32);   // $20 = 32
        }

        #endregion

        #region Label Tests

        [Fact]
        public void JMP_WithLabel_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "NOP",
                "NOP",
                "JMP LABEL",
                "LABEL:",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x00); // NOP
            result[2].Should().Be(0x40); // JMP opcode
            result[3].Should().Be(0x05); // Low byte of address 5
            result[4].Should().Be(0x00); // High byte of address 5
            result[5].Should().Be(0x01); // HALT
        }

        [Fact]
        public void JZ_WithLabel_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "START:",
                "JZ END",
                "NOP",
                "END:",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x41); // JZ opcode
            result[1].Should().Be(0x04); // Low byte of address 4
            result[2].Should().Be(0x00); // High byte of address 4
            result[3].Should().Be(0x00); // NOP
            result[4].Should().Be(0x01); // HALT
        }

        [Fact]
        public void JR_WithLabel_ShouldCalculateRelativeOffset()
        {
            // Arrange
            string[] lines =
            {                   // pos  size
                "START:",       //  -    0   
                "JR FORWARD",   //  0    2   
                "NOP",          //  2    1   
                "FORWARD:",     //  3    0   
                "HALT"          //  3    1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0xC0); // JR opcode
            result[1].Should().Be(1);    // Relative offset
            result[2].Should().Be(0x00); // NOP
            result[3].Should().Be(0x01); // HALT
        }

        [Fact]
        public void JR_WithBackwardLabel_ShouldCalculateNegativeOffset()
        {
            // Arrange
            string[] lines = 
            {
                "LOOP:",         // At address 0
                "NOP",          // At address 1 (1 byte)
                "JR LOOP"       // At address 1, target at 0 -> offset = 0 - (1 + 2) = -3
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0xC0); // JR opcode
            result[2].Should().Be(253);  // -3 as signed byte (256-3)
        }

        #endregion

        #region Error Cases

        [Fact]
        public void JumpInstruction_WithoutAddress_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JMP" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires an address*");
        }

        [Fact]
        public void RelativeJump_WithoutOffset_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JR" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires an offset*");
        }

        [Fact]
        public void RelativeJump_WithOffsetOutOfRange_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JR 200" }; // Offset too large (> 127)

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*offset out of range*");
        }

        [Fact]
        public void RelativeJump_WithNegativeOffsetOutOfRange_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JR -200" }; // Offset too small (< -128)

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*offset out of range*");
        }

        [Fact]
        public void JumpInstruction_WithUndefinedLabel_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JMP UNDEFINED_LABEL" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Undefined label: UNDEFINED_LABEL*");
        }

        [Fact]
        public void RelativeJump_WithUndefinedLabel_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JR UNDEFINED_LABEL" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Undefined label*");
        }

        [Fact]
        public void Label_DefinedTwice_ShouldThrowException()
        {
            // Arrange
            string[] lines = 
            {
                "LABEL:",
                "NOP",
                "LABEL:",
                "HALT"
            };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Label 'LABEL' already defined*");
        }

        #endregion

        #region Mixed Instructions Test

        [Fact]
        public void MultipleJumpInstructions_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "JMP 0x1000",
                "JZ 2000",
                "JNZ $3000",
                "JR +10",
                "JRZ -5"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(13);
            
            // JMP 0x1000
            result[0].Should().Be(0x40);
            result[1].Should().Be(0x00);
            result[2].Should().Be(0x10);
            
            // JZ 2000
            result[3].Should().Be(0x41);
            result[4].Should().Be(208); // 2000 & 0xFF
            result[5].Should().Be(7);   // 2000 >> 8
            
            // JNZ $3000
            result[6].Should().Be(0x42);
            result[7].Should().Be(0x00);
            result[8].Should().Be(0x30);
            
            // JR +10
            result[9].Should().Be(0xC0);
            result[10].Should().Be(10);
            
            // JRZ -5
            result[11].Should().Be(0xC1);
            result[12].Should().Be(251); // -5 as unsigned byte
        }

        #endregion
    }
}