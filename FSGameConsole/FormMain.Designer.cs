namespace FSGameConsole
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            
            // Form principale
            this.SuspendLayout();
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(1000, 700);
            this.Text = "FSGameConsole - Émulateur Authentique (Timing Réaliste 1MHz)";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            // Panel principal pour l'écran (ratio 320x200 = 8:5)
            this.screenPanel = new Panel();
            this.screenPanel.Location = new Point(12, 12);
            this.screenPanel.Size = new Size(640, 400); // 320x200 x2
            this.screenPanel.BackColor = Color.Black;
            this.screenPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(this.screenPanel);
            
            // Panel pour les contrôles
            this.controlPanel = new Panel();
            this.controlPanel.Location = new Point(670, 12);
            this.controlPanel.Size = new Size(300, 650);
            this.controlPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(this.controlPanel);
            
            // Labels et contrôles CPU
            this.lblCpuStatus = new Label();
            this.lblCpuStatus.Text = "État CPU: Timing Réaliste 1MHz";
            this.lblCpuStatus.Location = new Point(10, 10);
            this.lblCpuStatus.Size = new Size(280, 20);
            this.controlPanel.Controls.Add(this.lblCpuStatus);
            
            // Boutons de contrôle
            this.btnStart = new Button();
            this.btnStart.Text = "Démarrer";
            this.btnStart.Location = new Point(10, 40);
            this.btnStart.Size = new Size(65, 30);
            this.btnStart.Click += BtnStart_Click;
            this.controlPanel.Controls.Add(this.btnStart);
            
            this.btnStop = new Button();
            this.btnStop.Text = "Arrêter";
            this.btnStop.Location = new Point(85, 40);
            this.btnStop.Size = new Size(65, 30);
            this.btnStop.Click += BtnStop_Click;
            this.controlPanel.Controls.Add(this.btnStop);
            
            this.btnReset = new Button();
            this.btnReset.Text = "Reset";
            this.btnReset.Location = new Point(160, 40);
            this.btnReset.Size = new Size(60, 30);
            this.btnReset.Click += BtnReset_Click;
            this.controlPanel.Controls.Add(this.btnReset);
            
            this.btnColdBoot = new Button();
            this.btnColdBoot.Text = "Cold Boot";
            this.btnColdBoot.Location = new Point(230, 40);
            this.btnColdBoot.Size = new Size(65, 30);
            this.btnColdBoot.Click += BtnColdBoot_Click;
            this.controlPanel.Controls.Add(this.btnColdBoot);
            
            // Affichage des registres
            this.lblRegisters = new Label();
            this.lblRegisters.Text = "=== REGISTRES + PERF ===";
            this.lblRegisters.Location = new Point(10, 80);
            this.lblRegisters.Size = new Size(200, 20);
            this.lblRegisters.Font = new Font("Consolas", 9, FontStyle.Bold);
            this.controlPanel.Controls.Add(this.lblRegisters);
            
            this.txtRegisters = new TextBox();
            this.txtRegisters.Location = new Point(10, 105);
            this.txtRegisters.Size = new Size(270, 140);
            this.txtRegisters.Multiline = true;
            this.txtRegisters.ReadOnly = true;
            this.txtRegisters.Font = new Font("Consolas", 9);
            this.txtRegisters.ScrollBars = ScrollBars.Vertical;
            this.controlPanel.Controls.Add(this.txtRegisters);
            
            // Zone mémoire
            this.lblMemory = new Label();
            this.lblMemory.Text = "=== MÉMOIRE (RAM + BITMAP + ROM) ===";
            this.lblMemory.Location = new Point(10, 255);
            this.lblMemory.Size = new Size(270, 20);
            this.lblMemory.Font = new Font("Consolas", 9, FontStyle.Bold);
            this.controlPanel.Controls.Add(this.lblMemory);
            
            this.txtMemory = new TextBox();
            this.txtMemory.Location = new Point(10, 280);
            this.txtMemory.Size = new Size(270, 100);
            this.txtMemory.Multiline = true;
            this.txtMemory.ReadOnly = true;
            this.txtMemory.Font = new Font("Consolas", 8);
            this.txtMemory.ScrollBars = ScrollBars.Vertical;
            this.controlPanel.Controls.Add(this.txtMemory);
            
            // Zone de chargement de programme
            this.lblProgram = new Label();
            this.lblProgram.Text = "=== PROGRAMME ===";
            this.lblProgram.Location = new Point(10, 390);
            this.lblProgram.Size = new Size(200, 20);
            this.lblProgram.Font = new Font("Consolas", 9, FontStyle.Bold);
            this.controlPanel.Controls.Add(this.lblProgram);
            
            this.btnLoadDemo = new Button();
            this.btnLoadDemo.Text = "Démo Syscalls";
            this.btnLoadDemo.Location = new Point(10, 415);
            this.btnLoadDemo.Size = new Size(120, 30);
            this.btnLoadDemo.Click += BtnLoadDemo_Click;
            this.controlPanel.Controls.Add(this.btnLoadDemo);
            
            // Nouveau bouton pour charger un fichier .bin
            this.btnLoadBin = new Button();
            this.btnLoadBin.Text = "Charger .bin";
            this.btnLoadBin.Location = new Point(140, 415);
            this.btnLoadBin.Size = new Size(100, 30);
            this.btnLoadBin.Click += BtnLoadBin_Click;
            this.controlPanel.Controls.Add(this.btnLoadBin);
            
            this.btnStep = new Button();
            this.btnStep.Text = "Pas à Pas";
            this.btnStep.Location = new Point(10, 455);
            this.btnStep.Size = new Size(100, 30);
            this.btnStep.Click += BtnStep_Click;
            this.controlPanel.Controls.Add(this.btnStep);
            
            // Informations authentiques
            this.lblBitmapInfo = new Label();
            this.lblBitmapInfo.Text = "Timing: 1MHz authentique\nCycles/instruction variables\nSyscalls assembleur purs\nROM 100% authentique\nPAS de code C# direct!";
            this.lblBitmapInfo.Location = new Point(10, 495);
            this.lblBitmapInfo.Size = new Size(270, 80);
            this.lblBitmapInfo.Font = new Font("Consolas", 8);
            this.lblBitmapInfo.ForeColor = Color.DarkGreen;
            this.controlPanel.Controls.Add(this.lblBitmapInfo);
            
            // PLUS de cpuTimer ! Le timing est géré par ClockManager
            
            // Timer pour l'affichage bitmap seulement (plus rapide pour une meilleure fluidité)
            this.displayTimer = new System.Windows.Forms.Timer(this.components);
            this.displayTimer.Interval = 16; // ~60 FPS
            this.displayTimer.Tick += DisplayTimer_Tick;
            this.displayTimer.Start(); // Démarrer immédiatement
            
            this.ResumeLayout(false);
        }

        #endregion

        private Panel screenPanel;
        private Panel controlPanel;
        private Label lblCpuStatus;
        private Button btnStart;
        private Button btnStop;
        private Button btnReset;
        private Button btnColdBoot;
        private Label lblRegisters;
        private TextBox txtRegisters;
        private Label lblMemory;
        private TextBox txtMemory;
        private Label lblProgram;
        private Button btnLoadDemo;
        private Button btnLoadBin;
        private Button btnStep;
        private Label lblBitmapInfo;
        // PLUS de cpuTimer !
        private System.Windows.Forms.Timer displayTimer;
    }
}
