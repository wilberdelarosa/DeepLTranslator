using System;
using System.Drawing;
using System.Windows.Forms;

namespace DeepLTranslator
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
        private Panel mainPanel;
        private Label titleLabel;
        private Label techniquesLabel;
        private Label inputLabel;
        private Label fromLabel;
        private Label toLabel;
        private Label outputLabel;

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
            mainPanel = new Panel();
            titleLabel = new Label();
            techniquesLabel = new Label();
            inputLabel = new Label();
            _inputTextBox = new TextBox();
            fromLabel = new Label();
            _sourceLanguageComboBox = new ComboBox();
            _swapButton = new Button();
            toLabel = new Label();
            _targetLanguageComboBox = new ComboBox();
            _detectedLanguageLabel = new Label();
            _translateButton = new Button();
            _clearButton = new Button();
            _cancelButton = new Button();
            _progressBar = new ProgressBar();
            outputLabel = new Label();
            _outputTextBox = new TextBox();
            _listenButton = new Button();
            _copyButton = new Button();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.AutoScroll = true;
            mainPanel.BackColor = Color.Transparent;
            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(techniquesLabel);
            mainPanel.Controls.Add(inputLabel);
            mainPanel.Controls.Add(_inputTextBox);
            mainPanel.Controls.Add(fromLabel);
            mainPanel.Controls.Add(_sourceLanguageComboBox);
            mainPanel.Controls.Add(_swapButton);
            mainPanel.Controls.Add(toLabel);
            mainPanel.Controls.Add(_targetLanguageComboBox);
            mainPanel.Controls.Add(_detectedLanguageLabel);
            mainPanel.Controls.Add(_translateButton);
            mainPanel.Controls.Add(_clearButton);
            mainPanel.Controls.Add(_cancelButton);
            mainPanel.Controls.Add(_progressBar);
            mainPanel.Controls.Add(outputLabel);
            mainPanel.Controls.Add(_outputTextBox);
            mainPanel.Controls.Add(_listenButton);
            mainPanel.Controls.Add(_copyButton);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(20);
            mainPanel.Size = new Size(1004, 766);
            mainPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(33, 37, 41);
            titleLabel.Location = new Point(0, 0);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(620, 48);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "🌐 DeepL Translator - Práctica Final";
            // 
            // techniquesLabel
            // 
            techniquesLabel.AutoSize = true;
            techniquesLabel.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            techniquesLabel.ForeColor = Color.FromArgb(108, 117, 125);
            techniquesLabel.Location = new Point(23, 664);
            techniquesLabel.Name = "techniquesLabel";
            techniquesLabel.Size = new Size(0, 21);
            techniquesLabel.TabIndex = 1;
            techniquesLabel.Click += techniquesLabel_Click;
            // 
            // inputLabel
            // 
            inputLabel.AutoSize = true;
            inputLabel.Font = new Font("Segoe UI", 10F);
            inputLabel.ForeColor = Color.FromArgb(73, 80, 87);
            inputLabel.Location = new Point(0, 50);
            inputLabel.Name = "inputLabel";
            inputLabel.Size = new Size(203, 28);
            inputLabel.TabIndex = 2;
            inputLabel.Text = "Enter text to translate:";
            // 
            // _inputTextBox
            // 
            _inputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _inputTextBox.BackColor = Color.White;
            _inputTextBox.BorderStyle = BorderStyle.FixedSingle;
            _inputTextBox.Font = new Font("Segoe UI", 11F);
            _inputTextBox.ForeColor = Color.FromArgb(33, 37, 41);
            _inputTextBox.Location = new Point(0, 87);
            _inputTextBox.Multiline = true;
            _inputTextBox.Name = "_inputTextBox";
            _inputTextBox.ScrollBars = ScrollBars.Vertical;
            _inputTextBox.Size = new Size(981, 120);
            _inputTextBox.TabIndex = 0;
            // 
            // fromLabel
            // 
            fromLabel.AutoSize = true;
            fromLabel.Font = new Font("Segoe UI", 10F);
            fromLabel.ForeColor = Color.FromArgb(73, 80, 87);
            fromLabel.Location = new Point(0, 210);
            fromLabel.Name = "fromLabel";
            fromLabel.Size = new Size(62, 28);
            fromLabel.TabIndex = 3;
            fromLabel.Text = "From:";
            // 
            // _sourceLanguageComboBox
            // 
            _sourceLanguageComboBox.BackColor = Color.White;
            _sourceLanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _sourceLanguageComboBox.FlatStyle = FlatStyle.Flat;
            _sourceLanguageComboBox.Font = new Font("Segoe UI", 10F);
            _sourceLanguageComboBox.ForeColor = Color.FromArgb(33, 37, 41);
            _sourceLanguageComboBox.Location = new Point(0, 235);
            _sourceLanguageComboBox.Name = "_sourceLanguageComboBox";
            _sourceLanguageComboBox.Size = new Size(200, 36);
            _sourceLanguageComboBox.TabIndex = 1;
            // 
            // _swapButton
            // 
            _swapButton.BackColor = Color.FromArgb(108, 117, 125);
            _swapButton.Cursor = Cursors.Hand;
            _swapButton.FlatAppearance.BorderSize = 0;
            _swapButton.FlatStyle = FlatStyle.Flat;
            _swapButton.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            _swapButton.ForeColor = Color.White;
            _swapButton.Location = new Point(210, 235);
            _swapButton.Name = "_swapButton";
            _swapButton.Size = new Size(40, 47);
            _swapButton.TabIndex = 2;
            _swapButton.Text = "⇄";
            _swapButton.UseVisualStyleBackColor = false;
            // 
            // toLabel
            // 
            toLabel.AutoSize = true;
            toLabel.Font = new Font("Segoe UI", 10F);
            toLabel.ForeColor = Color.FromArgb(73, 80, 87);
            toLabel.Location = new Point(260, 210);
            toLabel.Name = "toLabel";
            toLabel.Size = new Size(36, 28);
            toLabel.TabIndex = 4;
            toLabel.Text = "To:";
            // 
            // _targetLanguageComboBox
            // 
            _targetLanguageComboBox.BackColor = Color.White;
            _targetLanguageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            _targetLanguageComboBox.FlatStyle = FlatStyle.Flat;
            _targetLanguageComboBox.Font = new Font("Segoe UI", 10F);
            _targetLanguageComboBox.ForeColor = Color.FromArgb(33, 37, 41);
            _targetLanguageComboBox.Location = new Point(260, 235);
            _targetLanguageComboBox.Name = "_targetLanguageComboBox";
            _targetLanguageComboBox.Size = new Size(200, 36);
            _targetLanguageComboBox.TabIndex = 3;
            // 
            // _detectedLanguageLabel
            // 
            _detectedLanguageLabel.AutoSize = true;
            _detectedLanguageLabel.BackColor = Color.FromArgb(240, 248, 255);
            _detectedLanguageLabel.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            _detectedLanguageLabel.ForeColor = Color.FromArgb(13, 110, 253);
            _detectedLanguageLabel.Location = new Point(0, 275);
            _detectedLanguageLabel.Name = "_detectedLanguageLabel";
            _detectedLanguageLabel.Padding = new Padding(5, 2, 5, 2);
            _detectedLanguageLabel.Size = new Size(223, 29);
            _detectedLanguageLabel.TabIndex = 5;
            _detectedLanguageLabel.Text = "Detected: 🌐 Auto-detect";
            _detectedLanguageLabel.Visible = false;
            // 
            // _translateButton
            // 
            _translateButton.BackColor = Color.FromArgb(13, 110, 253);
            _translateButton.Cursor = Cursors.Hand;
            _translateButton.FlatAppearance.BorderSize = 0;
            _translateButton.FlatStyle = FlatStyle.Flat;
            _translateButton.Font = new Font("Segoe UI", 9F);
            _translateButton.ForeColor = Color.White;
            _translateButton.Location = new Point(480, 235);
            _translateButton.Name = "_translateButton";
            _translateButton.Size = new Size(120, 30);
            _translateButton.TabIndex = 4;
            _translateButton.Text = "🔄 Translate";
            _translateButton.UseVisualStyleBackColor = false;
            // 
            // _clearButton
            // 
            _clearButton.BackColor = Color.FromArgb(108, 117, 125);
            _clearButton.Cursor = Cursors.Hand;
            _clearButton.FlatAppearance.BorderSize = 0;
            _clearButton.FlatStyle = FlatStyle.Flat;
            _clearButton.Font = new Font("Segoe UI", 9F);
            _clearButton.ForeColor = Color.White;
            _clearButton.Location = new Point(610, 235);
            _clearButton.Name = "_clearButton";
            _clearButton.Size = new Size(100, 30);
            _clearButton.TabIndex = 5;
            _clearButton.Text = "🗑️ Clear";
            _clearButton.UseVisualStyleBackColor = false;
            // 
            // _cancelButton
            // 
            _cancelButton.BackColor = Color.FromArgb(220, 53, 69);
            _cancelButton.Cursor = Cursors.Hand;
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.FlatStyle = FlatStyle.Flat;
            _cancelButton.Font = new Font("Segoe UI", 9F);
            _cancelButton.ForeColor = Color.White;
            _cancelButton.Location = new Point(720, 235);
            _cancelButton.Name = "_cancelButton";
            _cancelButton.Size = new Size(100, 30);
            _cancelButton.TabIndex = 9;
            _cancelButton.Text = "❌ Cancelar";
            _cancelButton.UseVisualStyleBackColor = false;
            _cancelButton.Visible = false;
            // 
            // _progressBar
            // 
            _progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _progressBar.ForeColor = Color.FromArgb(13, 110, 253);
            _progressBar.Location = new Point(0, 300);
            _progressBar.Name = "_progressBar";
            _progressBar.Size = new Size(1024, 8);
            _progressBar.Style = ProgressBarStyle.Continuous;
            _progressBar.TabIndex = 10;
            _progressBar.Visible = false;
            // 
            // outputLabel
            // 
            outputLabel.AutoSize = true;
            outputLabel.Font = new Font("Segoe UI", 10F);
            outputLabel.ForeColor = Color.FromArgb(73, 80, 87);
            outputLabel.Location = new Point(0, 315);
            outputLabel.Name = "outputLabel";
            outputLabel.Size = new Size(110, 28);
            outputLabel.TabIndex = 11;
            outputLabel.Text = "Translation:";
            // 
            // _outputTextBox
            // 
            _outputTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _outputTextBox.BackColor = Color.FromArgb(248, 249, 250);
            _outputTextBox.BorderStyle = BorderStyle.FixedSingle;
            _outputTextBox.Font = new Font("Segoe UI", 11F);
            _outputTextBox.ForeColor = Color.FromArgb(33, 37, 41);
            _outputTextBox.Location = new Point(0, 361);
            _outputTextBox.Multiline = true;
            _outputTextBox.Name = "_outputTextBox";
            _outputTextBox.ReadOnly = true;
            _outputTextBox.ScrollBars = ScrollBars.Vertical;
            _outputTextBox.Size = new Size(981, 120);
            _outputTextBox.TabIndex = 6;
            // 
            // _listenButton
            // 
            _listenButton.BackColor = Color.FromArgb(25, 135, 84);
            _listenButton.Cursor = Cursors.Hand;
            _listenButton.Enabled = false;
            _listenButton.FlatAppearance.BorderSize = 0;
            _listenButton.FlatStyle = FlatStyle.Flat;
            _listenButton.Font = new Font("Segoe UI", 9F);
            _listenButton.ForeColor = Color.White;
            _listenButton.Location = new Point(-10, 534);
            _listenButton.Name = "_listenButton";
            _listenButton.Size = new Size(100, 35);
            _listenButton.TabIndex = 7;
            _listenButton.Text = "🔊 Listen";
            _listenButton.UseVisualStyleBackColor = false;
            // 
            // _copyButton
            // 
            _copyButton.BackColor = Color.FromArgb(255, 193, 7);
            _copyButton.Cursor = Cursors.Hand;
            _copyButton.Enabled = false;
            _copyButton.FlatAppearance.BorderSize = 0;
            _copyButton.FlatStyle = FlatStyle.Flat;
            _copyButton.Font = new Font("Segoe UI", 9F);
            _copyButton.ForeColor = Color.White;
            _copyButton.Location = new Point(100, 534);
            _copyButton.Name = "_copyButton";
            _copyButton.Size = new Size(100, 35);
            _copyButton.TabIndex = 8;
            _copyButton.Text = "📋 Copy";
            _copyButton.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            BackColor = Color.FromArgb(248, 249, 250);
            ClientSize = new Size(1004, 766);
            Controls.Add(mainPanel);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(800, 650);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "DeepL Translator";
            Load += MainForm_Load;
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}
