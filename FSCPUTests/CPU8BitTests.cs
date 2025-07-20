using CPU8Bit;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Tests unitaires pour la classe CPU8Bit principale
    /// </summary>
    public class CPU8BitTests
    {
        private Memory _memory;
        private CPU8Bit.CPU8Bit _cpu;

        public CPU8BitTests()
        {
            _memory = new Memory(0x10000);
            _cpu = new CPU8Bit.CPU8Bit(_memory);
        }

        [Fact]
        public void CPU_ShouldInitializeCorrectly()
        {
            // Assert
            _cpu.A.Should().Be(0);
            _cpu.B.Should().Be(0);
            _cpu.C.Should().Be(0);
            _cpu.D.Should().Be(0);
            _cpu.PC.Should().Be(RomManager.ROM_BOOT_START); // Démarre en ROM maintenant
            _cpu.SP.Should().Be(0xFFFF);
            _cpu.IsRunning.Should().BeFalse();
            _cpu.Memory.Should().Be(_memory);
            _cpu.ALU.Should().NotBeNull();
            _cpu.SR.Should().NotBeNull();
        }

        [Fact]
        public void Reset_ShouldResetAllRegisters()
        {
            // Arrange
            _cpu.A = 10;
            _cpu.B = 20;
            _cpu.C = 30;
            _cpu.D = 40;
            _cpu.PC = 0x1000;
            _cpu.SP = 0x2000;
            _cpu.Start(false);

            // Act
            _cpu.Reset();

            // Assert
            _cpu.A.Should().Be(0);
            _cpu.B.Should().Be(0);
            _cpu.C.Should().Be(0);
            _cpu.D.Should().Be(0);
            _cpu.PC.Should().Be(RomManager.ROM_BOOT_START); // Démarre en ROM maintenant
            _cpu.SP.Should().Be(0xFFFF);
            _cpu.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void StartStop_ShouldControlExecution()
        {
            // Act & Assert
            _cpu.IsRunning.Should().BeFalse();

            _cpu.Start(false);
            _cpu.IsRunning.Should().BeTrue();

            _cpu.Stop();
            _cpu.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void GetRegister_ShouldReturnCorrectValues()
        {
            // Arrange
            _cpu.A = 10;
            _cpu.B = 20;
            _cpu.C = 30;
            _cpu.D = 40;

            // Act & Assert
            _cpu.GetRegister('A').Should().Be(10);
            _cpu.GetRegister('B').Should().Be(20);
            _cpu.GetRegister('C').Should().Be(30);
            _cpu.GetRegister('D').Should().Be(40);
        }

        [Fact]
        public void GetRegister_ShouldThrowForInvalidRegister()
        {
            // Act & Assert
            _cpu.Invoking(c => c.GetRegister('X'))
                .Should().Throw<ArgumentException>()
                .WithMessage("*Invalid register: X*");
        }

        [Fact]
        public void SetRegister_ShouldSetCorrectValues()
        {
            // Act
            _cpu.SetRegister('A', 100);
            _cpu.SetRegister('B', 200);
            _cpu.SetRegister('C', 50);
            _cpu.SetRegister('D', 75);

            // Assert
            _cpu.A.Should().Be(100);
            _cpu.B.Should().Be(200);
            _cpu.C.Should().Be(50);
            _cpu.D.Should().Be(75);
        }

        [Fact]
        public void SetRegister_ShouldUpdateZeroFlag()
        {
            // Act
            _cpu.SetRegister('A', 0);

            // Assert
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Fact]
        public void ExecuteCycle_ShouldNotExecuteWhenStopped()
        {
            // Arrange - Placer le programme en RAM et forcer PC à 0x0000 pour ce test
            _memory.WriteByte(0x0000, 0x10); // LDA
            _memory.WriteByte(0x0001, 0x42); // #66
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Stop();

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0); // PC ne devrait pas avoir bougé
            _cpu.A.Should().Be(0);  // A ne devrait pas avoir changé
        }

        [Fact]
        public void NOP_Instruction_ShouldDoNothing()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0x00); // NOP
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);
            ushort initialPC = _cpu.PC;

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be((ushort)(initialPC + 1));
            _cpu.A.Should().Be(0);
            _cpu.B.Should().Be(0);
            _cpu.C.Should().Be(0);
            _cpu.D.Should().Be(0);
            _cpu.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void HALT_Instruction_ShouldStopCPU()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0x01); // HALT
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.IsRunning.Should().BeFalse();
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void LDA_Instruction_ShouldLoadImmediateValue()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0x10); // LDA
            _memory.WriteByte(0x0001, 0x42); // #66
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x42);
            _cpu.PC.Should().Be(2);
            _cpu.SR.Zero.Should().BeFalse();
        }

        [Fact]
        public void LDA_Instruction_ShouldSetZeroFlag()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0x10); // LDA
            _memory.WriteByte(0x0001, 0x00); // #0
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0);
            _cpu.SR.Zero.Should().BeTrue();
        }

        [Theory]
        [InlineData(0x11, 'B')] // LDB
        [InlineData(0x12, 'C')] // LDC
        [InlineData(0x13, 'D')] // LDD
        public void LoadInstructions_ShouldLoadIntoCorrectRegister(byte opcode, char expectedRegister)
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, opcode);
            _memory.WriteByte(0x0001, 0x99);
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.GetRegister(expectedRegister).Should().Be(0x99);
            _cpu.PC.Should().Be(2);
        }

        [Fact]
        public void ADD_Instruction_ShouldAddBToA()
        {
            // Arrange - Placer le programme en RAM
            _cpu.A = 10;
            _cpu.B = 5;
            _memory.WriteByte(0x0000, 0x20); // ADD A,B
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(15);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void SUB_Instruction_ShouldSubtractBFromA()
        {
            // Arrange - Placer le programme en RAM
            _cpu.A = 15;
            _cpu.B = 5;
            _memory.WriteByte(0x0000, 0x21); // SUB A,B
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(10);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void AND_Instruction_ShouldPerformLogicalAnd()
        {
            // Arrange - Placer le programme en RAM
            _cpu.A = 0xFF;
            _cpu.B = 0x0F;
            _memory.WriteByte(0x0000, 0x30); // AND A,B
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0x0F);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void OR_Instruction_ShouldPerformLogicalOr()
        {
            // Arrange - Placer le programme en RAM
            _cpu.A = 0xF0;
            _cpu.B = 0x0F;
            _memory.WriteByte(0x0000, 0x31); // OR A,B
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.A.Should().Be(0xFF);
            _cpu.PC.Should().Be(1);
        }

        [Fact]
        public void JMP_Instruction_ShouldJumpToAddress()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0x40); // JMP
            _memory.WriteByte(0x0001, 0x00); // Low byte de 0x1000
            _memory.WriteByte(0x0002, 0x10); // High byte de 0x1000
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x1000);
        }

        [Fact]
        public void STA_Instruction_ShouldStoreAAtAddress()
        {
            // Arrange - Placer le programme en RAM
            _cpu.A = 0x42;
            _memory.WriteByte(0x0000, 0x50); // STA
            _memory.WriteByte(0x0001, 0x00); // Low byte de 0x2000
            _memory.WriteByte(0x0002, 0x20); // High byte de 0x2000
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _memory.ReadByte(0x2000).Should().Be(0x42);
            _cpu.PC.Should().Be(3);
        }

        [Fact]
        public void CALL_Instruction_ShouldCallSubroutine()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0x60); // CALL
            _memory.WriteByte(0x0001, 0x00); // Low byte de 0x2000
            _memory.WriteByte(0x0002, 0x20); // High byte de 0x2000
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);
            ushort initialSP = _cpu.SP;

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(0x2000);         // PC saute à l'adresse
            _cpu.SP.Should().Be((ushort)(initialSP - 2)); // SP décrémenté de 2
            
            // Vérifier que l'adresse de retour est sur la pile
            // Stack order: [low byte at SP][high byte at SP+1]
            byte returnLow = _memory.ReadByte(_cpu.SP);
            byte returnHigh = _memory.ReadByte((ushort)(_cpu.SP + 1));
            ushort returnAddress = (ushort)(returnLow | (returnHigh << 8));
            returnAddress.Should().Be(3); // PC était à 3 après lecture de l'instruction CALL
        }

        [Fact]
        public void RET_Instruction_ShouldReturnFromSubroutine()
        {
            // Arrange - Simuler un appel de sous-routine en plaçant une adresse de retour sur la pile
            ushort returnAddress = 0x1234;
            _cpu.SP = (ushort)(_cpu.SP - 2);
            _memory.WriteByte(_cpu.SP, (byte)(returnAddress & 0xFF));        // Low byte
            _memory.WriteByte((ushort)(_cpu.SP + 1), (byte)(returnAddress >> 8)); // High byte
            
            _memory.WriteByte(0x0000, 0x61); // RET
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act
            _cpu.ExecuteCycle();

            // Assert
            _cpu.PC.Should().Be(returnAddress);
            _cpu.SP.Should().Be(0xFFFF); // SP restauré
        }

        [Fact]
        public void ExecuteInstruction_ShouldThrowForUnknownOpcode()
        {
            // Arrange - Placer le programme en RAM
            _memory.WriteByte(0x0000, 0xFD); // Opcode invalide
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act & Assert
            _cpu.Invoking(c => c.ExecuteCycle())
                .Should().Throw<InvalidOperationException>()
                .WithMessage("*Unknown instruction: 0xFD*");
        }

        [Fact]
        public void ComplexProgram_ShouldExecuteCorrectly()
        {
            // Arrange - Programme qui charge A=10, B=5, additionne, et stocke le résultat
            _memory.WriteByte(0x0000, 0x10); // LDA #10
            _memory.WriteByte(0x0001, 0x0A);
            _memory.WriteByte(0x0002, 0x11); // LDB #5
            _memory.WriteByte(0x0003, 0x05);
            _memory.WriteByte(0x0004, 0x20); // ADD A,B
            _memory.WriteByte(0x0005, 0x50); // STA $1000
            _memory.WriteByte(0x0006, 0x00);
            _memory.WriteByte(0x0007, 0x10);
            _memory.WriteByte(0x0008, 0x01); // HALT
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act - Exécuter le programme complet
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert
            _cpu.A.Should().Be(15);              // A = 10 + 5
            _cpu.B.Should().Be(5);               // B inchangé
            _memory.ReadByte(0x1000).Should().Be(15); // Résultat stocké en mémoire
            _cpu.PC.Should().Be(9);              // PC après HALT
            _cpu.IsRunning.Should().BeFalse();   // CPU arrêté
        }

        [Fact]
        public void ProgramWithLoop_ShouldExecuteCorrectly()
        {
            // Arrange - Programme avec boucle simple
            // Charge A avec 5, décrémente jusqu'à 0
            _memory.WriteByte(0x0000, 0x10); // LDA #5
            _memory.WriteByte(0x0001, 0x05);
            _memory.WriteByte(0x0002, 0x11); // LOOP: LDB #1
            _memory.WriteByte(0x0003, 0x01);
            _memory.WriteByte(0x0004, 0x21); // SUB A,B (A = A - 1)
            _memory.WriteByte(0x0005, 0x40); // JMP LOOP (si on avait une instruction de saut conditionnel)
            _memory.WriteByte(0x0006, 0x02);
            _memory.WriteByte(0x0007, 0x00);
            _cpu.PC = 0x0000; // Forcer PC pour ce test
            _cpu.Start(false);

            // Act - Exécuter quelques cycles manuellement
            _cpu.ExecuteCycle(); // LDA #5
            _cpu.ExecuteCycle(); // LDB #1
            _cpu.ExecuteCycle(); // SUB A,B (A devient 4)

            // Assert
            _cpu.A.Should().Be(4);
            _cpu.B.Should().Be(1);
            _cpu.PC.Should().Be(5);
        }

        [Fact]
        public void RegisterProperties_ShouldWorkCorrectly()
        {
            // Act & Assert
            _cpu.A = 100;
            _cpu.A.Should().Be(100);

            _cpu.B = 200;
            _cpu.B.Should().Be(200);

            _cpu.C = 50;
            _cpu.C.Should().Be(50);

            _cpu.D = 75;
            _cpu.D.Should().Be(75);

            _cpu.PC = 0x1234;
            _cpu.PC.Should().Be(0x1234);

            _cpu.SP = 0x5678;
            _cpu.SP.Should().Be(0x5678);
        }
    }
}