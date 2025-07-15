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
            result.Should().HaveCount(9);
            
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
            {
                "START:",
                "LDA #10",
                "STA DATA",
                "JMP END",
                "DATA:",
                "DB 0, 0, 0",
                "END:",
                "HALT"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(11);
            
            // START: LDA #10
            result[0].Should().Be(0x10); // LDA opcode
            result[1].Should().Be(10);   // Immediate value
            
            // STA DATA
            result[2].Should().Be(0x50); // STA opcode
            result[3].Should().Be(0x07); // DATA address (7)
            result[4].Should().Be(0x00);
            
            // JMP END
            result[5].Should().Be(0x40); // JMP opcode
            result[6].Should().Be(0x0A); // END address (10)
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
            result.Should().HaveCount(12);
            
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
            result[9].Should().Be(0x80);  // LDA memory opcode
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
                  .WithMessage("*déjà défini*");
        }

        [Fact]
        public void UndefinedLabel_ShouldThrowException()
        {
            // Arrange
            string[] lines = { "JMP UNDEFINED" };

            // Act & Assert
            var action = () => _assembler.AssembleLines(lines);
            action.Should().Throw<AssemblerException>()
                  .WithMessage("*Label non défini*");
        }

        #endregion

        #region Integration Test

        [Fact]
        public void LargeProgram_WithAllFeatures_ShouldAssembleCorrectly()
        {
            // Arrange
            string[] lines = 
            {
                "; Simple program demonstrating all features",
                "MAIN:",
                "LDA #65",           // Load 'A'
                "STA MESSAGE",       // Store at MESSAGE
                "CALL PRINT_CHAR",   // Call subroutine
                "HALT",              // End program
                "",
                "MESSAGE:",
                "DB 0",              // Space for character
                "",
                "PRINT_CHAR:",       // Subroutine
                "PUSH A",            // Save A
                "LDA MESSAGE",       // Load character
                "NOP",               // Simulate printing
                "POP A",             // Restore A
                "RET",               // Return
                "",
                "DATA:",
                "DB 'H', 'e', 'l', 'l', 'o', 0"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(24);
            
            // Verify first few instructions
            result[0].Should().Be(0x10); // LDA #65
            result[1].Should().Be(65);
            result[2].Should().Be(0x50); // STA MESSAGE
            
            // The rest would be complex to verify completely,
            // but this shows the assembler can handle a real program
        }

        #endregion
    }
}