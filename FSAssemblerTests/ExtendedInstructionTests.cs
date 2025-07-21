using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Unit tests for extended instruction set - new opcodes not covered in existing test files
    /// This includes extended store instructions (STE/STF), extended stack operations (PUSH/POP D/E/F),
    /// and extended register transfer instructions (16-bit MOV and SWP operations)
    /// </summary>
    public class ExtendedInstructionTests
    {
        private readonly Assembler _assembler;

        public ExtendedInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Extended Store Instructions (STE/STF)

        [Fact]
        public void STE_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STE 0x2000"                  //  0    3   Store E at address 0x2000
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x56); // STE opcode
            result[1].Should().Be(0x00); // Low byte of address (little-endian)
            result[2].Should().Be(0x20); // High byte of address
        }

        [Fact]
        public void STF_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STF $3000"                   //  0    3   Store F at address 0x3000
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x57); // STF opcode
            result[1].Should().Be(0x00); // Low byte of address (little-endian)
            result[2].Should().Be(0x30); // High byte of address
        }

        [Fact]
        public void STE_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STE DATA_E",                 //  0    3   Store E at label address
                "DATA_E:",                    //  -    0   Label (position 3)
                "DB 0"                        //  3    1   Data byte
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0x56); // STE opcode
            result[1].Should().Be(0x03); // Low byte of label address (little-endian)
            result[2].Should().Be(0x00); // High byte of label address
            result[3].Should().Be(0x00); // DB data
        }

        #endregion

        #region Extended Stack Instructions (PUSH/POP D/E/F)

        [Fact]
        public void PUSH_D_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "PUSH D" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x7A); // PUSH D opcode
        }

        [Fact]
        public void POP_D_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "POP D" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x7B); // POP D opcode
        }

        [Fact]
        public void PUSH_E_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "PUSH E" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x7C); // PUSH E opcode
        }

        [Fact]
        public void POP_E_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "POP E" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x7D); // POP E opcode
        }

        [Fact]
        public void PUSH_F_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "PUSH F" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x7E); // PUSH F opcode
        }

        [Fact]
        public void POP_F_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "POP F" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x7F); // POP F opcode
        }

        [Theory]
        [InlineData("D", 0x7A, 0x7B)]
        [InlineData("E", 0x7C, 0x7D)]
        [InlineData("F", 0x7E, 0x7F)]
        public void ExtendedStackOperations_ShouldGenerateCorrectOpcodes(string register, byte pushOpcode, byte popOpcode)
        {
            // Arrange
            string[] pushLines = { $"PUSH {register}" };
            string[] popLines = { $"POP {register}" };

            // Act
            byte[] pushResult = _assembler.AssembleLines(pushLines);
            byte[] popResult = _assembler.AssembleLines(popLines);

            // Assert
            pushResult.Should().NotBeNull();
            pushResult.Should().HaveCount(1);
            pushResult[0].Should().Be(pushOpcode);

            popResult.Should().NotBeNull();
            popResult.Should().HaveCount(1);
            popResult[0].Should().Be(popOpcode);
        }

        #endregion

        #region Extended SWP Instructions (8-bit)

        [Fact]
        public void SWP_A_D_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP A,D" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA8); // SWP A,D opcode
        }

        [Fact]
        public void SWP_A_E_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP A,E" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xA9); // SWP A,E opcode
        }

        [Fact]
        public void SWP_A_F_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP A,F" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xAA); // SWP A,F opcode
        }

        [Theory]
        [InlineData("A,D", 0xA8)]
        [InlineData("A,E", 0xA9)]
        [InlineData("A,F", 0xAA)]
        public void ExtendedSWP_8Bit_ShouldGenerateCorrectOpcodes(string registers, byte expectedOpcode)
        {
            // Arrange
            string[] lines = { $"SWP {registers}" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(expectedOpcode);
        }

        #endregion

        #region Extended SWP Instructions (16-bit)

        [Fact]
        public void SWP_DA_DB_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP DA,DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xAB); // SWP DA,DB opcode
        }

        [Fact]
        public void SWP_DA_IDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP DA,IDX" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xAE); // SWP DA,IDX opcode
        }

        [Fact]
        public void SWP_DA_IDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "SWP DA,IDY" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xAF); // SWP DA,IDY opcode
        }

        [Theory]
        [InlineData("DA,DB", 0xAB)]
        [InlineData("DA,IDX", 0xAE)]
        [InlineData("DA,IDY", 0xAF)]
        public void ExtendedSWP_16Bit_ShouldGenerateCorrectOpcodes(string registers, byte expectedOpcode)
        {
            // Arrange
            string[] lines = { $"SWP {registers}" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(expectedOpcode);
        }

        #endregion

        #region Extended MOV Instructions (16-bit)

        [Fact]
        public void MOV_DA_DB_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV DA,DB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xAC); // MOV DA,DB opcode
        }

        [Fact]
        public void MOV_DB_DA_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV DB,DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xAD); // MOV DB,DA opcode
        }

        [Fact]
        public void MOV_DA_IDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV DA,IDX" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xB0); // MOV DA,IDX opcode
        }

        [Fact]
        public void MOV_DA_IDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV DA,IDY" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xB1); // MOV DA,IDY opcode
        }

        [Fact]
        public void MOV_IDX_DA_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV IDX,DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xB2); // MOV IDX,DA opcode
        }

        [Fact]
        public void MOV_IDY_DA_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "MOV IDY,DA" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xB3); // MOV IDY,DA opcode
        }

        [Theory]
        [InlineData("DA,DB", 0xAC)]
        [InlineData("DB,DA", 0xAD)]
        [InlineData("DA,IDX", 0xB0)]
        [InlineData("DA,IDY", 0xB1)]
        [InlineData("IDX,DA", 0xB2)]
        [InlineData("IDY,DA", 0xB3)]
        public void ExtendedMOV_16Bit_ShouldGenerateCorrectOpcodes(string registers, byte expectedOpcode)
        {
            // Arrange
            string[] lines = { $"MOV {registers}" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(expectedOpcode);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void STE_WithoutOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "STE" };

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*Instruction STE requires one operand*");
        }

        [Fact]
        public void PUSH_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH X" };

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*Invalid PUSH register: X*");
        }

        [Fact]
        public void SWP_WithInvalidRegisterCombination_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SWP A,X" };

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*Invalid SWP registers: A,X*");
        }

        [Fact]
        public void MOV_WithInvalidRegisterCombination_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "MOV DA,X" };

            // Act & Assert
            _assembler.Invoking(a => a.AssembleLines(lines))
                .Should().Throw<AssemblerException>()
                .WithMessage("Line 1")
                .WithInnerException<AssemblerException>()
                .WithMessage("*Invalid MOV registers: DA,X*");
        }

        #endregion

        #region Integration Tests with Extended Instructions

        [Fact]
        public void ComplexProgram_WithExtendedInstructions_ShouldAssembleCorrectly()
        {
            // Arrange - Program demonstrating all new extended instructions
            string[] lines = 
            {                                         // pos  size
                "EXTENDED_TEST:",                     //  -    0   Label
                "LDE #0x55",                          //  0    2   Load E with test value
                "LDF #0x66",                          //  2    2   Load F with test value  
                "STE STORAGE_E",                      //  4    3   Store E at memory address
                "STF STORAGE_F",                      //  7    3   Store F at memory address
                "",
                "; Test extended stack operations",
                "PUSH D",                             // 10    1   Push D onto stack
                "PUSH E",                             // 11    1   Push E onto stack
                "PUSH F",                             // 12    1   Push F onto stack
                "POP F",                              // 13    1   Pop from stack to F
                "POP E",                              // 14    1   Pop from stack to E
                "POP D",                              // 15    1   Pop from stack to D
                "",
                "; Test extended register operations",
                "LDDA #0x1234",                       // 16    3   Load DA with test value
                "LDDB #0x5678",                       // 19    3   Load DB with test value
                "SWP DA,DB",                          // 22    1   Swap DA and DB
                "MOV DA,IDX",                         // 23    1   Move DA to IDX
                "MOV IDY,DA",                         // 24    1   Move DA to IDY
                "",
                "; Test 8-bit extended swaps",
                "LDA #0x11",                          // 25    2   Load A with test value
                "LDD #0x22",                          // 27    2   Load D with test value
                "SWP A,D",                            // 29    1   Swap A and D
                "SWP A,E",                            // 30    1   Swap A and E
                "SWP A,F",                            // 31    1   Swap A and F
                "",
                "HALT",                               // 32    1   End program
                "",
                "; Data section",
                "STORAGE_E:",                         //  -    0   Label (position 33)
                "DB 0",                               // 33    1   Storage for E register test
                "STORAGE_F:",                         //  -    0   Label (position 34)
                "DB 0"                                // 34    1   Storage for F register test
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(35); // Total program size

            // Verify key opcodes
            result[0].Should().Be(0x14);  // LDE #0x55
            result[1].Should().Be(0x55);
            result[2].Should().Be(0x15);  // LDF #0x66
            result[3].Should().Be(0x66);
            result[4].Should().Be(0x56);  // STE STORAGE_E
            result[7].Should().Be(0x57);  // STF STORAGE_F
            result[10].Should().Be(0x7A); // PUSH D
            result[11].Should().Be(0x7C); // PUSH E
            result[12].Should().Be(0x7E); // PUSH F
            result[22].Should().Be(0xAB); // SWP DA,DB
            result[29].Should().Be(0xA8); // SWP A,D
            result[32].Should().Be(0x01); // HALT
        }

        [Fact]
        public void StackManipulation_WithExtendedRegisters_ShouldWorkCorrectly()
        {
            // Arrange - Program testing stack operations with D, E, F registers
            string[] lines = 
            {                                 // pos  size
                "LDD #0x77",                  //  0    2   Load D with test value
                "LDE #0x88",                  //  2    2   Load E with test value
                "LDF #0x99",                  //  4    2   Load F with test value
                "",
                "; Push all extended registers", 
                "PUSH D",                     //  6    1   Push D onto stack
                "PUSH E",                     //  7    1   Push E onto stack  
                "PUSH F",                     //  8    1   Push F onto stack
                "",
                "; Clear registers",
                "LDD #0x00",                  //  9    2   Clear D
                "LDE #0x00",                  // 11    2   Clear E
                "LDF #0x00",                  // 13    2   Clear F
                "",
                "; Pop in reverse order",
                "POP F",                      // 15    1   Pop to F (should be 0x99)
                "POP E",                      // 16    1   Pop to E (should be 0x88)
                "POP D",                      // 17    1   Pop to D (should be 0x77)
                "",
                "HALT"                        // 18    1   End program
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(19);

            // Verify stack operations opcodes
            result[6].Should().Be(0x7A);   // PUSH D
            result[7].Should().Be(0x7C);   // PUSH E
            result[8].Should().Be(0x7E);   // PUSH F
            result[15].Should().Be(0x7F);  // POP F
            result[16].Should().Be(0x7D);  // POP E
            result[17].Should().Be(0x7B);  // POP D
        }

        #endregion
    }
}