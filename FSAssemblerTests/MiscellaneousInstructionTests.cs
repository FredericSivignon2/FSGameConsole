using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions diverses : stockage, pile, transfert, sous-routines
    /// STA, STB, STC, STD, STDA, STDB, PUSH, POP, MOV, SWP, CALL, RET
    /// </summary>
    public class MiscellaneousInstructionTests
    {
        private readonly Assembler _assembler;

        public MiscellaneousInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Store Instructions

        [Fact]
        public void STA_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STA 0x1000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x50); // STA opcode
            result[1].Should().Be(0x00); // Low byte of 0x1000
            result[2].Should().Be(0x10); // High byte of 0x1000
        }

        [Fact]
        public void STB_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STB $ABCD" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x53); // STB opcode
            result[1].Should().Be(0xCD); // Low byte of $ABCD
            result[2].Should().Be(0xAB); // High byte of $ABCD
        }

        [Fact]
        public void STC_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STC 65535" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x54); // STC opcode
            result[1].Should().Be(0xFF); // Low byte of 65535
            result[2].Should().Be(0xFF); // High byte of 65535
        }

        [Fact]
        public void STD_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STD 0" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x55); // STD opcode
            result[1].Should().Be(0x00); // Low byte of 0
            result[2].Should().Be(0x00); // High byte of 0
        }

        [Fact]
        public void STDA_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STDA 0x8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x51); // STDA opcode
            result[1].Should().Be(0x00); // Low byte of 0x8000
            result[2].Should().Be(0x80); // High byte of 0x8000
        }

        [Fact]
        public void STDB_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "STDB 0x4000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x52); // STDB opcode
            result[1].Should().Be(0x00); // Low byte of 0x4000
            result[2].Should().Be(0x40); // High byte of 0x4000
        }

        #endregion

        #region Stack Instructions

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

        #region Transfer Instructions

        [Fact]
        public void MOV_AB_ShouldGenerateCorrectOpcode()
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
        public void MOV_AC_ShouldGenerateCorrectOpcode()
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
        public void MOV_BA_ShouldGenerateCorrectOpcode()
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
        public void MOV_BC_ShouldGenerateCorrectOpcode()
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
        public void MOV_CA_ShouldGenerateCorrectOpcode()
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
        public void MOV_CB_ShouldGenerateCorrectOpcode()
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

        [Fact]
        public void SWP_AB_ShouldGenerateCorrectOpcode()
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
        public void SWP_AC_ShouldGenerateCorrectOpcode()
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

        #region Subroutine Instructions

        [Fact]
        public void CALL_WithAddress_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "CALL 0x2000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x60); // CALL opcode
            result[1].Should().Be(0x00); // Low byte of 0x2000
            result[2].Should().Be(0x20); // High byte of 0x2000
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

        #region DB (Data Byte) Instruction

        [Fact]
        public void DB_WithSingleByte_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "DB 42" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(42);
        }

        [Fact]
        public void DB_WithMultipleBytes_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "DB 1,2,3,255,0" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(1);
            result[1].Should().Be(2);
            result[2].Should().Be(3);
            result[3].Should().Be(255);
            result[4].Should().Be(0);
        }

        [Fact]
        public void DB_WithHexValues_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "DB 0xFF,0x00,$AB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xFF);
            result[1].Should().Be(0x00);
            result[2].Should().Be(0xAB);
        }

        #endregion

        #region Complex Scenarios

        [Fact]
        public void CompleteProgram_WithAllInstructionTypes_ShouldAssemble()
        {
            // Arrange
            string[] lines = 
            {
                "; Simple program demonstrating various instructions",
                "MAIN:",
                "LDA #10",        // Load immediate
                "STA 0x8000",     // Store to memory
                "PUSH A",         // Push to stack
                "CALL SUBROUTINE",// Call subroutine
                "POP A",          // Pop from stack
                "HALT",           // End program
                "",
                "SUBROUTINE:",
                "LDB #20",        // Load another value
                "MOV A,B",        // Transfer register
                "RET"             // Return from subroutine
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(12);
            
            // LDA #10
            result[0].Should().Be(0x10); result[1].Should().Be(10);
            
            // STA 0x8000
            result[2].Should().Be(0x50); result[3].Should().Be(0x00); result[4].Should().Be(0x80);
            
            // PUSH A
            result[5].Should().Be(0x70);
            
            // CALL SUBROUTINE (address 10)
            result[6].Should().Be(0x60); result[7].Should().Be(0x0A); result[8].Should().Be(0x00);
            
            // POP A
            result[9].Should().Be(0x71);
            
            // HALT
            result[10].Should().Be(0x01);
            
            // At SUBROUTINE: LDB #20, MOV A,B, RET would start at address 11
            result[11].Should().Be(0x11); // LDB opcode
        }

        [Fact]
        public void StackOperations_ShouldGenerateCorrectSequence()
        {
            // Arrange
            string[] lines = 
            {
                "LDA #1",
                "LDB #2",
                "LDC #3",
                "PUSH A",     // Push all registers
                "PUSH B",
                "PUSH C",
                "POP C",      // Pop in reverse order
                "POP B",
                "POP A"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(12);
            
            result[0].Should().Be(0x10); result[1].Should().Be(1);   // LDA #1
            result[2].Should().Be(0x11); result[3].Should().Be(2);   // LDB #2
            result[4].Should().Be(0x12); result[5].Should().Be(3);   // LDC #3
            result[6].Should().Be(0x70);                             // PUSH A
            result[7].Should().Be(0x74);                             // PUSH B
            result[8].Should().Be(0x78);                             // PUSH C
            result[9].Should().Be(0x79);                             // POP C
            result[10].Should().Be(0x75);                            // POP B
            result[11].Should().Be(0x71);                            // POP A
        }

        #endregion

        #region Error Cases

        [Fact]
        public void STA_WithoutOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "STA" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*requires an address*");
        }

        [Fact]
        public void PUSH_WithoutOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*requires a register*");
        }

        [Fact]
        public void PUSH_WithInvalidRegister_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "PUSH X" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*Invalid PUSH register*");
        }

        [Fact]
        public void MOV_WithoutOperands_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "MOV" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*requires two registers*");
        }

        [Fact]
        public void MOV_WithInvalidRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "MOV A,D" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*Invalid MOV registers*");
        }

        [Fact]
        public void SWP_WithInvalidRegisters_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "SWP B,C" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*Invalid SWP registers*");
        }

        [Fact]
        public void CALL_WithoutOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "CALL" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*requires an address*");
        }

        [Fact]
        public void RET_WithOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "RET 42" };

            // Act & Assert
            Action act = () => _assembler.AssembleLines(lines);
            act.Should().Throw<AssemblerException>()
               .WithMessage("*takes no parameters*");
        }

        #endregion

        #region Case Insensitive Tests

        [Fact]
        public void MiscellaneousInstructions_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "sta 0x1000",
                "PUSH a",
                "pop A",
                "mov a,b",
                "SWP A,B",
                "call 0x2000",
                "RET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(10);
            result[0].Should().Be(0x50); // STA
            result[3].Should().Be(0x70); // PUSH A
            result[4].Should().Be(0x71); // POP A
            result[5].Should().Be(0xA0); // MOV A,B
            result[6].Should().Be(0xA6); // SWP A,B
            result[7].Should().Be(0x60); // CALL
            result[10].Should().Be(0x61); // RET (index 10, not 9)
        }

        #endregion
    }
}