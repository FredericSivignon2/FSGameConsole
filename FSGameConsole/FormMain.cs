using FSCPU;
using FSCPU.Graphics;

namespace FSGameConsole
{
    public partial class FormMain : Form
    {
        private FSCPU.CPU8Bit? _cpu;
        private Memory? _memory;
        
        // Nouveau système bitmap CPC authentique
        private VideoController? _videoController;
        private BitmapRenderer? _bitmapRenderer;
        
        // PLUS de cpuTimer ! Le timing est maintenant géré par ClockManager
        
        public FormMain()
        {
            InitializeComponent();
            InitializeEmulator();
        }
        
        private void InitializeEmulator()
        {
            // Créer la mémoire et le CPU
            _memory = new Memory(0x10000); // 64KB
            _cpu = new FSCPU.CPU8Bit(_memory);
            
            // Initialiser le nouveau système bitmap CPC authentique
            _videoController = new VideoController(_memory);
            _bitmapRenderer = new BitmapRenderer(_videoController);
            
            // Configurer le gestionnaire d'appels système
            _cpu.SystemCalls = new SystemCallManager(_memory, _videoController);
            
            // Effectuer un démarrage à froid (boot depuis la ROM avec timing authentique)
            _cpu.ColdBoot();
            
            // PLUS BESOIN d'exécuter manuellement la ROM ! 
            // Le ClockManager s'en charge automatiquement avec le timing réaliste
            
            UpdateDisplay();
        }
        
        private void BtnStart_Click(object? sender, EventArgs e)
        {
            StartCPU();
        }
        
        private void BtnStop_Click(object? sender, EventArgs e)
        {
            if (_cpu != null)
            {
                _cpu.Stop(); // Arrête automatiquement le ClockManager
                lblCpuStatus.Text = "État CPU: Arrêté";
                btnStart.Enabled = true;
                btnStop.Enabled = false;
            }
        }
        
        private void BtnReset_Click(object? sender, EventArgs e)
        {
            if (_cpu != null && _memory != null && _videoController != null)
            {
                _cpu.Stop();
                
                // Effectuer un redémarrage à chaud (warm boot) avec timing authentique
                _cpu.WarmBoot();
                
                lblCpuStatus.Text = "État CPU: Redémarrage ROM (timing réaliste)";
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                
                UpdateDisplay();
            }
        }
        
        private void BtnColdBoot_Click(object? sender, EventArgs e)
        {
            if (_cpu != null && _memory != null && _videoController != null)
            {
                _cpu.Stop();
                
                // Effectuer un démarrage à froid complet avec timing authentique
                _cpu.ColdBoot();
                
                lblCpuStatus.Text = "État CPU: Cold Boot ROM (timing réaliste)";
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                
                UpdateDisplay();
            }
        }
        
        private void BtnLoadDemo_Click(object? sender, EventArgs e)
        {
            if (_memory != null && _cpu != null && _videoController != null)
            {
                // Programme de démonstration utilisant les nouveaux appels système
                byte[] demoProgram = {
                    // Démonstration des appels système pour affichage
                    
                    // Effacer l'écran en bleu foncé
                    0x10, 0x03,        // LDA #3 (SYSCALL_CLEAR_SCREEN)
                    0x11, 0x01,        // LDB #1 (bleu foncé)
                    0xF0,              // SYS
                    
                    // Couleur jaune sur bleu
                    0x10, 0x05,        // LDA #5 (SYSCALL_SET_COLOR)
                    0x11, 0x0E,        // LDB #14 (jaune)
                    0x12, 0x01,        // LDC #1 (bleu)
                    0xF0,              // SYS
                    
                    // Positionner en (5,5)
                    0x10, 0x04,        // LDA #4 (SYSCALL_SET_CURSOR)
                    0x11, 0x05,        // LDB #5 (X=5)
                    0x12, 0x05,        // LDC #5 (Y=5)
                    0xF0,              // SYS
                    
                    // Afficher "Hello via SYSCALLS!"
                    0x10, 0x01, 0x11, 0x48, 0xF0,  // 'H'
                    0x10, 0x01, 0x11, 0x65, 0xF0,  // 'e'
                    0x10, 0x01, 0x11, 0x6C, 0xF0,  // 'l'
                    0x10, 0x01, 0x11, 0x6C, 0xF0,  // 'l'
                    0x10, 0x01, 0x11, 0x6F, 0xF0,  // 'o'
                    0x10, 0x01, 0x11, 0x20, 0xF0,  // ' '
                    0x10, 0x01, 0x11, 0x76, 0xF0,  // 'v'
                    0x10, 0x01, 0x11, 0x69, 0xF0,  // 'i'
                    0x10, 0x01, 0x11, 0x61, 0xF0,  // 'a'
                    0x10, 0x01, 0x11, 0x20, 0xF0,  // ' '
                    0x10, 0x01, 0x11, 0x53, 0xF0,  // 'S'
                    0x10, 0x01, 0x11, 0x59, 0xF0,  // 'Y'
                    0x10, 0x01, 0x11, 0x53, 0xF0,  // 'S'
                    0x10, 0x01, 0x11, 0x43, 0xF0,  // 'C'
                    0x10, 0x01, 0x11, 0x41, 0xF0,  // 'A'
                    0x10, 0x01, 0x11, 0x4C, 0xF0,  // 'L'
                    0x10, 0x01, 0x11, 0x4C, 0xF0,  // 'L'
                    0x10, 0x01, 0x11, 0x53, 0xF0,  // 'S'
                    0x10, 0x01, 0x11, 0x21, 0xF0,  // '!'
                    
                    0x01               // HALT
                };
                
                // Arrêter le CPU
                _cpu.Stop();
                
                // Charger le programme en RAM (ne touche pas à la ROM)
                _memory.LoadProgram(demoProgram);
                
                // Reset du CPU pour redémarrer en 0x0000 (RAM)
                _cpu.Reset();
                _cpu.PC = 0x0000; // Forcer le démarrage en RAM pour ce demo
                
                StartCPU();
                UpdateDisplay();
            }
        }
        
        /// <summary>
        /// Gestionnaire du bouton "Charger .bin" - charge un fichier binaire compilé et lance le programme
        /// </summary>
        private void BtnLoadBin_Click(object? sender, EventArgs e)
        {
            if (_memory == null || _cpu == null || _videoController == null) return;

            // Configurer le dialogue de sélection de fichier
            using var openFileDialog = new OpenFileDialog
            {
                Title = "Charger un fichier binaire (.bin)",
                Filter = "Fichiers binaires (*.bin)|*.bin|Tous les fichiers (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
                RestoreDirectory = true
            };

            // Afficher le dialogue et traiter la sélection
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Arrêter le CPU s'il est en cours d'exécution
                    if (_cpu.IsRunning)
                    {
                        _cpu.Stop();
                    }

                    // Lire le fichier binaire
                    byte[] binaryProgram = File.ReadAllBytes(openFileDialog.FileName);
                    
                    // Vérifier la taille du programme (32KB max pour ne pas toucher la mémoire bitmap)
                    if (binaryProgram.Length > 0x8000)
                    {
                        MessageBox.Show(
                            $"Le fichier est trop volumineux ({binaryProgram.Length} octets).\n" +
                            $"Taille maximale supportée: {0x8000} octets.",
                            "Erreur de chargement",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }

                    // Charger le programme en RAM
                    _memory.LoadProgram(binaryProgram);
                    
                    // Reset du CPU pour redémarrer en 0x0000 (RAM)
                    _cpu.Reset();
                    _cpu.PC = 0x0000; // Forcer le démarrage en RAM
                    
                    // Mettre à jour l'affichage
                    UpdateDisplay();
                    
                    // Démarrer automatiquement l'exécution du programme avec timing réaliste
                    StartCPU();
                    
                    // Afficher un message de succès dans la barre de statut
                    string fileName = Path.GetFileName(openFileDialog.FileName);
                    lblCpuStatus.Text = $"Programme {fileName} - Timing réaliste actif";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors du chargement:\n{ex.Message}",
                        "Erreur de chargement",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        
        private void BtnStep_Click(object? sender, EventArgs e)
        {
            if (_cpu != null && !_cpu.IsRunning)
            {
                // Mode pas à pas : exécuter une seule instruction
                _cpu.Start();
                _cpu.ExecuteCycle(); // Version legacy pour le debug
                _cpu.Stop();
                UpdateDisplay();
                
                // Synchroniser l'affichage
                _videoController?.SyncFromMemory();
            }
        }
        
        // PLUS de CpuTimer_Tick ! Le timing est géré par ClockManager
        
        private void DisplayTimer_Tick(object? sender, EventArgs e)
        {
            // Redessiner l'écran avec le nouveau système bitmap CPC authentique
            RenderBitmapScreen();
            
            // Mettre à jour les métriques de performance en temps réel
            UpdatePerformanceDisplay();
        }
        
        private void StartCPU()
        {
            if (_cpu != null)
            {
                _cpu.Start(); // Démarre automatiquement le ClockManager
                lblCpuStatus.Text = "État CPU: Timing réaliste actif";
                btnStart.Enabled = false;
                btnStop.Enabled = true;
            }
        }

        private void UpdateDisplay()
        {
            UpdateRegistersDisplay();
            UpdateMemoryDisplay();
        }
        
        private void UpdateRegistersDisplay()
        {
            if (_cpu != null)
            {
                string romIndicator = RomManager.IsRomAddress(_cpu.PC) ? " (ROM)" : " (RAM)";
                var (cps, ips) = _cpu.GetPerformanceMetrics();
                
                txtRegisters.Text = $"A  = 0x{_cpu.A:X2} ({_cpu.A})\r\n" +
                                   $"B  = 0x{_cpu.B:X2} ({_cpu.B})\r\n" +
                                   $"C  = 0x{_cpu.C:X2} ({_cpu.C})\r\n" +
                                   $"D  = 0x{_cpu.D:X2} ({_cpu.D})\r\n" +
                                   $"PC = 0x{_cpu.PC:X4} ({_cpu.PC}){romIndicator}\r\n" +
                                   $"SP = 0x{_cpu.SP:X4} ({_cpu.SP})\r\n" +
                                   $"SR = {_cpu.SR}\r\n" +
                                   $"CPS = {cps:F0} Hz\r\n" +
                                   $"IPS = {ips:F0} Hz";
            }
        }
        
        private void UpdateMemoryDisplay()
        {
            if (_memory != null)
            {
                // Afficher les premières lignes de mémoire programme ET un aperçu de la mémoire bitmap
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("=== RAM (0x0000-0x007F) ===");
                sb.Append(_memory.GetMemoryDump(0, 128));
                sb.AppendLine("\n=== MÉMOIRE BITMAP (0x8000-0x803F) ===");
                sb.Append(_memory.GetMemoryDump(0x8000, 64));
                sb.AppendLine("\n=== ROM BOOT (0xF400-0xF41F) ===");
                sb.Append(_memory.GetMemoryDump(0xF400, 32));
                
                txtMemory.Text = sb.ToString();
            }
        }
        
        /// <summary>
        /// Mise à jour des métriques de performance en temps réel
        /// </summary>
        private void UpdatePerformanceDisplay()
        {
            if (_cpu != null && _cpu.IsRunning)
            {
                var (cps, ips) = _cpu.GetPerformanceMetrics();
                int remainingCycles = _cpu.Clock.RemainingCycles;
                
                lblCpuStatus.Text = $"CPU: {ips:F0} IPS, {cps:F0} CPS, Cycles: {remainingCycles}";
            }
        }
        
        /// <summary>
        /// Nouveau système de rendu bitmap CPC authentique
        /// </summary>
        private void RenderBitmapScreen()
        {
            if (_bitmapRenderer == null || screenPanel == null) return;
            
            try
            {
                using (var graphics = screenPanel.CreateGraphics())
                {
                    // Calculer le rectangle de destination pour maintenir les proportions
                    var destRect = new Rectangle(0, 0, screenPanel.Width, screenPanel.Height);
                    
                    // Rendre directement sur le panel avec le nouveau système CPC authentique
                    _bitmapRenderer.RenderToGraphics(graphics, destRect);
                }
            }
            catch (Exception ex)
            {
                // En cas d'erreur, afficher un message de debug
                using (var graphics = screenPanel.CreateGraphics())
                {
                    graphics.Clear(Color.Black);
                    using (var font = new Font("Consolas", 10))
                    using (var brush = new SolidBrush(Color.Red))
                    {
                        graphics.DrawString($"Erreur rendu: {ex.Message}", font, brush, 10, 10);
                    }
                }
            }
        }
        
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // Nettoyer les ressources
            _cpu?.Stop(); // Arrête automatiquement le ClockManager
            displayTimer?.Stop();
            _bitmapRenderer?.Dispose();
            _cpu?.Dispose(); // Nettoie le ClockManager
            
            base.OnFormClosed(e);
        }
    }
}
