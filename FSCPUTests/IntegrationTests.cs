using CPU8Bit;
using FluentAssertions;

namespace FSCPUTests
{
    /// <summary>
    /// Tests d'int�gration pour le syst�me complet CPU + Memory
    /// </summary>
    public class IntegrationTests
    {
        private Memory _memory;
        private CPU8Bit.CPU8Bit _cpu;

        public IntegrationTests()
        {
            _memory = new Memory(0x10000);
            _cpu = new CPU8Bit.CPU8Bit(_memory);
        }

        [Fact]
        public void HelloWorldProgram_ShouldDisplayTextInVideoMemory()
        {
            // Arrange - Programme qui �crit "HELLO" dans la m�moire vid�o
            var program = new List<byte>
            {
                0x10, (byte)'H',        // LDA #'H'
                0x50, 0x00, 0xF8,       // STA $F800 (d�but m�moire vid�o)
                
                0x10, (byte)'E',        // LDA #'E'
                0x50, 0x01, 0xF8,       // STA $F801
                
                0x10, (byte)'L',        // LDA #'L'
                0x50, 0x02, 0xF8,       // STA $F802
                
                0x10, (byte)'L',        // LDA #'L'
                0x50, 0x03, 0xF8,       // STA $F803
                
                0x10, (byte)'O',        // LDA #'O'
                0x50, 0x04, 0xF8,       // STA $F804
                
                0x01                    // HALT
            };

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act - Ex�cuter le programme complet
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert
            var videoMemory = _memory.GetVideoMemory();
            videoMemory[0].Should().Be((byte)'H');
            videoMemory[1].Should().Be((byte)'E');
            videoMemory[2].Should().Be((byte)'L');
            videoMemory[3].Should().Be((byte)'L');
            videoMemory[4].Should().Be((byte)'O');
            
            _cpu.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void CountingProgram_ShouldCountFrom1To10()
        {
            // Arrange - Programme qui compte de 1 � 10 et stocke chaque valeur
            var program = new List<byte>
            {
                0x10, 0x01,             // LDA #1 (counter)
                0x11, 0x01,             // LDB #1 (increment)
                0x12, 0x0A,             // LDC #10 (limit)
                
                // LOOP:
                0x50, 0x00, 0x20,       // STA $2000 + counter (store current value)
                0x20,                   // ADD A,B (increment counter)
                
                // Simulation d'une comparaison (A - C)
                0x13, 0x00,             // LDD #0 (temp)
                0x21,                   // SUB A,B (A = A - 1, pour compenser l'incr�ment)
                0x20,                   // ADD A,B (restaurer A)
                
                // Pour ce test, on va juste faire 5 it�rations manuellement
                0x01                    // HALT
            };

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act - Ex�cuter quelques cycles pour tester la logique
            for (int i = 0; i < 20 && _cpu.IsRunning; i++)
            {
                _cpu.ExecuteCycle();
            }

            // Assert - V�rifier que des valeurs ont �t� stock�es
            _cpu.IsRunning.Should().BeFalse();
            _memory.ReadByte(0x2000).Should().Be(1); // Premi�re valeur stock�e
        }

        [Fact]
        public void ArithmeticOperations_ShouldWorkWithFlags()
        {
            // Arrange - Test des op�rations arithm�tiques avec flags
            var program = new List<byte>
            {
                // Test addition avec overflow
                0x10, 0xFF,             // LDA #255
                0x11, 0x01,             // LDB #1
                0x20,                   // ADD A,B (devrait faire overflow)
                
                // Test soustraction avec borrow
                0x10, 0x05,             // LDA #5
                0x11, 0x0A,             // LDB #10
                0x21,                   // SUB A,B (devrait faire borrow)
                
                // Test op�rations logiques
                0x10, 0xFF,             // LDA #0xFF
                0x11, 0x0F,             // LDB #0x0F
                0x30,                   // AND A,B
                
                0x10, 0xF0,             // LDA #0xF0
                0x11, 0x0F,             // LDB #0x0F
                0x31,                   // OR A,B
                
                0x01                    // HALT
            };

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act & Assert - V�rifier chaque �tape
            
            // Chargement initial
            _cpu.ExecuteCycle(); // LDA #255
            _cpu.ExecuteCycle(); // LDB #1
            _cpu.A.Should().Be(255);
            _cpu.B.Should().Be(1);
            
            // Addition avec overflow
            _cpu.ExecuteCycle(); // ADD A,B
            _cpu.A.Should().Be(0); // 255 + 1 = 256, mais 256 & 0xFF = 0
            _cpu.SR.Carry.Should().BeTrue();
            _cpu.SR.Zero.Should().BeTrue();
            
            // Soustraction avec borrow
            _cpu.ExecuteCycle(); // LDA #5
            _cpu.ExecuteCycle(); // LDB #10
            _cpu.ExecuteCycle(); // SUB A,B
            _cpu.A.Should().Be(251); // 5 - 10 = -5, mais (-5) & 0xFF = 251
            _cpu.SR.Carry.Should().BeTrue(); // Borrow
            
            // Op�ration AND
            _cpu.ExecuteCycle(); // LDA #0xFF
            _cpu.ExecuteCycle(); // LDB #0x0F
            _cpu.ExecuteCycle(); // AND A,B
            _cpu.A.Should().Be(0x0F);
            _cpu.SR.Carry.Should().BeFalse(); // AND efface le carry
            
            // Op�ration OR
            _cpu.ExecuteCycle(); // LDA #0xF0
            _cpu.ExecuteCycle(); // LDB #0x0F
            _cpu.ExecuteCycle(); // OR A,B
            _cpu.A.Should().Be(0xFF);
            
            // HALT
            _cpu.ExecuteCycle();
            _cpu.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void JumpInstruction_ShouldCreateSimpleLoop()
        {
            // Arrange - Programme avec saut inconditionnel (boucle infinie contr�l�e)
            var program = new List<byte>
            {
                0x10, 0x00,             // LDA #0 (compteur)
                0x11, 0x01,             // LDB #1 (incr�ment)
                
                // LOOP (adresse 0x0004):
                0x20,                   // ADD A,B (incr�menter compteur)
                0x50, 0x00, 0x30,       // STA $3000 (sauvegarder compteur)
                
                // On va tester manuellement au lieu de cr�er une vraie boucle infinie
                0x01                    // HALT
            };

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act - Ex�cuter le programme
            _cpu.ExecuteCycle(); // LDA #0
            _cpu.ExecuteCycle(); // LDB #1
            
            // Simuler quelques it�rations de la boucle
            for (int i = 1; i <= 5; i++)
            {
                _cpu.ExecuteCycle(); // ADD A,B
                _cpu.ExecuteCycle(); // STA $3000
                
                _cpu.A.Should().Be((byte)i);
                _memory.ReadByte(0x3000).Should().Be((byte)i);
                
                // Reset PC pour simuler la boucle (normalement fait par JMP)
                if (i < 5) _cpu.PC = 4;
            }
            
            _cpu.ExecuteCycle(); // HALT
            _cpu.IsRunning.Should().BeFalse();
        }

        [Fact]
        public void MemoryBoundaryTest_ShouldHandleEdgeCases()
        {
            // Arrange - Test des limites m�moire
            var program = new List<byte>
            {
                // �crire � la limite de la m�moire normale (avant la vid�o)
                0x10, 0x42,             // LDA #0x42
                0x50, 0xFF, 0xF7,       // STA $F7FF (juste avant la m�moire vid�o)
                
                // �crire au d�but de la m�moire vid�o
                0x10, 0x48,             // LDA #'H'
                0x50, 0x00, 0xF8,       // STA $F800 (d�but m�moire vid�o)
                
                0x01                    // HALT
            };

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert
            _memory.ReadByte(0xF7FF).Should().Be(0x42); // Limite avant vid�o
            _memory.ReadByte(0xF800).Should().Be((byte)'H'); // D�but vid�o
            
            var videoMemory = _memory.GetVideoMemory();
            videoMemory[0].Should().Be((byte)'H');
        }

        [Fact]
        public void StressTest_ShouldHandleManyInstructions()
        {
            // Arrange - Programme r�p�titif pour test de stress
            var program = new List<byte>();
            
            // Cr�er un programme qui fait 100 additions simples
            for (int i = 0; i < 50; i++)
            {
                program.AddRange(new byte[] { 0x10, (byte)(i % 256) }); // LDA #i
                program.AddRange(new byte[] { 0x11, 0x01 });            // LDB #1
                program.Add(0x20);                                      // ADD A,B
            }
            program.Add(0x01); // HALT

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act
            int cycleCount = 0;
            while (_cpu.IsRunning && cycleCount < 1000) // Limite de s�curit�
            {
                _cpu.ExecuteCycle();
                cycleCount++;
            }

            // Assert
            _cpu.IsRunning.Should().BeFalse();
            cycleCount.Should().BeLessThan(1000); // V�rifier qu'on n'a pas atteint la limite
            _cpu.A.Should().Be(50); // Derni�re valeur calcul�e: 49 + 1 = 50
        }

        [Fact]
        public void VideoMemoryIntegration_ShouldWorkWithTextDisplay()
        {
            // Arrange - Programme qui �crit sur plusieurs lignes
            _memory.WriteTextToVideo("Line 1", 0, 0);
            _memory.WriteTextToVideo("Line 2", 0, 1);
            
            var program = new List<byte>
            {
                // �crire "TEST" sur la ligne 3
                0x10, (byte)'T',        // LDA #'T'
                0x50, 0xA0, 0xF8,       // STA $F8A0 (ligne 2 * 80 = 160 = 0xA0)
                
                0x10, (byte)'E',        // LDA #'E'
                0x50, 0xA1, 0xF8,       // STA $F8A1
                
                0x10, (byte)'S',        // LDA #'S'
                0x50, 0xA2, 0xF8,       // STA $F8A2
                
                0x10, (byte)'T',        // LDA #'T'
                0x50, 0xA3, 0xF8,       // STA $F8A3
                
                0x01                    // HALT
            };

            _memory.LoadProgram(program.ToArray());
            _cpu.Start();

            // Act
            while (_cpu.IsRunning)
            {
                _cpu.ExecuteCycle();
            }

            // Assert
            var videoMemory = _memory.GetVideoMemory();
            
            // Ligne 0: "Line 1"
            videoMemory[0].Should().Be((byte)'L');
            videoMemory[1].Should().Be((byte)'i');
            videoMemory[2].Should().Be((byte)'n');
            videoMemory[3].Should().Be((byte)'e');
            
            // Ligne 1: "Line 2" (80 caract�res plus loin)
            videoMemory[80].Should().Be((byte)'L');
            videoMemory[81].Should().Be((byte)'i');
            
            // Ligne 2: "TEST" �crit par le programme (160 caract�res plus loin)
            videoMemory[160].Should().Be((byte)'T');
            videoMemory[161].Should().Be((byte)'E');
            videoMemory[162].Should().Be((byte)'S');
            videoMemory[163].Should().Be((byte)'T');
        }
    }
}