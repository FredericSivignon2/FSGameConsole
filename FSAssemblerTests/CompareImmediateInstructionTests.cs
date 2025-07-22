using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Unit tests for compare immediate instruction set - new CMP opcodes (0xD0-0xD7)
    /// This includes compare with immediate values for all 8-bit registers (A,B,C,D,E,F) 
    /// and 16-bit registers (DA,DB)
    /// </summary>
    public class CompareImmediateInstructionTests
    {
        private readonly Assembler _assembler;

        public CompareImmediateInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region 8-bit Compare Immediate Instructions (0xD0-0xD5)

        [Fact]
        public void CMP_A_Immediate_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP A,#0x42" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD0); // CMP A,#imm opcode
            result[1].Should().Be(0x42); // Immediate value
        }

        [Fact]
        public void CMP_B_Immediate_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP B,#100" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD1); // CMP B,#imm opcode
            result[1].Should().Be(100);  // Immediate value
        }

        [Fact]
        public void CMP_C_Immediate_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP C,#$FF" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD2); // CMP C,#imm opcode
            result[1].Should().Be(0xFF); // Immediate value
        }

        [Fact]
        public void CMP_D_Immediate_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP D,#0" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD3); // CMP D,#imm opcode
            result[1].Should().Be(0x00); // Immediate value
        }

        [Fact]
        public void CMP_E_Immediate_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP E,#255" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD4); // CMP E,#imm opcode
            result[1].Should().Be(255);  // Immediate value
        }

        [Fact]
        public void CMP_F_Immediate_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP F,#0x80" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD5); // CMP F,#imm opcode
            result[1].Should().Be(0x80); // Immediate value
        }

        [Theory]
        [InlineData("A", 0xD0, 0x42)]
        [InlineData("B", 0xD1, 0x10)]
        [InlineData("C", 0xD2, 0xFF)]
        [InlineData("D", 0xD3, 0x00)]
        [InlineData("E", 0xD4, 0x55)]
        [InlineData("F", 0xD5, 0xAA)]
        public void CMP_8BitRegister_Immediate_ShouldGenerateCorrectOpcodes(string register, byte expectedOpcode, byte immediateValue)
        {
            // Arrange
            string[] lines = { $"CMP {register},#{immediateValue}" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(expectedOpcode);
            result[1].Should().Be(immediateValue);
        }

        #endregion

        #region 16-bit Compare Immediate Instructions (0xD6-0xD7)

        [Fact]
        public void CMP_DA_Immediate16_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP DA,#0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xD6); // CMP DA,#imm16 opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void CMP_DB_Immediate16_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP DB,#$ABCD" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xD7); // CMP DB,#imm16 opcode
            result[1].Should().Be(0xCD); // Low byte (little-endian)
            result[2].Should().Be(0xAB); // High byte
        }

        [Fact]
        public void CMP_DA_Immediate16_WithDecimal_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CMP DA,#65535" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xD6); // CMP DA,#imm16 opcode
            result[1].Should().Be(0xFF); // Low byte (65535 = 0xFFFF)
            result[2].Should().Be(0xFF); // High byte
        }

        [Theory]
        [InlineData("DA", 0xD6, 0x1000)]
        [InlineData("DB", 0xD7, 0x2000)]
        [InlineData("DA", 0xD6, 0x0000)]
        [InlineData("DB", 0xD7, 0xFFFF)]
        public void CMP_16BitRegister_Immediate_ShouldGenerateCorrectOpcodes(string register, byte expectedOpcode, ushort immediateValue)
        {
            // Arrange
            string[] lines = { $"CMP {register},#0x{immediateValue:X4}" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(expectedOpcode);
            result[1].Should().Be((byte)(immediateValue & 0xFF));         // Low byte
            result[2].Should().Be((byte)((immediateValue >> 8) & 0xFF));  // High byte
        }

        #endregion

        #region Format and Spacing Tests

        [Fact]
        public void CMP_WithSpaces_ShouldParseCorrectly()
        {
            // Arrange
            string[] lines = { "CMP   A  ,  #0x42  " };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD0); // CMP A,#imm opcode
            result[1].Should().Be(0x42); // Immediate value
        }

        [Fact]
        public void CMP_LowerCase_ShouldParseCorrectly()
        {
            // Arrange
            string[] lines = { "cmp b,#100" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD1); // CMP B,#imm opcode
            result[1].Should().Be(100);  // Immediate value
        }

        [Fact]
        public void CMP_CharacterLiteral_ShouldParseCorrectly()
        {
            // Arrange
            string[] lines = { "CMP A,#'Z'" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0xD0); // CMP A,#imm opcode
            result[1].Should().Be((byte)'Z'); // ASCII value of 'Z'
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void CMP_WithoutImmediate_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP A,42" }; // Missing '#'

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*Invalid CMP registers: A,42 (only A,B and A,C supported)*");
        }

        [Fact]
        public void CMP_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP X,#42" };

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*Invalid CMP register: X*");
        }

        [Fact]
        public void CMP_WithoutComma_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP A #42" }; // Missing comma

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*CMP instruction requires either two registers (CMP A,B) or register with immediate (CMP A,#imm)*");
        }

        [Fact]
        public void CMP_8BitRegister_WithOverflowValue_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP A,#256" }; // Value too large for 8-bit

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithInnerException<OverflowException>()
                .WithMessage("Value was either too large or too small for an unsigned byte.");
        }

        [Fact]
        public void CMP_16BitRegister_WithOverflowValue_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CMP DA,#65536" }; // Value too large for 16-bit

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithInnerException<OverflowException>()
                .WithMessage("Value was either too large or too small for a UInt16.");
        }

        #endregion

        #region Backward Compatibility Tests

        [Fact]
        public void CMP_RegisterToRegister_ShouldStillWork()
        {
            // Arrange - Test that original CMP A,B still works
            string[] lines = { "CMP A,B" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2C); // Original CMP A,B opcode
        }

        [Fact]
        public void CMP_RegisterToRegister_AC_ShouldStillWork()
        {
            // Arrange - Test that original CMP A,C still works
            string[] lines = { "CMP A,C" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x2F); // Original CMP A,C opcode
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void ComplexProgram_WithCompareImmediate_ShouldAssembleCorrectly()
        {
            // Arrange - Program demonstrating all new CMP immediate instructions
            string[] lines = 
            {                                 // pos  size
                "COMPARE_TEST:",              //  -    0   Label
                "LDA #42",                    //  0    2   Load A with test value
                "CMP A,#42",                  //  2    2   Compare A with same value (should set Zero flag)
                "JZ EQUAL_A",                 //  4    3   Jump if equal
                "",
                "LDB #100",                   //  7    2   Load B with test value
                "CMP B,#50",                  //  9    2   Compare B with smaller value (B > 50)
                "JNC GREATER_B",              // 11    3   Jump if no carry (B >= 50)
                "",
                "LDC #10",                    // 14    2   Load C with test value
                "CMP C,#20",                  // 16    2   Compare C with larger value (C < 20)
                "JC LESS_C",                  // 18    3   Jump if carry (C < 20)
                "",
                "LDD #255",                   // 21    2   Load D with max value
                "CMP D,#255",                 // 23    2   Compare D with same value
                "JZ EQUAL_D",                 // 25    3   Jump if equal
                "",
                "LDE #0",                     // 28    2   Load E with zero
                "CMP E,#1",                   // 30    2   Compare E with 1 (E < 1)
                "JC LESS_E",                  // 32    3   Jump if carry
                "",
                "LDF #128",                   // 35    2   Load F with test value
                "CMP F,#128",                 // 37    2   Compare F with same value
                "JZ EQUAL_F",                 // 39    3   Jump if equal
                "",
                "LDDA #0x1234",               // 42    3   Load DA with 16-bit test value
                "CMP DA,#0x1234",             // 45    3   Compare DA with same value
                "JZ EQUAL_DA",                // 48    3   Jump if equal
                "",
                "LDDB #0x8000",               // 51    3   Load DB with 16-bit test value
                "CMP DB,#0x4000",             // 54    3   Compare DB with smaller value
                "JNC GREATER_DB",             // 57    3   Jump if no carry
                "",
                "HALT",                       // 60    1   End program
                "",
                "; Labels for jumps",
                "EQUAL_A:",                   //  -    0   Label (position 61)
                "NOP",                        // 61    1   Do nothing
                "EQUAL_B:",                   //  -    0   Label (position 62)
                "GREATER_B:",                 //  -    0   Label (position 62)
                "LESS_C:",                    //  -    0   Label (position 62) 
                "EQUAL_D:",                   //  -    0   Label (position 62)
                "LESS_E:",                    //  -    0   Label (position 62)
                "EQUAL_F:",                   //  -    0   Label (position 62)
                "EQUAL_DA:",                  //  -    0   Label (position 62)
                "GREATER_DB:",                //  -    0   Label (position 62)
                "NOP"                         // 62    1   Do nothing
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(63); // Total program size

            // Verify key opcodes for new CMP immediate instructions
            result[2].Should().Be(0xD0);   // CMP A,#42
            result[3].Should().Be(42);
            result[9].Should().Be(0xD1);   // CMP B,#50
            result[10].Should().Be(50);
            result[16].Should().Be(0xD2);  // CMP C,#20
            result[17].Should().Be(20);
            result[23].Should().Be(0xD3);  // CMP D,#255
            result[24].Should().Be(255);
            result[30].Should().Be(0xD4);  // CMP E,#1
            result[31].Should().Be(1);
            result[37].Should().Be(0xD5);  // CMP F,#128
            result[38].Should().Be(128);
            result[45].Should().Be(0xD6);  // CMP DA,#0x1234
            result[46].Should().Be(0x34);  // Low byte
            result[47].Should().Be(0x12);  // High byte
            result[54].Should().Be(0xD7);  // CMP DB,#0x4000
            result[55].Should().Be(0x00);  // Low byte
            result[56].Should().Be(0x40);  // High byte
        }

        [Fact]
        public void MixedCompareInstructions_ShouldAssembleCorrectly()
        {
            // Arrange - Program mixing old register-to-register CMP with new immediate CMP
            string[] lines = 
            {                                 // pos  size
                "LDA #10",                    //  0    2   Load A with 10
                "LDB #20",                    //  2    2   Load B with 20
                "CMP A,B",                    //  4    1   Compare A with B (old format)
                "CMP A,#10",                  //  5    2   Compare A with immediate (new format)
                "CMP B,#20",                  //  7    2   Compare B with immediate (new format)
                "HALT"                        //  9    1   End program
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(10);

            // Verify mixed opcodes
            result[4].Should().Be(0x2C);  // CMP A,B (original)
            result[5].Should().Be(0xD0);  // CMP A,#10 (new)
            result[6].Should().Be(10);
            result[7].Should().Be(0xD1);  // CMP B,#20 (new)
            result[8].Should().Be(20);
        }

        #endregion
    }
}