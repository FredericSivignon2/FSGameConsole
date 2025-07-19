using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour les instructions d'index (Index Register Instructions)
    /// LDIX1, LDIX2, LDIY1, LDIY2 et toutes les nouvelles instructions d'adressage indexé
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
        public void LDIX1_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIX1 #0x1234" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1A); // LDIX1 immediate opcode
            result[1].Should().Be(0x34); // Low byte (little-endian)
            result[2].Should().Be(0x12); // High byte
        }

        [Fact]
        public void LDIX2_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIX2 #0xABCD" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1B); // LDIX2 immediate opcode
            result[1].Should().Be(0xCD); // Low byte (little-endian)
            result[2].Should().Be(0xAB); // High byte
        }

        [Fact]
        public void LDIY1_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIY1 #65535" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1C); // LDIY1 immediate opcode
            result[1].Should().Be(0xFF); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        [Fact]
        public void LDIY2_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = { "LDIY2 #$8000" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x1D); // LDIY2 immediate opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x80); // High byte
        }

        #endregion

        #region Indexed Addressing Instructions (0x86-0x8F)

        [Fact]
        public void LDA_IndexedIDX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDX1)"                  //  0    1   Load A from address pointed by IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x86); // LDA (IDX1) opcode
        }

        [Fact]
        public void LDB_IndexedIDX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDX1)"                  //  0    1   Load B from address pointed by IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x87); // LDB (IDX1) opcode
        }

        [Fact]
        public void LDA_IndexedIDY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDY1)"                  //  0    1   Load A from address pointed by IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x88); // LDA (IDY1) opcode
        }

        [Fact]
        public void LDB_IndexedIDY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDY1)"                  //  0    1   Load B from address pointed by IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x89); // LDB (IDY1) opcode
        }

        [Fact]
        public void STA_IndexedIDX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDX1)"                  //  0    1   Store A at address pointed by IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8A); // STA (IDX1) opcode
        }

        [Fact]
        public void STB_IndexedIDX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDX1)"                  //  0    1   Store B at address pointed by IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8B); // STB (IDX1) opcode
        }

        [Fact]
        public void STA_IndexedIDY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDY1)"                  //  0    1   Store A at address pointed by IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8C); // STA (IDY1) opcode
        }

        [Fact]
        public void STB_IndexedIDY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDY1)"                  //  0    1   Store B at address pointed by IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8D); // STB (IDY1) opcode
        }

        [Fact]
        public void LDA_IndexedIDX2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDX2)"                  //  0    1   Load A from address pointed by IDX2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8E); // LDA (IDX2) opcode
        }

        [Fact]
        public void LDA_IndexedIDY2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDY2)"                  //  0    1   Load A from address pointed by IDY2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0x8F); // LDA (IDY2) opcode
        }

        #endregion

        #region Indexed Addressing with Displacement (0x90-0x99)

        [Fact]
        public void LDA_IndexedIDX1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDX1+5)"                //  0    2   Load A from IDX1 + 5
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x90); // LDA (IDX1+offset) opcode
            result[1].Should().Be(5);    // 8-bit signed offset
        }

        [Fact]
        public void LDB_IndexedIDX1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDX1-10)"               //  0    2   Load B from IDX1 - 10
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x91); // LDB (IDX1+offset) opcode
            result[1].Should().Be(246);  // -10 as signed byte (256 - 10)
        }

        [Fact]
        public void LDA_IndexedIDY1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDY1+127)"              //  0    2   Load A from IDY1 + 127 (max positive offset)
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x92); // LDA (IDY1+offset) opcode
            result[1].Should().Be(127);  // Maximum positive offset
        }

        [Fact]
        public void LDB_IndexedIDY1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDB (IDY1-128)"              //  0    2   Load B from IDY1 - 128 (max negative offset)
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x93); // LDB (IDY1+offset) opcode
            result[1].Should().Be(128);  // -128 as signed byte
        }

        [Fact]
        public void STA_IndexedIDX1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDX1+8)"                //  0    2   Store A at IDX1 + 8
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x94); // STA (IDX1+offset) opcode
            result[1].Should().Be(8);    // Offset
        }

        [Fact]
        public void STB_IndexedIDX1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDX1-5)"                //  0    2   Store B at IDX1 - 5
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x95); // STB (IDX1+offset) opcode
            result[1].Should().Be(251);  // -5 as signed byte (256 - 5)
        }

        [Fact]
        public void STA_IndexedIDY1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STA (IDY1+16)"               //  0    2   Store A at IDY1 + 16
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x96); // STA (IDY1+offset) opcode
            result[1].Should().Be(16);   // Offset
        }

        [Fact]
        public void STB_IndexedIDY1WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STB (IDY1+0)"                //  0    2   Store B at IDY1 + 0
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x97); // STB (IDY1+offset) opcode
            result[1].Should().Be(0);    // Zero offset
        }

        [Fact]
        public void LDA_IndexedIDX2WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDX2+32)"               //  0    2   Load A from IDX2 + 32
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x98); // LDA (IDX2+offset) opcode
            result[1].Should().Be(32);   // Offset
        }

        [Fact]
        public void LDA_IndexedIDY2WithOffset_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDA (IDY2-1)"                //  0    2   Load A from IDY2 - 1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result[0].Should().Be(0x99); // LDA (IDY2+offset) opcode
            result[1].Should().Be(255);  // -1 as signed byte
        }

        #endregion

        #region Auto-Increment/Decrement Instructions (0xC4-0xCB)

        [Fact]
        public void LDAIX1_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIX1+"                     //  0    1   Load A from (IDX1), then increment IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC4); // LDAIX1+ opcode
        }

        [Fact]
        public void LDAIY1_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIY1+"                     //  0    1   Load A from (IDY1), then increment IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC5); // LDAIY1+ opcode
        }

        [Fact]
        public void STAIX1_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIX1+"                     //  0    1   Store A at (IDX1), then increment IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC6); // STAIX1+ opcode
        }

        [Fact]
        public void STAIY1_PostIncrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIY1+"                     //  0    1   Store A at (IDY1), then increment IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC7); // STAIY1+ opcode
        }

        [Fact]
        public void LDAIX1_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIX1-"                     //  0    1   Load A from (IDX1), then decrement IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC8); // LDAIX1- opcode
        }

        [Fact]
        public void LDAIY1_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "LDAIY1-"                     //  0    1   Load A from (IDY1), then decrement IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xC9); // LDAIY1- opcode
        }

        [Fact]
        public void STAIX1_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIX1-"                     //  0    1   Store A at (IDX1), then decrement IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xCA); // STAIX1- opcode
        }

        [Fact]
        public void STAIY1_PostDecrement_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "STAIY1-"                     //  0    1   Store A at (IDY1), then decrement IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xCB); // STAIY1- opcode
        }

        #endregion

        #region Index Register Arithmetic Instructions (0xE0-0xEB)

        [Fact]
        public void INCIX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "INCIX1"                      //  0    1   Increment IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE0); // INCIX1 opcode
        }

        [Fact]
        public void DECIX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "DECIX1"                      //  0    1   Decrement IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE1); // DECIX1 opcode
        }

        [Fact]
        public void INCIY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "INCIY1"                      //  0    1   Increment IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE2); // INCIY1 opcode
        }

        [Fact]
        public void DECIY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "DECIY1"                      //  0    1   Decrement IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE3); // DECIY1 opcode
        }

        [Fact]
        public void INCIX2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "INCIX2"                      //  0    1   Increment IDX2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE4); // INCIX2 opcode
        }

        [Fact]
        public void DECIX2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "DECIX2"                      //  0    1   Decrement IDX2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE5); // DECIX2 opcode
        }

        [Fact]
        public void INCIY2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "INCIY2"                      //  0    1   Increment IDY2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE6); // INCIY2 opcode
        }

        [Fact]
        public void DECIY2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "DECIY2"                      //  0    1   Decrement IDY2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xE7); // DECIY2 opcode
        }

        [Fact]
        public void ADDIX1_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "ADDIX1 #0x1000"              //  0    3   ADDIX1 - Add 16-bit immediate to IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xE8); // ADDIX1 #imm16 opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0x10); // High byte
        }

        [Fact]
        public void ADDIY1_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "ADDIY1 #500"                 //  0    3   Add 16-bit immediate to IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xEA); // ADDIY1 #imm16 opcode
            result[1].Should().Be(244);  // 500 & 0xFF = 244
            result[2].Should().Be(1);    // 500 >> 8 = 1
        }

        [Fact]
        public void ADDIX2_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "ADDIX2 #$FF00"               //  0    3   Add 16-bit immediate to IDX2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xE9); // ADDIX2 #imm16 opcode
            result[1].Should().Be(0x00); // Low byte
            result[2].Should().Be(0xFF); // High byte
        }

        [Fact]
        public void ADDIY2_ImmediateValue_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "ADDIY2 #1"                   //  0    3   Add 16-bit immediate to IDY2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0xEB); // ADDIY2 #imm16 opcode
            result[1].Should().Be(0x01); // Low byte
            result[2].Should().Be(0x00); // High byte
        }

        #endregion

        #region Index Register Transfer Instructions (0xF1-0xF9)

        [Fact]
        public void MVIX1IX2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIX1IX2"                    //  0    1   Move IDX1 to IDX2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF1); // MVIX1IX2 opcode
        }

        [Fact]
        public void MVIX2IX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIX2IX1"                    //  0    1   Move IDX2 to IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF2); // MVIX2IX1 opcode
        }

        [Fact]
        public void MVIY1IY2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIY1IY2"                    //  0    1   Move IDY1 to IDY2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF3); // MVIY1IY2 opcode
        }

        [Fact]
        public void MVIY2IY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIY2IY1"                    //  0    1   Move IDY2 to IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF4); // MVIY2IY1 opcode
        }

        [Fact]
        public void MVIX1IY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIX1IY1"                    //  0    1   Move IDX1 to IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF5); // MVIX1IY1 opcode
        }

        [Fact]
        public void MVIY1IX1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "MVIY1IX1"                    //  0    1   Move IDY1 to IDX1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF6); // MVIY1IX1 opcode
        }

        [Fact]
        public void SWPIX1IX2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "SWPIX1IX2"                   //  0    1   Swap IDX1 and IDX2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF7); // SWPIX1IX2 opcode
        }

        [Fact]
        public void SWPIY1IY2_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "SWPIY1IY2"                   //  0    1   Swap IDY1 and IDY2
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF8); // SWPIY1IY2 opcode
        }

        [Fact]
        public void SWPIX1IY1_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                                 // pos  size
                "SWPIX1IY1"                   //  0    1   Swap IDX1 and IDY1
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0xF9); // SWPIX1IY1 opcode
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
                "LDIX1 #SOURCE_ARRAY",                //  0    3   Load source pointer
                "LDIY1 #DEST_ARRAY",                  //  3    3   Load destination pointer  
                "LDA #10",                            //  6    2   Load counter
                "",
                "COPY_LOOP:",                         //  -    0   Label (position 8)
                "LDAIX1+",                            //  8    1   Load from source, increment pointer
                "STAIY1+",                            //  9    1   Store to dest, increment pointer
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
            result[0].Should().Be(0x1A);  // LDIX1 #imm16
            result[1].Should().Be(0x0F);  // SOURCE_ARRAY address low (15)
            result[2].Should().Be(0x00);  // SOURCE_ARRAY address high

            result[3].Should().Be(0x1C);  // LDIY1 #imm16
            result[4].Should().Be(0x19);  // DEST_ARRAY address low (25)
            result[5].Should().Be(0x00);  // DEST_ARRAY address high

            result[6].Should().Be(0x10);  // LDA #imm
            result[7].Should().Be(10);    // Counter value

            result[8].Should().Be(0xC4);  // LDAIX1+
            result[9].Should().Be(0xC7);  // STAIY1+
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
                "LDIX1 #PLAYER_STRUCT",               //  0    3   Point to player structure
                "LDA #100",                           //  3    2   Player X position
                "STA (IDX1+0)",                       //  5    2   Store X (offset 0)
                "LDA #50",                            //  7    2   Player Y position  
                "STA (IDX1+1)",                       //  9    2   Store Y (offset 1)
                "LDA #255",                           // 11    2   Player health
                "STA (IDX1+2)",                       // 13    2   Store health (offset 2)
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
            result[0].Should().Be(0x1A);  // LDIX1 #imm16
            result[1].Should().Be(0x10);  // PLAYER_STRUCT address low (16)
            result[2].Should().Be(0x00);  // PLAYER_STRUCT address high

            // Verify X position setup
            result[3].Should().Be(0x10);  // LDA #imm
            result[4].Should().Be(100);   // X value
            result[5].Should().Be(0x94);  // STA (IDX1+offset)
            result[6].Should().Be(0);     // Offset 0

            // Verify Y position setup
            result[7].Should().Be(0x10);  // LDA #imm
            result[8].Should().Be(50);    // Y value
            result[9].Should().Be(0x94);  // STA (IDX1+offset)
            result[10].Should().Be(1);    // Offset 1

            // Verify health setup
            result[11].Should().Be(0x10); // LDA #imm
            result[12].Should().Be(255);  // Health value
            result[13].Should().Be(0x94); // STA (IDX1+offset)
            result[14].Should().Be(2);    // Offset 2

            result[15].Should().Be(0x01); // HALT
        }

        #endregion

        #region Error Cases

        [Fact]
        public void IndexInstruction_WithInvalidOffset_ShouldThrowException()
        {
            // Arrange - Offset trop grand pour un signed byte
            string[] lines = { "LDA (IDX1+200)" }; // > 127

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Invalid offset in indexed expression*");
        }

        [Fact]
        public void IndexInstruction_WithInvalidNegativeOffset_ShouldThrowException()
        {
            // Arrange - Offset négatif trop grand
            string[] lines = { "LDA (IDX1-200)" }; // < -128

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*offset out of range*");
        }

        [Fact]
        public void IndexLoadInstruction_WithInvalidValue_ShouldThrowException()
        {
            // Arrange - Valeur trop grande pour 16-bit
            string[] lines = { "LDIX1 #65536" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<OverflowException>();
        }

        [Fact]
        public void IndexInstruction_WithMissingOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "LDIX1" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires one operand*");
        }

        [Fact]
        public void IndexArithmeticInstruction_WithMissingOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "ADDIX1" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*requires one operand*");
        }

        [Fact]
        public void IndexIncrementInstruction_WithExtraOperand_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "INCIX1 #5" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithInnerException<AssemblerException>()
                  .WithMessage("*Instruction INCIX1 takes no parameters*");
        }

        #endregion

        #region Label Support Tests

        [Fact]
        public void LDIX1_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "LDIX1 MESSAGE",  //  0    3   Load IDX1 with MESSAGE address
                "HALT",           //  3    1   Halt
                "MESSAGE:",       //  -    0   Label (position 4)
                "DB 42"           //  4    1   Data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x1A); // LDIX1 opcode
            result[1].Should().Be(0x04); // MESSAGE address low byte (position 4)
            result[2].Should().Be(0x00); // MESSAGE address high byte
            result[3].Should().Be(0x01); // HALT opcode
            result[4].Should().Be(42);   // Data at MESSAGE
        }

        [Fact]
        public void LDIX2_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "LDIX2 DATA",     //  0    3   Load IDX2 with DATA address
                "NOP",            //  3    1   
                "DATA:",          //  -    0   Label (position 4)
                "DB 0xFF"         //  4    1   Data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x1B); // LDIX2 opcode
            result[1].Should().Be(0x04); // DATA address low byte (position 4)
            result[2].Should().Be(0x00); // DATA address high byte
            result[3].Should().Be(0x00); // NOP opcode
            result[4].Should().Be(0xFF); // Data at DATA
        }

        [Fact]
        public void LDIY1_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "LDIY1 STRING",   //  0    3   Load IDY1 with STRING address
                "STRING:",        //  -    0   Label (position 3)
                "DB \"Hi\""       //  3    3   String "Hi" + null terminator
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result[0].Should().Be(0x1C); // LDIY1 opcode
            result[1].Should().Be(0x03); // STRING address low byte (position 3)
            result[2].Should().Be(0x00); // STRING address high byte
            result[3].Should().Be(72);   // 'H'
            result[4].Should().Be(105);  // 'i'
            result[5].Should().Be(0);    // null terminator
        }

        [Fact]
        public void LDIY2_WithLabel_ShouldGenerateCorrectCode()
        {
            // Arrange
            string[] lines = 
            {                     // pos  size
                "JMP MAIN",       //  0    3   Skip data
                "BUFFER:",        //  -    0   Label (position 3)
                "DB 0, 0, 0",     //  3    3   Buffer data
                "MAIN:",          //  -    0   Label (position 6)
                "LDIY2 BUFFER"    //  6    3   Load IDY2 with BUFFER address
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(9);
            result[0].Should().Be(0x40); // JMP opcode
            result[1].Should().Be(0x06); // MAIN address low byte (position 6)
            result[2].Should().Be(0x00); // MAIN address high byte
            result[3].Should().Be(0);    // Buffer data
            result[4].Should().Be(0);    // Buffer data
            result[5].Should().Be(0);    // Buffer data
            result[6].Should().Be(0x1D); // LDIY2 opcode
            result[7].Should().Be(0x03); // BUFFER address low byte (position 3)
            result[8].Should().Be(0x00); // BUFFER address high byte
        }

        [Fact]
        public void IndexLoad_ROMStyleProgram_ShouldAssembleCorrectly()
        {
            // Arrange - Test inspired by the ROM file
            string[] lines = 
            {                            // pos  size
                "Startup:",              //  -    0   Label
                "LDIX1 WelcomeMessage",  //  0    3   Load IDX1 with message address (ROM style!)
                "CALL PrintString",      //  3    3   Call print function
                "HALT",                  //  6    1   Halt
                "",
                "PrintString:",          //  -    0   Label (position 7)
                "NOP",                   //  7    1   Placeholder
                "RET",                   //  8    1   Return
                "",
                "WelcomeMessage:",       //  -    0   Label (position 9)
                "DB \"FS System v1.0\""
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(24);
            
            // Verify LDIX1 with label resolution
            result[0].Should().Be(0x1A); // LDIX1 opcode
            result[1].Should().Be(9);    // WelcomeMessage address low byte (position 9)
            result[2].Should().Be(0);    // WelcomeMessage address high byte
            
            // Verify CALL instruction
            result[3].Should().Be(0x60); // CALL opcode
            result[4].Should().Be(7);    // PrintString address low byte (position 7)
            result[5].Should().Be(0);    // PrintString address high byte
            
            // Verify HALT
            result[6].Should().Be(0x01); // HALT opcode
            
            // Verify the string data starts at position 9
            string welcomeMessage = System.Text.Encoding.ASCII.GetString(result, 9, 14);
            welcomeMessage.Should().Be("FS System v1.0");
            result[23].Should().Be(0); // null terminator at the end
        }

        [Fact]
        public void IndexLoad_BothSyntaxesSupported_ShouldWork()
        {
            // Arrange - Test both immediate and label syntax work
            string[] lines = 
            {                            // pos  size
                "LDIX1 #0x8000",         //  0    3   Immediate syntax
                "LDIX2 DATA_PTR",        //  3    3   Label syntax
                "LDIY1 #1000",           //  6    3   Immediate decimal
                "LDIY2 STRING_PTR",      //  9    3   Label syntax
                "HALT",                  // 12    1   Halt
                "DATA_PTR:",             //  -    0   Label (position 13)
                "DB 0xAB",               // 13    1   Data
                "STRING_PTR:",           //  -    0   Label (position 14)
                "DB \"OK\""              // 14    3   String + null
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(17);
            
            // Verify LDIX1 #0x8000
            result[0].Should().Be(0x1A); // LDIX1 opcode
            result[1].Should().Be(0x00); // 0x8000 low byte
            result[2].Should().Be(0x80); // 0x8000 high byte
            
            // Verify LDIX2 DATA_PTR
            result[3].Should().Be(0x1B); // LDIX2 opcode
            result[4].Should().Be(13);   // DATA_PTR address low byte (position 13)
            result[5].Should().Be(0);    // DATA_PTR address high byte
            
            // Verify LDIY1 #1000
            result[6].Should().Be(0x1C); // LDIY1 opcode
            result[7].Should().Be(232);  // 1000 & 0xFF (low byte)
            result[8].Should().Be(3);    // 1000 >> 8 (high byte)
            
            // Verify LDIY2 STRING_PTR
            result[9].Should().Be(0x1D);  // LDIY2 opcode
            result[10].Should().Be(14);   // STRING_PTR address low byte (position 14)
            result[11].Should().Be(0);    // STRING_PTR address high byte
            
            // Verify HALT
            result[12].Should().Be(0x01); // HALT opcode
            
            // Verify data
            result[13].Should().Be(0xAB); // Data at DATA_PTR
            result[14].Should().Be(79);   // 'O'
            result[15].Should().Be(75);   // 'K'
            result[16].Should().Be(0);    // null terminator
        }

        [Fact]
        public void ROM_Original_LDIX1_Label_ShouldAssemble()
        {
            // Arrange - Simplified version of the original ROM with LDIX1 label syntax
            string[] lines = 
            {
                "Startup:",
                "    LDIX1 WelcomeMessage",    // This should now work!
                "    CALL PrintString",
                "    HALT",
                "",
                "PrintString:",
                "PrintString_Loop:",
                "    LDA (IDX1)", 
                "    LDB #0",
                "    CMP A,B",
                "    JZ PrintString_End",
                "    INCIX1",
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
            assembleAction.Should().NotThrow("LDIX1 should now support label syntax like LDDA");
            
            byte[] result = _assembler.AssembleLines(lines);
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(0);
            
            // Verify that LDIX1 got the correct address for WelcomeMessage
            result[0].Should().Be(0x1A, "First instruction should be LDIX1 opcode");
            // The exact address will depend on the program layout, but it should be non-zero
            // since WelcomeMessage is defined later in the program
            ushort welcomeMessageAddress = (ushort)(result[1] | (result[2] << 8));
            welcomeMessageAddress.Should().BeGreaterThan(0, "WelcomeMessage should have a valid address");
        }

        [Fact]
        public void LDIX1_WelcomeMessage_ROM_Style_ShouldWork()
        {
            // Arrange - Test the exact ROM syntax that was failing
            string[] lines = 
            {                            // pos  size
                "LDIX1 WelcomeMessage",  //  0    3   This is the line that was failing!
                "HALT",                  //  3    1   
                "WelcomeMessage:",       //  -    0   Label (position 4)
                "DB \"FS System v1.0\""
            };

            // Act & Assert - Should NOT throw an exception
            Action assembleAction = () => _assembler.AssembleLines(lines);
            assembleAction.Should().NotThrow("LDIX1 with label should now work like LDDA");
            
            byte[] result = _assembler.AssembleLines(lines);
            
            // Assert
            result.Should().NotBeNull();
            result[0].Should().Be(0x1A, "LDIX1 opcode");
            result[1].Should().Be(4, "WelcomeMessage address low byte (position 4)");
            result[2].Should().Be(0, "WelcomeMessage address high byte"); 
            result[3].Should().Be(0x01, "HALT opcode");
            
            // Verify string content
            string message = System.Text.Encoding.ASCII.GetString(result, 4, 14);
            message.Should().Be("FS System v1.0");
        }
    }
    #endregion
}