using DeepLTranslator.Services;
using DeepLTranslator.Models;
using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using DeepLTranslator.Logging;

namespace DeepLTranslator
{
    public partial class MainForm : Form
    {
        private readonly DeepLService _deepLService;
        private readonly TextToSpeechService _textToSpeechService;
        private readonly List<LanguageInfo> _languages;
        private string _lastTranslatedText = string.Empty;
        private CancellationTokenSource? _cancellationTokenSource;

        public MainForm()
        {
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                InitializeComponent();
                _languages = new List<LanguageInfo>();
                return;
            }

            if (!AppConfig.ValidateApiKey())
            {
                MessageBox.Show("Clave API de DeepL inválida. Configura la variable DEEPL_API_KEY.",
                    "Error de Configuración", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            _deepLService = new DeepLService(AppConfig.DeepLApiKey);
            _textToSpeechService = new TextToSpeechService();
            _languages = LanguageService.GetLanguagesWithFlags().OrderBy(l => l.Name).ToList();

            InitializeComponent();

            if (!DesignMode)
            {
                LoadLanguages();
                SetupEventHandlers();
                _deepLService.TranslationProgress += OnTranslationProgress;
                _deepLService.TranslationCompleted += OnTranslationCompleted;
            }
        }

        private void SetupEventHandlers()
        {
            if (_translateButton != null)
                _translateButton.Click += async (sender, e) => await TranslateText();

            if (_clearButton != null)
                _clearButton.Click += (sender, e) => ClearAll();

            if (_listenButton != null)
                _listenButton.Click += async (sender, e) => await PlayTranslation();

            if (_copyButton != null)
                _copyButton.Click += (sender, e) => CopyToClipboard();

            if (_swapButton != null)
                _swapButton.Click += (sender, e) => SwapLanguages();

            if (_inputTextBox != null)
            {
                _inputTextBox.TextChanged += (sender, e) =>
                {
                    if (string.IsNullOrWhiteSpace(_inputTextBox.Text))
                        _detectedLanguageLabel.Visible = false;
                };

                _inputTextBox.KeyDown += async (sender, e) =>
                {
                    if (e.Control && e.KeyCode == Keys.Enter)
                    {
                        e.Handled = true;
                        await TranslateText();
                    }
                };
            }
        }

        private void OnTranslationProgress(object? sender, TranslationProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnTranslationProgress(sender, e)));
                return;
            }

            _progressBar.Value = e.ProgressPercentage;
            _translateButton.Text = e.Message;
        }

        private void OnTranslationCompleted(object? sender, TranslationCompletedEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => OnTranslationCompleted(sender, e)));
                return;
            }

            if (e.Success)
            {
                _outputTextBox.Text = e.TranslatedText;
                _lastTranslatedText = e.TranslatedText;

                // Mostrar idioma detectado
                if (_sourceLanguageComboBox.SelectedIndex == 0 && !string.IsNullOrEmpty(e.DetectedLanguage))
                {
                    var detectedLanguageName = LanguageService.GetLanguageNameFromDetected(e.DetectedLanguage);
                    var detectedLanguageFlag = LanguageService.GetLanguageFlagFromDetected(e.DetectedLanguage);
                    _detectedLanguageLabel.Text = $"Detectado: {detectedLanguageFlag} {detectedLanguageName}";
                    _detectedLanguageLabel.Visible = true;
                    _detectedLanguageLabel.BringToFront();
                }

                _listenButton.Enabled = true;
                _copyButton.Enabled = true;
            }
            else
            {
                MessageBox.Show($"Error de traducción: {e.ErrorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLanguages()
        {
            if (_sourceLanguageComboBox == null || _targetLanguageComboBox == null)
                return;

            // Cargar idiomas de origen
            _sourceLanguageComboBox.Items.Clear();
            _sourceLanguageComboBox.Items.Add("🌐 Auto-detect");

            var formattedLanguages = _languages.Select(lang => $"{lang.FlagEmoji} {lang.Name}").ToArray();
            _sourceLanguageComboBox.Items.AddRange(formattedLanguages);
            _sourceLanguageComboBox.SelectedIndex = 0;

            // Cargar idiomas de destino
            _targetLanguageComboBox.Items.Clear();
            _targetLanguageComboBox.Items.AddRange(formattedLanguages);
            _targetLanguageComboBox.SelectedIndex = 0;
        }

        private async Task TranslateText()
        {
            var inputText = _inputTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(inputText))
            {
                MessageBox.Show("Ingresa texto para traducir.", "Sin Texto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_targetLanguageComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Selecciona un idioma de destino.", "Sin Idioma", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                _progressBar.Visible = true;
                _progressBar.Value = 0;
                _translateButton.Enabled = false;
                _translateButton.Text = "Traduciendo...";

                var targetLanguage = _languages[_targetLanguageComboBox.SelectedIndex];
                string? sourceLanguageCode = null;

                if (_sourceLanguageComboBox.SelectedIndex > 0)
                {
                    var sourceIndex = _sourceLanguageComboBox.SelectedIndex - 1;
                    sourceLanguageCode = _languages[sourceIndex].Code;
                }

                var (translatedText, detectedLanguage) = await _deepLService.TranslateTextAsync(
                    inputText, targetLanguage.Code, sourceLanguageCode, _cancellationTokenSource.Token);

                if (string.IsNullOrWhiteSpace(translatedText))
                    throw new InvalidOperationException("La traducción devolvió un resultado vacío");
            }
            catch (OperationCanceledException)
            {
                // Traducción cancelada - no mostrar error
            }
            catch (Exception ex)
            {
                var errorMessage = ex.Message;
                if (ex.Message.Contains("Invalid API key"))
                    errorMessage += "\n\nVerifica tu clave API de DeepL.";
                else if (ex.Message.Contains("quota exceeded"))
                    errorMessage += "\n\nCuota de API excedida.";

                MessageBox.Show($"Error: {errorMessage}", "Error de Traducción", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _progressBar.Visible = false;
                _translateButton.Enabled = true;
                _translateButton.Text = "🔄 Traducir";
            }
        }

        private void SwapLanguages()
        {
            if (_sourceLanguageComboBox.SelectedIndex > 0)
            {
                var sourceIndex = _sourceLanguageComboBox.SelectedIndex;
                var targetIndex = _targetLanguageComboBox.SelectedIndex;

                _sourceLanguageComboBox.SelectedIndex = targetIndex + 1;
                _targetLanguageComboBox.SelectedIndex = sourceIndex - 1;

                if (!string.IsNullOrEmpty(_outputTextBox.Text))
                {
                    var tempText = _inputTextBox.Text;
                    _inputTextBox.Text = _outputTextBox.Text;
                    _outputTextBox.Text = tempText;
                    _lastTranslatedText = tempText;
                }
            }
            else
            {
                MessageBox.Show("Selecciona un idioma de origen específico para intercambiar.",
                    "No se Puede Intercambiar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearAll()
        {
            _cancellationTokenSource?.Cancel();
            _inputTextBox.Clear();
            _outputTextBox.Clear();
            _detectedLanguageLabel.Visible = false;
            _listenButton.Enabled = false;
            _copyButton.Enabled = false;
            _inputTextBox.Focus();
        }

        private async Task PlayTranslation()
        {
            if (string.IsNullOrEmpty(_lastTranslatedText))
                return;

            try
            {
                _listenButton.Enabled = false;
                _listenButton.Text = "🔊 Reproduciendo...";

                var selectedLanguage = _languages[_targetLanguageComboBox.SelectedIndex];
                await _textToSpeechService.SpeakAsync(_lastTranslatedText, selectedLanguage.Code);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de texto a voz: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                _listenButton.Enabled = true;
                _listenButton.Text = "🔊 Escuchar";
            }
        }

        private void CopyToClipboard()
        {
            if (string.IsNullOrEmpty(_lastTranslatedText))
                return;

            try
            {
                Clipboard.SetText(_lastTranslatedText);

                var originalText = _copyButton.Text;
                var originalColor = _copyButton.BackColor;
                _copyButton.Text = "✅ Copiado!";
                _copyButton.BackColor = Color.FromArgb(25, 135, 84);

                var timer = new System.Windows.Forms.Timer { Interval = 2000 };
                timer.Tick += (s, e) =>
                {
                    _copyButton.Text = originalText;
                    _copyButton.BackColor = originalColor;
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al copiar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _deepLService?.Dispose();
            _textToSpeechService?.Dispose();
            base.OnFormClosed(e);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Evento requerido por el diseñador
        }

        private void techniquesLabel_Click(object sender, EventArgs e)
        {

        }
    }
}
