using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;

namespace DeepLTranslator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
       // private CancellationTokenSource _cancellationTokenSource;

        // Controles de la interfaz
        private TextBox _inputTextBox;
        private TextBox _outputTextBox;
        private ComboBox _sourceLanguageComboBox;
        private ComboBox _targetLanguageComboBox;
        private Label _detectedLanguageLabel;
        private Button _translateButton;
        private Button _listenButton;
        private Button _copyButton;
        private Button _clearButton;
        private Button _swapButton;
        private ProgressBar _progressBar;
        private Button _cancelButton;

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
            this.SuspendLayout();
            
            // Configuración del formulario principal
            this.Text = "DeepL Translator";
            this.Size = new Size(900, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.MinimumSize = new Size(800, 650);
            this.MaximizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            CreateControls();
            SetupModernUI();

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CreateControls()
        {
            // Panel principal con scroll automático
            var mainPanel = new Panel
            {
                Name = "mainPanel",
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = Color.Transparent,
                AutoScroll = true // Agregar scroll automático para pantallas pequeñas
            };

            // Título
            var titleLabel = new Label
            {
                Name = "titleLabel",
                Text = "🌐 DeepL Translator - Práctica Final",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                AutoSize = true,
                Location = new Point(0, 0)
            };

            // Etiqueta de técnicas implementadas
            var techniquesLabel = new Label
            {
                Name = "techniquesLabel",
                Text = "✨ Implementa: async/await, Task, CancellationToken, Task.WhenAll, Parallel.ForEach, Eventos Personalizados, LINQ",
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                ForeColor = Color.FromArgb(108, 117, 125),
                AutoSize = true,
                Location = new Point(0, 30)
            };

            // Área de texto de entrada
            var inputLabel = new Label
            {
                Name = "inputLabel",
                Text = "Enter text to translate:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Location = new Point(0, 50)
            };

            _inputTextBox = new TextBox
            {
                Name = "_inputTextBox",
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 75),
                Size = new Size(840, 120),
                TabIndex = 0,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Selectores de idioma
            var fromLabel = new Label
            {
                Name = "fromLabel",
                Text = "From:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Location = new Point(0, 210)
            };

            _sourceLanguageComboBox = new ComboBox
            {
                Name = "_sourceLanguageComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(0, 235),
                Size = new Size(200, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                FlatStyle = FlatStyle.Flat,
                TabIndex = 1
            };

            // Botón de intercambio
            _swapButton = CreateModernButton("⇄", new Point(210, 235), new Size(40, 30), Color.FromArgb(108, 117, 125));
            _swapButton.Name = "_swapButton";
            _swapButton.TabIndex = 2;
            _swapButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

            var toLabel = new Label
            {
                Name = "toLabel",
                Text = "To:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Location = new Point(260, 210)
            };

            _targetLanguageComboBox = new ComboBox
            {
                Name = "_targetLanguageComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                Location = new Point(260, 235),
                Size = new Size(200, 30),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(33, 37, 41),
                FlatStyle = FlatStyle.Flat,
                TabIndex = 3
            };

            _detectedLanguageLabel = new Label
            {
                Name = "_detectedLanguageLabel",
                Text = "Detected: 🌐 Auto-detect",
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(13, 110, 253),
                AutoSize = true,
                Location = new Point(0, 275),
                Visible = false,
                BackColor = Color.FromArgb(240, 248, 255), // Fondo azul claro
                Padding = new Padding(5, 2, 5, 2) // Padding interno
            };

            // Botones de acción
            _translateButton = CreateModernButton("🔄 Translate", new Point(480, 235), new Size(120, 30), Color.FromArgb(13, 110, 253));
            _translateButton.Name = "_translateButton";
            _translateButton.TabIndex = 4;

            _clearButton = CreateModernButton("🗑️ Clear", new Point(610, 235), new Size(100, 30), Color.FromArgb(108, 117, 125));
            _clearButton.Name = "_clearButton";
            _clearButton.TabIndex = 5;

            // Botón de cancelar
            _cancelButton = CreateModernButton("❌ Cancelar", new Point(720, 235), new Size(100, 30), Color.FromArgb(220, 53, 69));
            _cancelButton.Name = "_cancelButton";
            _cancelButton.TabIndex = 9;
            _cancelButton.Visible = false;
            
            _cancelButton.Click += (sender, e) =>
            {
               // _cancellationTokenSource?.Cancel();
                _cancelButton.Visible = false;
                _translateButton.Enabled = true;
                _translateButton.Text = "🔄 Traducir";
                _progressBar.Visible = false;
            };

            // Barra de progreso
            _progressBar = new ProgressBar
            {
                Name = "_progressBar",
                Location = new Point(0, 300),
                Size = new Size(840, 8), // Más alta para mejor visibilidad
                Style = ProgressBarStyle.Continuous, // Cambiar a continuo para mostrar progreso real
                Visible = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.FromArgb(13, 110, 253)
            };

            // Área de texto de salida
            var outputLabel = new Label
            {
                Name = "outputLabel",
                Text = "Translation:",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = Color.FromArgb(73, 80, 87),
                AutoSize = true,
                Location = new Point(0, 315)
            };

            _outputTextBox = new TextBox
            {
                Name = "_outputTextBox",
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(248, 249, 250),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(0, 340),
                Size = new Size(840, 120),
                ReadOnly = true,
                TabIndex = 6,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Botones de acción para la traducción
            _listenButton = CreateModernButton("🔊 Listen", new Point(0, 470), new Size(100, 35), Color.FromArgb(25, 135, 84));
            _listenButton.Name = "_listenButton";
            _listenButton.Enabled = false;
            _listenButton.TabIndex = 7;

            _copyButton = CreateModernButton("📋 Copy", new Point(110, 470), new Size(100, 35), Color.FromArgb(255, 193, 7));
            _copyButton.Name = "_copyButton";
            _copyButton.Enabled = false;
            _copyButton.TabIndex = 8;

            // Agregar todos los controles al panel principal
            mainPanel.Controls.AddRange(new Control[]
            {
                titleLabel, 
                techniquesLabel, // Agregar nueva etiqueta
                inputLabel, 
                _inputTextBox, 
                fromLabel,
                _sourceLanguageComboBox,
                _swapButton,
                toLabel,
                _targetLanguageComboBox,
                _detectedLanguageLabel,
                _translateButton, 
                _clearButton,
                _cancelButton, // Agregar botón de cancelar
                _progressBar,
                outputLabel, 
                _outputTextBox, 
                _listenButton, 
                _copyButton
            });

            // Agregar el panel principal al formulario
            this.Controls.Add(mainPanel);
        }

        private Button CreateModernButton(string text, Point location, Size size, Color backColor)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = size,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            // Configurar apariencia plana
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backColor, 0.1f);

            return button;
        }

        private void SetupModernUI()
        {
            try
            {
                ApplyRoundedCorners(_inputTextBox, 8);
                ApplyRoundedCorners(_outputTextBox, 8);
                ApplyRoundedCorners(_sourceLanguageComboBox, 6);
                ApplyRoundedCorners(_targetLanguageComboBox, 6);
                ApplyRoundedCorners(_translateButton, 6);
                ApplyRoundedCorners(_clearButton, 6);
                ApplyRoundedCorners(_swapButton, 6);
                ApplyRoundedCorners(_listenButton, 6);
                ApplyRoundedCorners(_copyButton, 6);
                ApplyRoundedCorners(_detectedLanguageLabel, 4);
                ApplyRoundedCorners(_cancelButton, 6);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying rounded corners: {ex.Message}");
            }

            try
            {
                ApplyShadowEffect(_inputTextBox);
                ApplyShadowEffect(_outputTextBox);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying shadow effects: {ex.Message}");
            }
        }

        private void ApplyRoundedCorners(Control control, int radius)
        {
            if (control == null || control.Width <= 0 || control.Height <= 0)
                return;

            try
            {
                var path = new GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(control.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(control.Width - radius, control.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, control.Height - radius, radius, radius, 90, 90);
                path.CloseAllFigures();
                control.Region = new Region(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying rounded corners to {control.Name}: {ex.Message}");
            }
        }

        private void ApplyShadowEffect(Control control)
        {
            try
            {
                var parent = control.Parent;
                if (parent == null) return;

                // Crear un panel de sombra detrás del control
                var shadowPanel = new Panel
                {
                    BackColor = Color.FromArgb(50, 0, 0, 0), // Sombra semi-transparente
                    Location = new Point(control.Location.X + 2, control.Location.Y + 2),
                    Size = control.Size,
                    Anchor = control.Anchor
                };

                // Insertar la sombra detrás del control
                parent.Controls.Add(shadowPanel);
                shadowPanel.BringToFront();
                control.BringToFront();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying shadow effect to {control.Name}: {ex.Message}");
            }
        }

        #endregion
    }
}
