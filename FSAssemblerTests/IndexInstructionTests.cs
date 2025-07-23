using FluentAssertions;
using FSAssembler;
using System.Reflection;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions d'index (Index Register Instructions)
    /// LDIDX, LDIDY et toutes les nouvelles instructions d'adressage indexé
    /// Note: Updated to match CPU8Bit.cs implementation with only IDX et IDY registers (0x1A, 0x1B)
    /// </summary>
    public class IndexInstructionTests
    {
        private readonly Assembler _assembler;

        public IndexInstructionTests()
        {
            _assembler = new Assembler();
        }

        #region Index Register Load Instructions (16-bit Immediate)

        [Fact]
        public void LDIDX_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIDX #0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1A); // LDIDX immediate opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void LDIDY_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIDY #0xABCD" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1B); // LDIDY immediate opcode
            result[1].Should().Be(0xCD); // Low byte (little-endian)
            result[2].Should().Be(0xAB); // High byte
        }

        [Fact]
        public void LDIDX_DecimalImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIDX #65535" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1A); // LDIDX immediate opcode
            result[1].Should().Be(0xFF); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        [Fact]
        public void LDIDY_DollarHexImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIDY #$8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1B); // LDIDY immediate opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x80); // High byte
        }

        #endregion

        #region Indexed Addressing Instructions (0x86-0x8F) - Simplified for IDX/IDY only

        [Fact]
        public void LDA_IndexedIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDX)"                   //  0    1   Load A from address pointed by IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x86); // LDA (IDX) opcode
        }

        [Fact]
        public void LDB_IndexedIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDX)"                   //  0    1   Load B from address pointed by IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x87); // LDB (IDX) opcode
        }

        [Fact]
        public void LDA_IndexedIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDY)"                   //  0    1   Load A from address pointed by IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x88); // LDA (IDY) opcode
        }

        [Fact]
        public void LDB_IndexedIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDY)"                   //  0    1   Load B from address pointed by IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x89); // LDB (IDY) opcode
        }

        [Fact]
        public void STA_IndexedIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDX)"                   //  0    1   Store A at address pointed by IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8A); // STA (IDX) opcode
        }

        [Fact]
        public void STB_IndexedIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDX)"                   //  0    1   Store B at address pointed by IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8B); // STB (IDX) opcode
        }

        [Fact]
        public void STA_IndexedIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDY)"                   //  0    1   Store A at address pointed by IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8C); // STA (IDY) opcode
        }

        [Fact]
        public void STB_IndexedIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDY)"                   //  0    1   Store B at address pointed by IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8D); // STB (IDY) opcode
        }

        #endregion

        #region Indexed Addressing with Displacement (0x90-0x99) - Simplified for IDX/IDY only

        [Fact]
        public void LDA_IndexedIDXWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDX+5)"                 //  0    2   Load A from IDX + 5
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x90); // LDA (IDX+offset) opcode
            result[1].Should().Be(5);    // 8-bit signed offset
        }

        [Fact]
        public void LDB_IndexedIDXWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDX-10)"                //  0    2   Load B from IDX - 10
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x91); // LDB (IDX+offset) opcode
            result[1].Should().Be(246);  // -10 as signed byte (256 - 10)
        }

        [Fact]
        public void LDA_IndexedIDYWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDY+127)"               //  0    2   Load A from IDY + 127 (max positive offset)
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x92); // LDA (IDY+offset) opcode
            result[1].Should().Be(127);  // Maximum positive offset
        }

        [Fact]
        public void LDB_IndexedIDYWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDY-128)"               //  0    2   Load B from IDY - 128 (max negative offset)
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x93); // LDB (IDY+offset) opcode
            result[1].Should().Be(128);  // -128 as signed byte
        }

        [Fact]
        public void STA_IndexedIDXWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDX+8)"                 //  0    2   Store A at IDX + 8
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x94); // STA (IDX+offset) opcode
            result[1].Should().Be(8);    // Offset
        }

        [Fact]
        public void STB_IndexedIDXWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDX-5)"                 //  0    2   Store B at IDX - 5
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x95); // STB (IDX+offset) opcode
            result[1].Should().Be(251);  // -5 as signed byte (256 - 5)
        }

        [Fact]
        public void STA_IndexedIDYWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDY+16)"                //  0    2   Store A at IDY + 16
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x96); // STA (IDY+offset) opcode
            result[1].Should().Be(16);   // Offset
        }

        [Fact]
        public void STB_IndexedIDYWithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDY+0)"                 //  0    2   Store B at IDY + 0
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x97); // STB (IDY+offset) opcode
            result[1].Should().Be(0);    // Zero offset
        }

        #endregion

        #region Auto-Increment/Decrement Instructions (0xC4-0xCB)

        [Fact]
        public void LDAIDX_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIDX+"                     //  0    1   Load A from (IDX), then increment IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC4); // LDAIDX+ opcode
        }

        [Fact]
        public void LDAIDY_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIDY+"                     //  0    1   Load A from (IDY), then increment IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC5); // LDAIDY+ opcode
        }

        [Fact]
        public void STAIDX_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIDX+"                     //  0    1   Store A at (IDX), then increment IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC6); // STAIDX+ opcode
        }

        [Fact]
        public void STAIDY_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIDY+"                     //  0    1   Store A at (IDY), then increment IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC7); // STAIDY+ opcode
        }

        [Fact]
        public void LDAIDX_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIDX-"                     //  0    1   Load A from (IDX), then decrement IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC8); // LDAIDX- opcode
        }

        [Fact]
        public void LDAIDY_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIDY-"                     //  0    1   Load A from (IDY), then decrement IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC9); // LDAIDY- opcode
        }

        [Fact]
        public void STAIDX_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIDX-"                     //  0    1   Store A at (IDX), then decrement IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xCA); // STAIDX- opcode
        }

        [Fact]
        public void STAIDY_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIDY-"                     //  0    1   Store A at (IDY), then decrement IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xCB); // STAIDY- opcode
        }

        #endregion

        #region Index Register Arithmetic Instructions (0xE0-0xE3, 0xE8, 0xEA)

        [Fact]
        public void INCIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "INCIDX"                      //  0    1   Increment IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE0); // INCIDX opcode
        }

        [Fact]
        public void DECIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "DECIDX"                      //  0    1   Decrement IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE1); // DECIDX opcode
        }

        [Fact]
        public void INCIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "INCIDY"                      //  0    1   Increment IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE2); // INCIDY opcode
        }

        [Fact]
        public void DECIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "DECIDY"                      //  0    1   Decrement IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE3); // DECIDY opcode
        }

        [Fact]
        public void ADDIDX_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "ADDIDX #0x1000"              //  0    3   ADDIDX - Add 16-bit immediate to IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xE8); // ADDIDX #imm16 opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x10); // High byte
        }

        [Fact]
        public void ADDIDY_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "ADDIDY #500"                 //  0    3   Add 16-bit immediate to IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xEA); // ADDIDY #imm16 opcode
            result[1].Should().Be(244);  // 500 & 0xFF = 244
            result[2].Should().Be(1);    // 500 >> 8 = 1
        }

        #endregion

        #region Index Register Transfer Instructions (0xF5, 0xF6, 0xF9)

        [Fact]
        public void MVIDXIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIDXIDY"                    //  0    1   Move IDX to IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF5); // MVIDXIDY opcode
        }

        [Fact]
        public void MVIDYIDX_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIDYIDX"                    //  0    1   Move IDY to IDX
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF6); // MVIDYIDX opcode
        }

        [Fact]
        public void SWPIDXIDY_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "SWPIDXIDY"                   //  0    1   Swap IDX and IDY
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF9); // SWPIDXIDY opcode
        }

        #endregion

        #region Integration Tests with Index Instructions

        [Fact]
        public void ComplexIndexProgram_ShouldAssembleCorrectly()
        {
            // Arrange - Programme utilisant plusieurs instructions d'index
            string[] lines = 
            {                                         // pos  size
                "ARRAY_COPY:",                        //  -    0   Label
                "LDIDX #SOURCE_ARRAY",                //  0    3   Load source pointer
                "LDIDY #DEST_ARRAY",                  //  3    3   Load destination pointer  
                "LDA #10",                            //  6    2   Load counter
                "",
                "COPY_LOOP:",                         //  -    0   Label (position 8)
                "LDAIDX+",                            //  8    1   Load from source, increment pointer
                "STAIDY+",                            //  9    1   Store to dest, increment pointer
                "DEC A",                              // 10    1   Decrement counter
                "JNZ COPY_LOOP",                      // 11    3   Continue if not zero
                "HALT",                               // 14    1   End program
                "",
                "SOURCE_ARRAY:",                      //  -    0   Label (position 15)
                "DB 1, 2, 3, 4, 5, 6, 7, 8, 9, 10",  // 15   10   Source data
                "DEST_ARRAY:",                        //  -    0   Label (position 25)
                "DB 0, 0, 0, 0, 0, 0, 0, 0, 0, 0"    // 25   10   Destination space
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(35); // Total: 3+3+2+1+1+1+3+1+10+10 = 35 bytes

            // Verify key instructions
            result[0].Should().Be(0x1A);  // LDIDX #imm16
            result[1].Should().Be(0x0F);  // SOURCE_ARRAY address low (15)
            result[2].Should().Be(0x00);  // SOURCE_ARRAY address high

            result[3].Should().Be(0x1B);  // LDIDY #imm16
            result[4].Should().Be(0x19);  // DEST_ARRAY address low (25)
            result[5].Should().Be(0x00);  // DEST_ARRAY address high

            result[6].Should().Be(0x10);  // LDA #imm
            result[7].Should().Be(10);    // Counter value

            result[8].Should().Be(0xC4);  // LDAIDX+
            result[9].Should().Be(0xC7);  // STAIDY+
            result[10].Should().Be(0x29); // DEC A
            result[11].Should().Be(0x42); // JNZ
            result[12].Should().Be(0x08); // COPY_LOOP address low (8)
            result[13].Should().Be(0x00); // COPY_LOOP address high

            result[14].Should().Be(0x01); // HALT

            // Verify source data
            for (int i = 0; i < 10; i++)
            {
                result[15 + i].Should().Be((byte)(i + 1));
            }

            // Verify destination space (initially zeros)
            for (int i = 0; i < 10; i++)
            {
                result[25 + i].Should().Be(0);
            }
        }

        [Fact]
        public void StructureAccess_WithIndexAndOffset_ShouldAssembleCorrectly()
        {
            // Arrange - Programme accédant aux membres d'une structure avec offset
            string[] lines = 
            {                                         // pos  size
                "PLAYER_SETUP:",                      //  -    0   Label
                "LDIDX #PLAYER_STRUCT",               //  0    3   Point to player structure
                "LDA #100",                           //  3    2   Player X position
                "STA (IDX+0)",                        //  5    2   Store X (offset 0)
                "LDA #50",                            //  7    2   Player Y position  
                "STA (IDX+1)",                        //  9    2   Store Y (offset 1)
                "LDA #255",                           // 11    2   Player health
                "STA (IDX+2)",                        // 13    2   Store health (offset 2)
                "HALT",                               // 15    1   End program
                "",
                "PLAYER_STRUCT:",                     //  -    0   Label (position 16)
                "DB 0, 0, 0"                          // 16    3   X, Y, Health
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(19); // Total: 3+2+2+2+2+2+2+1+3 = 19 bytes

            // Verify structure setup
            result[0].Should().Be(0x1A);  // LDIDX #imm16
            result[1].Should().Be(0x10);  // PLAYER_STRUCT address low (16)
            result[2].Should().Be(0x00);  // PLAYER_STRUCT address high

            // Verify X position setup
            result[3].Should().Be(0x10);  // LDA #imm
            result[4].Should().Be(100);   // X value
            result[5].Should().Be(0x94);  // STA (IDX+offset)
            result[6].Should().Be(0);     // Offset 0

            // Verify Y position setup
            result[7].Should().Be(0x10);  // LDA #imm
            result[8].Should().Be(50);    // Y value
            result[9].Should().Be(0x94);  // STA (IDX+offset)
            result[10].Should().Be(1);    // Offset 1

            // Verify health setup
            result[11].Should().Be(0x10); // LDA #imm
            result[12].Should().Be(255);  // Health value
            result[13].Should().Be(0x94); // STA (IDX+offset)
            result[14].Should().Be(2);    // Offset 2

            result[15].Should().Be(0x01); // HALT
        }

        #endregion

        #region Error Cases

        [Fact]
        public void IndexInstruction_WithInvalidOffset_ShouldThrowException()
        {
            // Arrange - Offset trop grand pour un signed byte
            string[] lines = { "LDA (IDX+200)" }; // > 127

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid offset in indexed expression*");
        }

        [Fact]
        public void IndexInstruction_WithInvalidNegativeOffset_ShouldThrowException()
        {
            // Arrange - Offset négatif trop grand
            string[] lines = { "LDA (IDX-200)" }; // < -128

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*offset out of range*");
        }

        [Fact]
        public void IndexLoadInstruction_WithInvalidValue_ShouldThrowException()
        {
            // Arrange - Valeur trop grande pour 16-bit
            string[] lines = { "LDIDX #65536" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<TargetInvocationException>()
                  .WithInnerException<OverflowException>();
        }

        [Fact]
        public void IndexInstruction_WithMissingOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "LDIDX" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires one operand*");
        }

        [Fact]
        public void IndexArithmeticInstruction_WithMissingOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "ADDIDX" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<TargetInvocationException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires one operand*");
        }

        [Fact]
        public void IndexIncrementInstruction_WithExtraOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "INCIDX #5" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Instruction INCIDX takes no parameters*");
        }

        #endregion

        #region Label Support Tests

        [Fact]
        public void LDIDX_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "LDIDX MESSAGE",  //  0    3   Load IDX with MESSAGE address
                "HALT",           //  3    1   Halt
                "MESSAGE:",       //  -    0   Label (position 4)
                "DB 42"           //  4    1   Data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x1A); // LDIDX opcode
            result[1].Should().Be(0x04); // MESSAGE address low byte (position 4)
            result[2].Should().Be(0x00); // MESSAGE address high byte
            result[3].Should().Be(0x01); // HALT opcode
            result[4].Should().Be(42);   // Data at MESSAGE
        }

        [Fact]
        public void LDIDY_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "LDIDY DATA",     //  0    3   Load IDY with DATA address
                "NOP",            //  3    1   
                "DATA:",          //  -    0   Label (position 4)
                "DB 0xFF"         //  4    1   Data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x1B); // LDIDY opcode
            result[1].Should().Be(0x04); // DATA address low byte (position 4)
            result[2].Should().Be(0x00); // DATA address high byte
            result[3].Should().Be(0x00); // NOP opcode
            result[4].Should().Be(0xFF); // Data at DATA
        }

        [Fact]
        public void LDIDX_WelcomeMessage_ROM_Style_ShouldWork()
        {
            // Arrange - Test the exact ROM syntax that était failing
            string[] lines = 
            {                            // pos  size
                "LDIDX WelcomeMessage",  //  0    3   This is the line that was failing!
                "HALT",                  //  3    1   
                "WelcomeMessage:",       //  -    0   Label (position 4)
                "DB \"FS System v1.0\""
            };

            // Act & Assert - Should NOT throw an exception
            Action assembleAction = () => _assembler.AssembleLines(lines);
            assembleAction.Should().NotThrow("LDIDX with label should now work like LDDA");
            
            byte[] result = _assembler.AssembleLines(lines);
            
            // Assert
            result.Should().NotBeNull();
            result[0].Should().Be(0x1A, "LDIDX opcode");
            result[1].Should().Be(4, "WelcomeMessage address low byte (position 4)");
            result[2].Should().Be(0, "WelcomeMessage address high byte"); 
            result[3].Should().Be(0x01, "HALT opcode");
            
            // Verify string content
            string message = System.Text.Encoding.ASCII.GetString(result, 4, 14);
            message.Should().Be("FS System v1.0");
        }

        [Fact]
        public void ROM_Original_LDIDX_Label_ShouldAssemble()
        {
            // Arrange - Simplified version of the original ROM with LDIDX label syntax
            string[] lines = 
            {
                "Startup:",
                "    LDIDX WelcomeMessage",    // This should now work!
                "    CALL PrintString",
                "    HALT",
                "",
                "PrintString:",
                "PrintString_Loop:",
                "    LDA (IDX)", 
                "    LDB #0",
                "    CMP A,B",
                "    JZ PrintString_End",
                "    INCIDX",
                "    JMP PrintString_Loop",
                "PrintString_End:",
                "    RET",
                "",
                "WelcomeMessage:",
                "    DB \"FS System v1.0\""
            };

            // Act - This should NOT throw an exception anymore
            Action assembleAction = () => _assembler.AssembleLines(lines);

            // Assert
            assembleAction.Should().NotThrow("LDIDX should now support label syntax like LDDA");
            
            byte[] result = _assembler.AssembleLines(lines);
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            
            // Verify that LDIDX got the correct address for WelcomeMessage
            result[0].Should().Be(0x1A, "First instruction should be LDIDX opcode");
            // The exact address will depend on the program layout, but it should be non-zero
            // since WelcomeMessage is defined later in the program
            ushort welcomeMessageAddress = (ushort)(result[1] | (result[2] << 8));
            welcomeMessageAddress.Should().BeGreaterThan(0, "WelcomeMessage should have a valid address");
        }

        [Fact]
        public void IndexLoad_BothSyntaxesSupported_ShouldWork()
        {
            // Arrange - Test both immediate and label syntax work
            string[] lines = 
            {                            // pos  size
                "LDIDX #0x8000",         //  0    3   Immediate syntax
                "LDIDY DATA_PTR",        //  3    3   Label syntax
                "HALT",                  //  6    1   Halt
                "DATA_PTR:",             //  -    0   Label (position 7)
                "DB 0xAB",               //  7    1   Data
                "STRING_PTR:",           //  -    0   Label (position 8)
                "DB \"OK\""              //  8    3   String + null
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(11);
            
            // Verify LDIDX #0x8000
            result[0].Should().Be(0x1A); // LDIDX opcode
            result[1].Should().Be(0x00); // 0x8000 low byte
            result[2].Should().Be(0x80); // 0x8000 high byte
            
            // Verify LDIDY DATA_PTR
            result[3].Should().Be(0x1B); // LDIDY opcode
            result[4].Should().Be(7);    // DATA_PTR address low byte (position 7)
            result[5].Should().Be(0);    // DATA_PTR address high byte
            
            // Verify HALT
            result[6].Should().Be(0x01); // HALT opcode
            
            // Verify data
            result[7].Should().Be(0xAB); // Data at DATA_PTR
            result[8].Should().Be(79);   // 'O'
            result[9].Should().Be(75);   // 'K'
            result[10].Should().Be(0);   // null terminator
        }

        #endregion
    }
}