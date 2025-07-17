using FSAssembler;
using FluentAssertions;
using System.IO;

namespace FSAssemblerTests
{
    /// <summary>
    /// Tests d'intégration pour l'assembleur FSAssembler
    /// Tests de programmes complets et de scénarios réalistes
    /// </summary>
    public class AssemblerIntegrationTests
    {
        private readonly Assembler _assembler;

        public AssemblerIntegrationTests()
        {
            _assembler = new Assembler();
        }

        #region Complete Program Tests

        [Fact]
        public void SimpleLoop_ShouldAssembleAndWork()
        {
            // Arrange - A simple loop that counts from 0 to 9
            string[] lines = 
            {                     // pos  size
                "LOOP:",          //  -    0   Label
                "LDA #0",         //  0    2   Initialize counter
                "COUNTER:",       //  -    0   Label
                "INC A",          //  2    1   Increment counter
                "CMP A,C",        //  3    1   Compare with limit (assuming C contains 10)
                "JZ END",         //  4    3   Jump to end if equal
                "JMP COUNTER",    //  7    3   Continue loop
                "END:",           //  -    0   Label
                "HALT"            // 10    1   Halt
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(11);  // Total: 2+1+1+3+3+1 = 11 bytes
            
            // Verify key instructions
            result[0].Should().Be(0x10); // LDA #0
            result[1].Should().Be(0);
            result[2].Should().Be(0x28); // INC A
            result[3].Should().Be(0x2F); // CMP A,C
            result[4].Should().Be(0x41); // JZ
            result[5].Should().Be(0x0A); // END address (10)
            result[6].Should().Be(0x00);
            result[7].Should().Be(0x40); // JMP
            result[8].Should().Be(0x02); // COUNTER address (2)
            result[9].Should().Be(0x00);
            result[10].Should().Be(0x01); // HALT
        }

        [Fact]
        public void ArithmeticCalculation_ShouldAssembleCorrectly()
        {
            // Arrange - Calculate (A + B) * C and store result
            string[] lines = 
            {                     // pos  size
                "MAIN:",          //  -    0   Label
                "LDA #10",        //  0    2   Load 10 into A
                "LDB #5",         //  2    2   Load 5 into B
                "LDC #3",         //  4    2   Load 3 into C
                "ADD",            //  6    1   A = A + B (15)
                "MOV B,A",        //  7    1   Move result to B
                "MOV A,C",        //  8    1   Move C to A
                "SUB",            //  9    1   A = A - B (3 - 15 = -12, but unsigned)
                "STA RESULT",     // 10    3   Store result
                "HALT",           // 13    1   Halt
                "RESULT:",        //  -    0   Label
                "DB 0"            // 14    1   Result data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(15);  // Total: 2+2+2+1+1+1+1+3+1+1 = 15 bytes
            
            // Verify the structure
            result[0].Should().Be(0x10); // LDA #10
            result[1].Should().Be(10);
            result[2].Should().Be(0x11); // LDB #5
            result[3].Should().Be(5);
            result[4].Should().Be(0x12); // LDC #3
            result[5].Should().Be(3);
            result[6].Should().Be(0x20); // ADD
            result[7].Should().Be(0xA2); // MOV B,A
            result[8].Should().Be(0xA1); // MOV A,C
            result[9].Should().Be(0x21); // SUB
            result[10].Should().Be(0x50); // STA
            result[11].Should().Be(0x0E); // RESULT address (14)
            result[12].Should().Be(0x00);
            result[13].Should().Be(0x01); // HALT
            result[14].Should().Be(0);   // RESULT data
        }

        [Fact]
        public void SubroutineWithParameters_ShouldAssembleCorrectly()
        {
            // Arrange - Subroutine that adds two numbers
            string[] lines = 
            {                     // pos  size
                "MAIN:",          //  -    0   Label
                "LDA #20",        //  0    2   First parameter
                "LDB #30",        //  2    2   Second parameter
                "CALL ADD_FUNC",  //  4    3   Call addition function
                "STA RESULT",     //  7    3   Store result
                "HALT",           // 10    1   Halt
                "",
                "ADD_FUNC:",      //  -    0   Label (position 11)
                "PUSH C",         // 11    1   Save C register
                "MOV C,A",        // 12    1   Move A to C
                "MOV A,B",        // 13    1   Move B to A
                "MOV B,C",        // 14    1   Move C (original A) to B
                "ADD",            // 15    1   A = A + B
                "POP C",          // 16    1   Restore C register
                "RET",            // 17    1   Return with result in A
                "",
                "RESULT:",        //  -    0   Label (position 18)
                "DB 0"            // 18    1   Result data
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(19);  // Total: 2+2+3+3+1+1+1+1+1+1+1+1+1 = 19 bytes
            
            // Verify main function
            result[0].Should().Be(0x10); // LDA #20
            result[1].Should().Be(20);
            result[2].Should().Be(0x11); // LDB #30
            result[3].Should().Be(30);
            result[4].Should().Be(0x60); // CALL
            result[5].Should().Be(0x0B); // ADD_FUNC address (11)
            result[6].Should().Be(0x00);
            result[7].Should().Be(0x50); // STA
            result[8].Should().Be(0x12); // RESULT address (18)
            result[9].Should().Be(0x00);
            result[10].Should().Be(0x01); // HALT
            
            // Verify subroutine exists
            result[11].Should().Be(0x78); // PUSH C
            result[12].Should().Be(0xA4); // MOV C,A
            result[13].Should().Be(0xA0); // MOV A,B
            result[14].Should().Be(0xA3); // MOV B,C
            result[15].Should().Be(0x20); // ADD
            result[16].Should().Be(0x79); // POP C
            result[17].Should().Be(0x61); // RET
            result[18].Should().Be(0);   // RESULT data
        }

        [Fact]
        public void ComplexLogicalOperations_ShouldAssembleCorrectly()
        {
            // Arrange - Complex logical operations with bit manipulation
            string[] lines = 
            {                     // idx  length
                "START:",
                "LDA #0xFF",      //  0    2  Load all bits set
                "LDB #0x0F",      //  2    2  Load lower nibble mask
                "AND A,B",        //  4    1  A = A AND B (0x0F)
                "SHL A",          //  5    1  Shift left (0x1E)
                "LDC #0x81",      //  6    2  Load test pattern
                "XOR A,C",        //  8    1  A = A XOR C
                "NOT",            //  9    1  A = NOT A
                "STA RESULT1",    // 10    3  Store intermediate result
                "",
                "LDA RESULT1",    // 13    3  Load back
                "SHR",            // 16    1  Shift right
                "OR A,B",         // 17    1  A = A OR B
                "STA RESULT2",    // 18    3  Store final result
                "HALT",           // 21    1  Halt
                "",
                "RESULT1:",       //
                "DB 0",
                "RESULT2:",
                "DB 0"
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(24);  // Corrected from 23 to 24
            
            // Verify logical operations are present
            result[4].Should().Be(0x30); // AND A,B
            result[5].Should().Be(0x34); // SHL A
            result[8].Should().Be(0x38); // XOR A,C
            result[9].Should().Be(0x33); // NOT
            result[16].Should().Be(0x35); // SHR
            result[17].Should().Be(0x31); // OR A,B
        }

        [Fact]
        public void StackOperationsDemo_ShouldAssembleCorrectly()
        {
            // Arrange - Demonstrate stack operations
            string[] lines = 
            {                     // pos  size
                "MAIN:",          //  -    0   Label
                "LDA #10",        //  0    2   Setup test values
                "LDB #20",        //  2    2   
                "LDC #30",        //  4    2   
                "LDDA #0x1234",   //  6    3   16-bit value
                "",
                "PUSH A",         //  9    1   Push all values onto stack
                "PUSH B",         // 10    1   
                "PUSH C",         // 11    1   
                "PUSH DA",        // 12    1   
                "",
                "LDA #99",        // 13    2   Overwrite registers
                "LDB #88",        // 15    2   
                "LDC #77",        // 17    2   
                "LDDA #0x5678",   // 19    3   
                "",
                "POP DA",         // 22    1   Restore from stack (reverse order)
                "POP C",          // 23    1   
                "POP B",          // 24    1   
                "POP A",          // 25    1   
                "",
                "STA FINAL_A",    // 26    3   Store restored values
                "STB FINAL_B",    // 29    3   
                "STC FINAL_C",    // 32    3   
                "STDA FINAL_DA",  // 35    3   
                "HALT",           // 38    1   
                "",
                "FINAL_A:", "DB 0",      // 39    1   Data
                "FINAL_B:", "DB 0",      // 40    1   Data
                "FINAL_C:", "DB 0",      // 41    1   Data
                "FINAL_DA:", "DB 0, 0"   // 42    2   Data (16-bit)
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(44);  // Total: 2+2+2+3+1+1+1+1+2+2+2+3+1+1+1+1+3+3+3+3+1+1+1+1+2 = 44 bytes
            
            // Verify stack operations
            result[9].Should().Be(0x70);  // PUSH A
            result[10].Should().Be(0x74); // PUSH B
            result[11].Should().Be(0x78); // PUSH C
            result[12].Should().Be(0x72); // PUSH DA
            
            result[22].Should().Be(0x73); // POP DA
            result[23].Should().Be(0x79); // POP C
            result[24].Should().Be(0x75); // POP B
            result[25].Should().Be(0x71); // POP A
            
            // Verify store operations
            result[26].Should().Be(0x50); // STA
            result[27].Should().Be(0x27); // FINAL_A address (39)
            result[29].Should().Be(0x53); // STB
            result[30].Should().Be(0x28); // FINAL_B address (40)
            result[32].Should().Be(0x54); // STC
            result[33].Should().Be(0x29); // FINAL_C address (41)
            result[35].Should().Be(0x51); // STDA
            result[36].Should().Be(0x2A); // FINAL_DA address (42)
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void MemoryCopyRoutine_ShouldAssembleCorrectly()
        {
            // Arrange - Copy data from one location to another
            string[] lines = 
            {                               // pos  size
                "MAIN:",                    //  -    0   Label
                "LDDA #22",                 //  0    3   Load source address (22 is SOURCE position)
                "LDDB #27",                 //  3    3   Load destination address (27 is DEST position)
                "LDC #5",                   //  6    2   Load count
                "",
                "COPY_LOOP:",               //  -    0   Label (position 8)
                "LDA SOURCE",               //  8    3   This is simplified - real version would use DA/DB
                "STA DEST",                 // 11    3   This is simplified - real version would use DA/DB
                "DEC C",                    // 14    1   Decrement counter
                "JZ COPY_DONE",             // 15    3   Exit if done
                "JMP COPY_LOOP",            // 18    3   Continue loop
                "",
                "COPY_DONE:",               //  -    0   Label (position 21)
                "HALT",                     // 21    1   Halt
                "",
                "SOURCE:",                  //  -    0   Label (position 22)
                "DB 1, 2, 3, 4, 5",         // 22    5   Source data
                "DEST:",                    //  -    0   Label (position 27)
                "DB 0, 0, 0, 0, 0"          // 27    5   Destination space
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(32);  // Total: 3+3+2+3+3+1+3+3+1+5+5 = 32 bytes
            
            // Verify copy loop instructions
            result[8].Should().Be(0x80);  // LDA SOURCE
            result[9].Should().Be(0x16);  // SOURCE address low (22)
            result[10].Should().Be(0x00); // SOURCE address high
            result[11].Should().Be(0x50); // STA DEST
            result[12].Should().Be(0x1B); // DEST address low (27)
            result[13].Should().Be(0x00); // DEST address high
            result[14].Should().Be(0x2E); // DEC C
            result[15].Should().Be(0x41); // JZ
            result[16].Should().Be(0x15); // COPY_DONE address (21)
            
            // Verify source data
            result[22].Should().Be(1);
            result[23].Should().Be(2);
            result[24].Should().Be(3);
            result[25].Should().Be(4);
            result[26].Should().Be(5);
            
            // Verify destination space
            result[27].Should().Be(0);
            result[28].Should().Be(0);
            result[29].Should().Be(0);
            result[30].Should().Be(0);
            result[31].Should().Be(0);
        }

        [Fact]
        public void CalculatorProgram_ShouldAssembleCorrectly()
        {
            // Arrange - Simple calculator that can add, subtract, multiply (by addition)
            string[] lines = 
            {                     // pos  size
                "MAIN:",          //  -    0   Label
                "LDA NUM1",       //  0    3   Load first number (memory addressing)
                "LDB NUM2",       //  3    3   Load second number (memory addressing)
                "LDC OPERATION",  //  6    3   Load operation (1=add, 2=sub, 3=mul) (memory addressing)
                "",
                "LDA #1",         //  9    2   Load 1 into A for comparison (immediate)
                "CMP A,C",        // 11    1   Compare operation with 1
                "JZ DO_ADD",      // 12    3   Jump if zero
                "LDA #2",         // 15    2   Load 2 into A for comparison (immediate)
                "CMP A,C",        // 17    1   Compare operation with 2
                "JZ DO_SUB",      // 18    3   Jump if zero
                "LDA #3",         // 21    2   Load 3 into A for comparison (immediate)
                "CMP A,C",        // 23    1   Compare operation with 3
                "JZ DO_MUL",      // 24    3   Jump if zero
                "JMP ERROR",      // 27    3   Jump to error
                "",
                "DO_ADD:",        //  -    0   Label (position 30)
                "LDA NUM1",       // 30    3   Reload first number
                "LDB NUM2",       // 33    3   Reload second number
                "ADD",            // 36    1   A = A + B
                "JMP STORE_RESULT", // 37  3   Jump to store
                "",
                "DO_SUB:",        //  -    0   Label (position 40)
                "LDA NUM1",       // 40    3   Reload first number
                "LDB NUM2",       // 43    3   Reload second number
                "SUB",            // 46    1   A = A - B
                "JMP STORE_RESULT", // 47  3   Jump to store
                "",
                "DO_MUL:",        //  -    0   Label (position 50)
                "LDA NUM1",       // 50    3   Load first number
                "LDB NUM2",       // 53    3   Load second number
                "PUSH A",         // 56    1   Save original A
                "LDA #0",         // 57    2   Initialize result
                "MUL_LOOP:",      //  -    0   Label (position 59)
                "ADD",            // 59    1   Add B to A
                "DEC C",          // 60    1   Decrement counter (reusing C)
                "JZ MUL_DONE",    // 61    3   Jump if done
                "JMP MUL_LOOP",   // 64    3   Continue loop
                "MUL_DONE:",      //  -    0   Label (position 67)
                "POP B",          // 67    1   Clean up stack (was A)
                "JMP STORE_RESULT", // 68  3   Jump to store
                "",
                "STORE_RESULT:",  //  -    0   Label (position 71)
                "STA RESULT",     // 71    3   Store result
                "HALT",           // 74    1   Halt
                "",
                "ERROR:",         //  -    0   Label (position 75)
                "LDA #255",       // 75    2   Error code
                "STA RESULT",     // 77    3   Store error
                "HALT",           // 80    1   Halt
                "",
                "NUM1:", "DB 15", // 81    1   Data: first number
                "NUM2:", "DB 7",  // 82    1   Data: second number
                "OPERATION:", "DB 1",  // 83  1   Addition operation
                "RESULT:", "DB 0" // 84    1   Result storage
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(85);  // Total: 3+3+3+2+1+3+2+1+3+2+1+3+3+3+3+1+3+3+3+1+3+3+3+1+2+1+1+3+3+1+3+3+1+2+3+1+1+1+1 = 85 bytes
            
            // Verify main logic
            result[0].Should().Be(0x80);  // LDA NUM1 (memory)
            result[1].Should().Be(0x51);  // NUM1 address low (81)
            result[2].Should().Be(0x00);  // NUM1 address high
            result[9].Should().Be(0x10);  // LDA #1 (immediate)
            result[10].Should().Be(1);    // Value 1
            result[11].Should().Be(0x2F); // CMP A,C
            result[12].Should().Be(0x41); // JZ DO_ADD
            result[13].Should().Be(0x1E); // DO_ADD address (30)
            
            // Verify data section (at the end)
            result[81].Should().Be(15);  // NUM1
            result[82].Should().Be(7);   // NUM2
            result[83].Should().Be(1);   // OPERATION (add)
            result[84].Should().Be(0);   // RESULT
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void ProgramWithAllInstructionTypes_ShouldCompileWithoutErrors()
        {
            // Arrange - Comprehensive test using every instruction type
            string[] lines = 
            {
                "; Comprehensive instruction test",
                "MAIN:",
                "NOP",                    // Basic
                "",
                "; Load instructions",
                "LDA #42", "LDB #43", "LDC #44", "LDD #45", "LDE #46", "LDF #47",
                "LDDA #0x1234", "LDDB #0x5678",
                "LDA 0x8000", "LDB 0x8001",
                "",
                "; Arithmetic",
                "ADD", "SUB", "ADD16", "SUB16",
                "INC A", "DEC B", "INC C", "DEC C",
                "INC16 DA", "DEC16 DB",
                "CMP A,B", "CMP A,C",
                "",
                "; Logical",
                "AND A,B", "OR A,C", "XOR A,B",
                "NOT", "SHL A", "SHL B", "SHR",
                "",
                "; Jumps",
                "JMP SKIP1",
                "SKIP1:",
                "JZ SKIP2", "JNZ SKIP3", "JC SKIP4", "JNC SKIP5", "JN SKIP6", "JNN SKIP7",
                "SKIP2:", "SKIP3:", "SKIP4:", "SKIP5:", "SKIP6:", "SKIP7:",
                "",
                "; Relative jumps",
                "JR +5", "JRZ -3", "JRNZ +1", "JRC -1",
                "",
                "; Store",
                "STA 0x9000", "STB 0x9001", "STC 0x9002", "STD 0x9003",
                "STDA 0x9004", "STDB 0x9006",
                "",
                "; Transfer",
                "MOV A,B", "MOV B,C", "MOV C,A",
                "SWP A,B", "SWP A,C",
                "",
                "; Stack & Subroutines",
                "PUSH A", "PUSH DA", "CALL SUB1",
                "POP DA", "POP A",
                "HALT",
                "",
                "SUB1:",
                "PUSH B", "PUSH C",
                "NOP",
                "POP C", "POP B",
                "RET",
                "",
                "; System",
                "SYS",
                "",
                "; Data",
                "DATA:",
                "DB 1, 2, 3, 'A', 'B', 0xFF, $80"
            };

            // Act
            var action = () => _assembler.AssembleLines(lines);

            // Assert
            action.Should().NotThrow();
            
            byte[] result = _assembler.AssembleLines(lines);
            result.Should().NotBeNull();
            result.Length.Should().BeGreaterThan(50); // Should be a substantial program
        }

        [Fact]
        public void ProgramWithComplexJumps_ShouldResolveAllLabels()
        {
            // Arrange - Test forward and backward references
            string[] lines = 
            {                     // pos  size
                "START:",         //  -    0   Label
                "JMP FORWARD1",   //  0    3   Forward reference
                "",
                "BACK1:",         //  -    0   Label (position 3)
                "JMP FORWARD2",   //  3    3   Forward reference
                "NOP",            //  6    1   
                "",
                "BACK2:",         //  -    0   Label (position 7)
                "JMP END",        //  7    3   Forward reference
                "",
                "FORWARD1:",      //  -    0   Label (position 10)
                "JMP BACK1",      // 10    3   Backward reference
                "",
                "FORWARD2:",      //  -    0   Label (position 13)
                "JMP BACK2",      // 13    3   Backward reference
                "",
                "END:",           //  -    0   Label (position 16)
                "HALT"            // 16    1   Halt
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(17);  // Total: 3+3+1+3+3+3+1 = 17 bytes
            
            // All jumps should resolve without errors
            // START: JMP FORWARD1
            result[0].Should().Be(0x40);
            result[1].Should().Be(0x0A); // FORWARD1 address (10)
            result[2].Should().Be(0x00);
            
            // BACK1: JMP FORWARD2  
            result[3].Should().Be(0x40);
            result[4].Should().Be(0x0D); // FORWARD2 address (13)
            result[5].Should().Be(0x00);
            
            // NOP
            result[6].Should().Be(0x00);
            
            // BACK2: JMP END
            result[7].Should().Be(0x40);
            result[8].Should().Be(0x10); // END address (16)
            result[9].Should().Be(0x00);
            
            // FORWARD1: JMP BACK1
            result[10].Should().Be(0x40);
            result[11].Should().Be(0x03); // BACK1 address (3)
            result[12].Should().Be(0x00);
            
            // FORWARD2: JMP BACK2
            result[13].Should().Be(0x40);
            result[14].Should().Be(0x07); // BACK2 address (7)
            result[15].Should().Be(0x00);
            
            // END: HALT
            result[16].Should().Be(0x01);
        }

        #endregion

        #region File Operations Tests

        [Fact]
        public void AssembleFile_WithValidFile_ShouldWork()
        {
            // Arrange
            string tempFile = Path.GetTempFileName();
            string[] program = 
            {                     // pos  size
                "; Test program", //  -    0   Comment (ignored)
                "LDA #42",        //  0    2   Load immediate
                "STA 0x8000",     //  2    3   Store to memory
                "HALT"            //  5    1   Halt
            };
            
            File.WriteAllLines(tempFile, program);

            try
            {
                // Act
                byte[] result = _assembler.AssembleFile(tempFile);

                // Assert
                result.Should().NotBeNull();
                result.Should().HaveCount(6);  // Total: 2+3+1 = 6 bytes
                result[0].Should().Be(0x10); // LDA immediate opcode
                result[1].Should().Be(42);   // Immediate value
                result[2].Should().Be(0x50); // STA memory opcode
                result[3].Should().Be(0x00); // Address low byte
                result[4].Should().Be(0x80); // Address high byte
                result[5].Should().Be(0x01); // HALT opcode
            }
            finally
            {
                // Cleanup
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void LargeProgram_ShouldAssembleInReasonableTime()
        {
            // Arrange - Generate a large program
            var lines = new List<string>();
            lines.Add("MAIN:");
            
            // Add 1000 instructions
            for (int i = 0; i < 1000; i++)
            {
                lines.Add($"LDA #{i % 256}");
                lines.Add("NOP");
            }
            lines.Add("HALT");

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            byte[] result = _assembler.AssembleLines(lines.ToArray());
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(3001); // 1000 * (2 + 1) + 1 = 3001 bytes total
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in under 1 second
        }

        [Fact]
        public void IndexInstructionsWithLargeProgram_ShouldAssembleInReasonableTime()
        {
            // Arrange - Generate a large program with index instructions
            var lines = new List<string>();
            lines.Add("MAIN:");
            lines.Add("LDIX1 #0x8000");
            lines.Add("LDIY1 #0x9000");
            
            // Add 500 index operations
            for (int i = 0; i < 500; i++)
            {
                lines.Add("LDAIX1+");
                lines.Add("STAIY1+");
                lines.Add("INCIX1");
                lines.Add("DECIY1");
            }
            lines.Add("HALT");

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            byte[] result = _assembler.AssembleLines(lines.ToArray());
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Length.Should().Be(2009); // 3+3+500*(1+1+1+1)+1 = 2009 bytes total
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in under 1 second
        }

        #endregion

        #region Index Register Instructions Integration Tests

        [Fact]
        public void IndexArrayCopyProgram_ShouldAssembleCorrectly()
        {
            // Arrange - Efficient array copy using index registers with auto-increment
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
                "DEST_ARRAY:",                       //  -    0   Label (position 25)
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
        public void IndexStructureAccessProgram_ShouldAssembleCorrectly()
        {
            // Arrange - Structure access using index registers with offset
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
                "",
                "PLAYER_READ:",                       //  -    0   Label (position 15)
                "LDA (IDX1+0)",                       // 15    2   Read X position
                "LDB (IDX1+1)",                       // 17    2   Read Y position
                "LDA (IDX1+2)",                       // 19    2   Read health (overwrites X in A)
                "HALT",                               // 21    1   End program
                "",
                "PLAYER_STRUCT:",                     //  -    0   Label (position 22)
                "DB 0, 0, 0"                          // 22    3   X, Y, Health
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(25); // Total: 3+2+2+2+2+2+2+2+2+2+1+3+2+1+1+6+6 = 25 bytes

            // Verify structure setup
            result[0].Should().Be(0x1A);  // LDIX1 #imm16
            result[1].Should().Be(0x16);  // PLAYER_STRUCT address low (22)
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

            // Verify read operations
            result[15].Should().Be(0x90); // LDA (IDX1+offset)
            result[16].Should().Be(0);    // Offset 0
            result[17].Should().Be(0x91); // LDB (IDX1+offset)
            result[18].Should().Be(1);    // Offset 1
            result[19].Should().Be(0x90); // LDA (IDX1+offset)
            result[20].Should().Be(2);    // Offset 2

            result[21].Should().Be(0x01); // HALT
        }

        [Fact]
        public void IndexStringProcessingProgram_ShouldAssembleCorrectly()
        {
            // Arrange - String processing using index registers
            string[] lines = 
            {                                         // pos  size
                "STRING_PROCESS:",                    //  -    0   Label
                "LDIX1 #SOURCE_STRING",               //  0    3   Source string pointer
                "LDIY1 #DEST_STRING",                 //  3    3   Destination string pointer
                "",
                "PROCESS_LOOP:",                      //  -    0   Label (position 6)
                "LDAIX1+",                            //  6    1   Load char from source, increment
                "CMP A,#0",                           //  7    2   Check for null terminator
                "JZ PROCESS_DONE",                    //  9    3   Jump if end of string
                "",
                "; Convert lowercase to uppercase (simplified)",
                "CMP A,#97",                          // 12    2   Compare with 'a' (ASCII 97)
                "JC STORE_CHAR",                      // 14    3   Jump if less than 'a'
                "CMP A,#122",                         // 17    2   Compare with 'z' (ASCII 122)
                "JNC STORE_CHAR",                     // 19    3   Jump if greater than 'z'
                "SUB A,#32",                          // 22    2   Convert to uppercase (subtract 32)
                "",
                "STORE_CHAR:",                        //  -    0   Label (position 24)
                "STAIY1+",                            // 24    1   Store char to dest, increment
                "JMP PROCESS_LOOP",                   // 25    3   Continue processing
                "",
                "PROCESS_DONE:",                      //  -    0   Label (position 28)
                "LDA #0",                             // 28    2   Load null terminator
                "STA (IDY1)",                         // 30    1   Store null terminator
                "HALT",                               // 31    1   End program
                "",
                "SOURCE_STRING:",                     //  -    0   Label (position 32)
                "DB 'h', 'e', 'l', 'l', 'o', 0",     // 32    6   Source string: "hello"
                "DEST_STRING:",                       //  -    0   Label (position 38)
                "DB 0, 0, 0, 0, 0, 0"                 // 38    6   Destination buffer
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(44); // Total: 3+3+1+2+3+2+3+2+3+2+1+3+2+1+1+6+6 = 44 bytes

            // Verify setup
            result[0].Should().Be(0x1A);  // LDIX1 #imm16
            result[1].Should().Be(0x20);  // SOURCE_STRING address low (32)
            result[2].Should().Be(0x00);  // SOURCE_STRING address high

            result[3].Should().Be(0x1C);  // LDIY1 #imm16
            result[4].Should().Be(0x26);  // DEST_STRING address low (38)
            result[5].Should().Be(0x00);  // DEST_STRING address high

            // Verify main loop
            result[6].Should().Be(0xC4);  // LDAIX1+
            result[7].Should().Be(0x2C);  // CMP A,#imm (using existing CMP A,B where B=immediate)
            result[8].Should().Be(0);     // Immediate value (null)
            result[9].Should().Be(0x41);  // JZ
            result[10].Should().Be(0x1C); // PROCESS_DONE address low (28)
            result[11].Should().Be(0x00); // PROCESS_DONE address high

            // Verify character processing
            result[24].Should().Be(0xC7); // STAIY1+
            result[25].Should().Be(0x40); // JMP
            result[26].Should().Be(0x06); // PROCESS_LOOP address low (6)
            result[27].Should().Be(0x00); // PROCESS_LOOP address high

            // Verify source string
            result[32].Should().Be((byte)'h');
            result[33].Should().Be((byte)'e');
            result[34].Should().Be((byte)'l');
            result[35].Should().Be((byte)'l');
            result[36].Should().Be((byte)'o');
            result[37].Should().Be(0); // Null terminator
        }

        [Fact]
        public void IndexMultidimensionalArrayAccess_ShouldAssembleCorrectly()
        {
            // Arrange - 2D array access using index arithmetic
            string[] lines = 
            {                                         // pos  size
                "MATRIX_ACCESS:",                     //  -    0   Label
                "LDIX1 #MATRIX",                      //  0    3   Base address of 3x3 matrix
                "LDA #1",                             //  3    2   Row index (1)
                "LDB #2",                             //  5    2   Column index (2)
                "",
                "; Calculate offset: row * 3 + col",
                "LDC #3",                             //  7    2   Load row width (3)
                "MOV A,A",                            //  9    1   Ensure A contains row (redundant but clear)
                "",
                "; Multiply row by width (A * C)",
                "PUSH A",                             // 10    1   Save original row
                "LDA #0",                             // 11    2   Initialize result
                "MULT_LOOP:",                         //  -    0   Label (position 13)
                "POP A",                              // 13    1   Get current row value
                "CMP A,#0",                           // 14    2   Check if done
                "JZ MULT_DONE",                       // 16    3   Jump if multiplication complete
                "PUSH A",                             // 19    1   Save row value back
                "LDA TEMP_RESULT",                    // 20    3   Load current result
                "ADD A,C",                            // 23    1   Add width to result  
                "STA TEMP_RESULT",                    // 24    3   Store back
                "POP A",                              // 27    1   Get row value
                "DEC A",                              // 28    1   Decrement row counter
                "PUSH A",                             // 29    1   Save decremented value
                "JMP MULT_LOOP",                      // 30    3   Continue multiplication
                "",
                "MULT_DONE:",                         //  -    0   Label (position 33)
                "LDA TEMP_RESULT",                    // 33    3   Load multiplication result
                "ADD A,B",                            // 36    1   Add column index
                "ADDIX1 #0",                          // 37    3   Add offset to base (A in separate instruction)
                "; Note: This is simplified - real implementation would need offset in 16-bit",
                "",
                "LDA (IDX1)",                         // 40    1   Load matrix element
                "STA RESULT",                         // 41    3   Store result
                "HALT",                               // 44    1   End program
                "",
                "MATRIX:",                            //  -    0   Label (position 45)
                "DB 1, 2, 3",                         // 45    3   Row 0: [1, 2, 3]
                "DB 4, 5, 6",                         // 48    3   Row 1: [4, 5, 6]  
                "DB 7, 8, 9",                         // 51    3   Row 2: [7, 8, 9]
                "TEMP_RESULT:",                       //  -    0   Label (position 54)
                "DB 0",                               // 54    1   Temporary storage
                "RESULT:",                            //  -    0   Label (position 55)
                "DB 0"                                // 55    1   Final result
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(56); // Complex calculation

            // Verify matrix setup
            result[0].Should().Be(0x1A);  // LDIX1 #imm16
            result[1].Should().Be(0x2D);  // MATRIX address low (45)
            result[2].Should().Be(0x00);  // MATRIX address high

            // Verify matrix data (3x3 = 9 elements)
            result[45].Should().Be(1); result[46].Should().Be(2); result[47].Should().Be(3);
            result[48].Should().Be(4); result[49].Should().Be(5); result[50].Should().Be(6);
            result[51].Should().Be(7); result[52].Should().Be(8); result[53].Should().Be(9);

            // This test validates that complex index arithmetic can be assembled
            // The actual matrix access would retrieve matrix[1][2] = 6
        }

        [Fact]
        public void IndexRegisterTransferOperations_ShouldAssembleCorrectly()
        {
            // Arrange - Test all index register transfer and swap operations
            string[] lines = 
            {                                         // pos  size
                "INDEX_OPERATIONS:",                  //  -    0   Label
                "; Initialize all index registers",
                "LDIX1 #0x1111",                      //  0    3   Load IDX1
                "LDIX2 #0x2222",                      //  3    3   Load IDX2
                "LDIY1 #0x3333",                      //  6    3   Load IDY1
                "LDIY2 #0x4444",                      //  9    3   Load IDY2
                "",
                "; Test move operations",
                "MVIX1IX2",                           // 12    1   Move IDX1 to IDX2
                "MVIY1IY2",                           // 13    1   Move IDY1 to IDY2
                "",
                "; Test swap operations",
                "SWPIX1IY1",                          // 14    1   Swap IDX1 and IDY1
                "SWPIX1IX2",                          // 15    1   Swap IDX1 and IDX2
                "",
                "; Test arithmetic operations",
                "INCIX1",                             // 16    1   Increment IDX1
                "DECIY1",                             // 17    1   Decrement IDY1
                "ADDIX1 #0x1000",                     // 18    3   Add to IDX1
                "ADDIY2 #0x0001",                     // 21    3   Add to IDY2
                "",
                "HALT"                                // 24    1   End program
            };

            // Act
            byte[] result = _assembler.AssembleLines(lines);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(25); // Total: 3+3+3+3+1+1+1+1+1+1+3+3+1 = 25 bytes

            // Verify index register loads
            result[0].Should().Be(0x1A);   // LDIX1 #imm16
            result[1].Should().Be(0x11);   // Low byte
            result[2].Should().Be(0x11);   // High byte

            result[3].Should().Be(0x1B);   // LDIX2 #imm16
            result[4].Should().Be(0x22);   // Low byte
            result[5].Should().Be(0x22);   // High byte

            result[6].Should().Be(0x1C);   // LDIY1 #imm16
            result[7].Should().Be(0x33);   // Low byte
            result[8].Should().Be(0x33);   // High byte

            result[9].Should().Be(0x1D);   // LDIY2 #imm16
            result[10].Should().Be(0x44);  // Low byte
            result[11].Should().Be(0x44);  // High byte

            // Verify transfer operations
            result[12].Should().Be(0xF1);  // MVIX1IX2
            result[13].Should().Be(0xF3);  // MVIY1IY2

            // Verify swap operations
            result[14].Should().Be(0xF9);  // SWPIX1IY1
            result[15].Should().Be(0xF7);  // SWPIX1IX2

            // Verify arithmetic operations
            result[16].Should().Be(0xE0);  // INCIX1
            result[17].Should().Be(0xE3);  // DECIY1

            result[18].Should().Be(0xE8);  // ADDIX1 #imm16
            result[19].Should().Be(0x00);  // Low byte of 0x1000
            result[20].Should().Be(0x10);  // High byte of 0x1000

            result[21].Should().Be(0xEB);  // ADDIY2 #imm16
            result[22].Should().Be(0x01);  // Low byte of 0x0001
            result[23].Should().Be(0x00);  // High byte of 0x0001

            result[24].Should().Be(0x01);  // HALT
        }

        #endregion
    }
}