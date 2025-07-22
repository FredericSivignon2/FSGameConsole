using FSAssembler;
using FluentAssertions;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests unitaires pour la directive DB et les labels
    /// Tests des fonctionnalités avancées de l'assembleur
    /// </summary>
    public class DataAndLabelTests
    {
        private readonly Assembler _assembler;

        public DataAndLabelTests()
        {
            _assembler = new Assembler();
        }

        #region DB Directive Tests

        [Fact]
        public void DB_WithSingleByte_ShouldGenerateCorrectData()
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
        public void DB_WithMultipleBytes_ShouldGenerateCorrectData()
        {
            // Arrange
            string[] lines = { "DB 10, 20, 30, 40" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(10);
            result[1].Should().Be(20);
            result[2].Should().Be(30);
            result[3].Should().Be(40);
        }

        [Fact]
        public void DB_WithHexValues_ShouldGenerateCorrectData()
        {
            // Arrange
            string[] lines = { "DB 0xFF, 0x00, 0xAB" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(255);
            result[1].Should().Be(0);
            result[2].Should().Be(171);
        }

        [Fact]
        public void DB_WithDollarHexValues_ShouldGenerateCorrectData()
        {
            // Arrange
            string[] lines = { "DB $80, $7F, $01" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(128);
            result[1].Should().Be(127);
            result[2].Should().Be(1);
        }

        [Fact]
        public void DB_WithCharacterValues_ShouldGenerateCorrectData()
        {
            // Arrange
            string[] lines = { "DB 'A', 'B', 'C'" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(65);  // 'A'
            result[1].Should().Be(66);  // 'B'
            result[2].Should().Be(67);  // 'C'
        }

        [Fact]
        public void DB_WithMixedValues_ShouldGenerateCorrectData()
        {
            // Arrange
            string[] lines = { "DB 42, 0xFF, 'Z', $10" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(42);
            result[1].Should().Be(255);
            result[2].Should().Be(90);  // 'Z'
            result[3].Should().Be(16);  // $10
        }

        [Fact]
        public void DB_WithSpaces_ShouldHandleFormatting()
        {
            // Arrange
            string[] lines = { "DB  10 , 20  ,  30" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(10);
            result[1].Should().Be(20);
            result[2].Should().Be(30);
        }

        [Fact]
        public void DB_WithStringLiteral_ShouldGenerateCorrectDataWithNullTerminator()
        {
            // Arrange
            string[] lines = { "DB \"Hello\"" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6); // "Hello" + null terminator
            result[0].Should().Be((byte)'H'); // 72
            result[1].Should().Be((byte)'e'); // 101
            result[2].Should().Be((byte)'l'); // 108
            result[3].Should().Be((byte)'l'); // 108
            result[4].Should().Be((byte)'o'); // 111
            result[5].Should().Be(0);         // null terminator
        }

        [Fact]
        public void DB_WithMixedStringAndValues_ShouldGenerateCorrectData()
        {
            // Arrange
            string[] lines = { "DB \"Hi\", 42, 'A'" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5); // "Hi" + null + 42 + 'A'
            result[0].Should().Be((byte)'H'); // 72
            result[1].Should().Be((byte)'i'); // 105
            result[2].Should().Be(0);         // null terminator for string
            result[3].Should().Be(42);        // decimal value
            result[4].Should().Be((byte)'A'); // character value
        }

        [Fact]
        public void DB_WithEmptyString_ShouldGenerateNullTerminatorOnly()
        {
            // Arrange
            string[] lines = { "DB \"\"" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().Be(0); // null terminator only
        }

        [Fact]
        public void DB_WithStringContainingSpaces_ShouldHandleSpacesCorrectly()
        {
            // Arrange
            string[] lines = { "DB \"FS System v1.0\"" };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(15); // "FS System v1.0" (14 chars) + null terminator
            result[0].Should().Be((byte)'F');
            result[1].Should().Be((byte)'S');
            result[2].Should().Be((byte)' '); // space character
            result[3].Should().Be((byte)'S');
            result[4].Should().Be((byte)'y');
            result[5].Should().Be((byte)'s');
            result[6].Should().Be((byte)'t');
            result[7].Should().Be((byte)'e');
            result[8].Should().Be((byte)'m');
            result[9].Should().Be((byte)' '); // space character
            result[10].Should().Be((byte)'v');
            result[11].Should().Be((byte)'1');
            result[12].Should().Be((byte)'.');
            result[13].Should().Be((byte)'0');
            result[14].Should().Be(0); // null terminator
        }

        #endregion

        #region Label Tests

        [Fact]
        public void Label_AtBeginning_ShouldResolveToZero()
        {
            // Arrange
            string[] lines = 
            {
                "START:",
                "JMP START"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result[0].Should().Be(0x40); // JMP opcode
            result[1].Should().Be(0x00); // Low byte of address 0
            result[2].Should().Be(0x00); // High byte of address 0
        }

        [Fact]
        public void Label_AfterInstructions_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "NOP",
                "NOP",
                "TARGET:",
                "HALT",
                "JMP TARGET"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(6);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x00); // NOP
            result[2].Should().Be(0x01); // HALT (TARGET is here at address 2)
            result[3].Should().Be(0x40); // JMP opcode
            result[4].Should().Be(0x02); // Low byte of address 2
            result[5].Should().Be(0x00); // High byte of address 2
        }

        [Fact]
        public void Label_WithDB_ShouldCalculateAddressCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "DB 10, 20, 30",
                "DATA_END:",
                "LDA #42",
                "JMP DATA_END"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(8);
            result[0].Should().Be(10);   // DB data
            result[1].Should().Be(20);
            result[2].Should().Be(30);
            result[3].Should().Be(0x10); // LDA opcode (DATA_END is here at address 3)
            result[4].Should().Be(42);   // LDA immediate value
            result[5].Should().Be(0x40); // JMP opcode
            result[6].Should().Be(0x03); // Low byte of address 3
            result[7].Should().Be(0x00); // High byte of address 3
        }

        [Fact]
        public void MultipleLabels_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "START:",
                "JMP MIDDLE",
                "EARLY:",
                "HALT",
                "MIDDLE:",
                "JMP END",
                "END:",
                "JMP EARLY"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(10);
            
            // START: JMP MIDDLE
            result[0].Should().Be(0x40); // JMP
            result[1].Should().Be(0x04); // MIDDLE address (4)
            result[2].Should().Be(0x00);
            
            // EARLY: HALT
            result[3].Should().Be(0x01); // HALT (EARLY is at address 3)
            
            // MIDDLE: JMP END
            result[4].Should().Be(0x40); // JMP (MIDDLE is at address 4)
            result[5].Should().Be(0x07); // END address (7)
            result[6].Should().Be(0x00);
            
            // END: JMP EARLY
            result[7].Should().Be(0x40); // JMP (END is at address 7)
            result[8].Should().Be(0x03); // EARLY address (3)
            // result[9] would be 0x00 but array is only 9 elements
        }

        [Fact]
        public void Label_CaseInsensitive_ShouldWork()
        {
            // Arrange
            string[] lines = 
            {
                "loop:",
                "NOP",
                "JMP LOOP"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0x00); // NOP
            result[1].Should().Be(0x40); // JMP
            result[2].Should().Be(0x00); // Address 0 (loop)
            result[3].Should().Be(0x00);
        }

        #endregion

        #region Complex Program Tests

        [Fact]
        public void ComplexProgram_WithDataAndLabels_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines =
            {                   // pos  size
                "START:",       //  -    0   
                "LDA #10",      //  0    2   
                "STA DATA",     //  2    3   
                "JMP END",      //  5    3   
                "DATA:",        //  8    0   
                "DB 0, 0, 0",   //  8    3   
                "END:",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(12);
            
            // START: LDA #10
            result[0].Should().Be(0x10); // LDA opcode
            result[1].Should().Be(10);   // Immediate value
            
            // STA DATA
            result[2].Should().Be(0x50); // STA opcode
            result[3].Should().Be(0x08); // DATA address (8)
            result[4].Should().Be(0x00);
            
            // JMP END
            result[5].Should().Be(0x40); // JMP opcode
            result[6].Should().Be(0x0B); // END address (11)
            result[7].Should().Be(0x00);
            
            // DATA: DB 0, 0, 0
            result[8].Should().Be(0);
            result[9].Should().Be(0);
            result[10].Should().Be(0);
            
            // END: HALT would be at address 10, but it's missing in this test
        }

        [Fact]
        public void ProgramWithStringData_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "JMP MAIN",
                "MESSAGE:",
                "DB 'H', 'e', 'l', 'l', 'o', 0",
                "MAIN:",
                "LDA MESSAGE",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(13);
            
            // JMP MAIN
            result[0].Should().Be(0x40); // JMP
            result[1].Should().Be(0x09); // MAIN address (9)
            result[2].Should().Be(0x00);
            
            // MESSAGE: "Hello\0"
            result[3].Should().Be(72);  // 'H'
            result[4].Should().Be(101); // 'e'
            result[5].Should().Be(108); // 'l'
            result[6].Should().Be(108); // 'l'
            result[7].Should().Be(111); // 'o'
            result[8].Should().Be(0);   // null terminator
            
            // MAIN: LDA MESSAGE
            result[9].Should().Be(0x90);  // LDA memory opcode
            result[10].Should().Be(0x03); // MESSAGE address (3)
            result[11].Should().Be(0x00);
        }

        #endregion

        #region Forward and Backward Reference Tests

        [Fact]
        public void ForwardReference_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "JMP FORWARD",  // Reference before definition
                "NOP",
                "FORWARD:",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(5);
            result[0].Should().Be(0x40); // JMP
            result[1].Should().Be(0x04); // FORWARD address (4)
            result[2].Should().Be(0x00);
            result[3].Should().Be(0x00); // NOP
            result[4].Should().Be(0x01); // HALT
        }

        [Fact]
        public void BackwardReference_ShouldResolveCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "BACKWARD:",
                "NOP",
                "JMP BACKWARD"  // Reference after definition
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(4);
            result[0].Should().Be(0x00); // NOP (BACKWARD is at address 0)
            result[1].Should().Be(0x40); // JMP
            result[2].Should().Be(0x00); // BACKWARD address (0)
            result[3].Should().Be(0x00);
        }

        #endregion

        #region Error Cases

        [Fact]
        public void DB_WithInvalidValue_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "DB 256" }; // Value too large for byte

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<OverflowException>();
        }

        [Fact]
        public void DB_WithInvalidCharacter_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "DB 'AB'" }; // Multi-character literal

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<Exception>();
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

        [Fact]
        public void UndefinedLabel_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JMP UNDEFINED" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Undefined label: UNDEFINED*");
        }

        #endregion

        #region Integration Test

        [Fact]
        public void LargeProgram_WithAllFeatures_ShouldAssembleCorrectly()
        {
            // Arrange - Complete program demonstrating all features
            string[] lines = 
            {                                       // pos  size
                "; Simple program demonstrating all features",
                "MAIN:",                            //  -    0   Label
                "LDA #65",                          //  0    2   Load 'A' character
                "STA MESSAGE",                      //  2    3   Store at MESSAGE
                "CALL PRINT_CHAR",                  //  5    3   Call subroutine
                "HALT",                             //  8    1   End program
                "",
                "MESSAGE:",                         //  -    0   Label (position 9)
                "DB 0",                             //  9    1   Space for character
                "",
                "PRINT_CHAR:",                      //  -    0   Label (position 10)
                "PUSH A",                           // 10    1   Save A
                "LDA MESSAGE",                      // 11    3   Load character
                "NOP",                              // 14    1   Simulate printing
                "POP A",                            // 15    1   Restore A
                "RET",                              // 16    1   Return
                "",
                "DATA:",                            //  -    0   Label (position 17)
                "DB 'H', 'e', 'l', 'l', 'o', 0"   // 17    6   String data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(23);  // Total: 2+3+3+1+1+1+3+1+1+1+6 = 23 bytes
            
            // Verify main function
            result[0].Should().Be(0x10); // LDA #65
            result[1].Should().Be(65);
            result[2].Should().Be(0x50); // STA MESSAGE
            result[3].Should().Be(0x09); // MESSAGE address (9)
            result[4].Should().Be(0x00);
            result[5].Should().Be(0x60); // CALL
            result[6].Should().Be(0x0A); // PRINT_CHAR address (10)
            result[7].Should().Be(0x00);
            result[8].Should().Be(0x01); // HALT
            
            // Verify MESSAGE location
            result[9].Should().Be(0);    // MESSAGE data
            
            // Verify PRINT_CHAR subroutine
            result[10].Should().Be(0x70); // PUSH A
            result[11].Should().Be(0x90); // LDA MESSAGE (memory mode)
            result[12].Should().Be(0x09); // MESSAGE address (9)
            result[13].Should().Be(0x00);
            result[14].Should().Be(0x00); // NOP
            result[15].Should().Be(0x71); // POP A
            result[16].Should().Be(0x61); // RET
            
            // Verify DATA section
            result[17].Should().Be(72);   // 'H'
            result[18].Should().Be(101);  // 'e'
            result[19].Should().Be(108);  // 'l'
            result[20].Should().Be(108);  // 'l'
            result[21].Should().Be(111);  // 'o'
            result[22].Should().Be(0);    // null terminator
        }

        #endregion

        #region ROM-Style Program Tests

        [Fact]
        public void DB_ROMStyle_WelcomeMessage_ShouldAssembleCorrectly()
        {
            // Arrange - Test inspired by the ROM line: DB "FS System v1.0"
            string[] lines = 
            {
                "WelcomeMessage:",
                "DB \"FS System v1.0\""
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(15); // 14 characters + null terminator

            // Verify the exact string content
            result[0].Should().Be(0x46); // 'F'
            result[1].Should().Be(0x53); // 'S'
            result[2].Should().Be(0x20); // ' '
            result[3].Should().Be(0x53); // 'S'
            result[4].Should().Be(0x79); // 'y'
            result[5].Should().Be(0x73); // 's'
            result[6].Should().Be(0x74); // 't'
            result[7].Should().Be(0x65); // 'e'
            result[8].Should().Be(0x6D); // 'm'
            result[9].Should().Be(0x20); // ' '
            result[10].Should().Be(0x76); // 'v'
            result[11].Should().Be(0x31); // '1'
            result[12].Should().Be(0x2E); // '.'
            result[13].Should().Be(0x30); // '0'
            result[14].Should().Be(0x00); 
        }

        [Fact] 
        public void CompleteROMStyleProgram_ShouldAssembleWithStringSupport()
        {
            // Arrange - Simplified version of ROM startup with string support
            string[] lines = 
            {                         // pos  size
                "Startup:",           //  -    0   Label
                "LDDA WelcomeMessage",//  0    3   Load message address
                "CALL PrintString",   //  3    3   Call print function  
                "HALT",               //  6    1   Halt program
                "",
                "PrintString:",       //  -    0   Label (position 7)
                "NOP",                //  7    1   Placeholder function
                "RET",                //  8    1   Return
                "",
                "WelcomeMessage:",    //  -    0   Label (position 9)
                "DB \"FS System v1.0\"" // 9   15   String with null terminator
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(24); // 9 instruction bytes + 15 string bytes + null = 24 total
            
            // Verify LDDA instruction with correct address resolution
            result[0].Should().Be(0x18); // LDDA opcode
            result[1].Should().Be(9);    // WelcomeMessage address low byte 
            result[2].Should().Be(0);    // WelcomeMessage address high byte
            
            // Verify CALL instruction
            result[3].Should().Be(0x60); // CALL opcode
            result[4].Should().Be(7);    // PrintString address low byte
            result[5].Should().Be(0);    // PrintString address high byte
            
            // Verify HALT
            result[6].Should().Be(0x01); // HALT opcode
            
            // Verify PrintString function
            result[7].Should().Be(0x00); // NOP opcode
            result[8].Should().Be(0x61); // RET opcode
            
            // Verify the string data starts at position 9
            string welcomeMessage = System.Text.Encoding.ASCII.GetString(result, 9, 14);
            welcomeMessage.Should().Be("FS System v1.0");
            result[23].Should().Be(0); // null terminator at the end
        }

        #endregion
    }
}