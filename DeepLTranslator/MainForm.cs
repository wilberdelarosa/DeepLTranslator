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
using System.Diagnostics;
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
            // Crear controles para que el diseñador de Visual Studio pueda renderizar el formulario
            InitializeComponent();

            // Si se está ejecutando dentro del diseñador, omitir la lógica de tiempo de ejecución

            // Permitir que el diseñador de Visual Studio cargue el formulario sin ejecutar lógica de tiempo de ejecución
            if (IsInDesignMode())
            {
                InitializeComponent();
                _languages = new List<LanguageInfo>();
                return;
            }

            try
            {
                ErrorLogger.LogInfo("Iniciando aplicación DeepL Translator", "MainForm Constructor");

                if (!AppConfig.ValidateApiKey())
                {
                    var errorMsg = "Clave API de DeepL inválida o faltante. Por favor verifica tu configuración.\n\n" +
                        "Puedes configurar la variable de entorno DEEPL_API_KEY con tu clave personal.";

                    ErrorLogger.LogError(new InvalidOperationException("API Key validation failed"), "API Key Validation");

                    MessageBox.Show(errorMsg, "Error de Configuración", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                    return;
                }

                _deepLService = new DeepLService(AppConfig.DeepLApiKey);
                _textToSpeechService = new TextToSpeechService();


                _languages = LanguageService.GetLanguagesWithFlags()
                    .OrderBy(l => l.Name)
                    .ToList();

                InitializeComponent();
                LoadLanguages();
                SetupEventHandlers();

                _deepLService.TranslationProgress += OnTranslationProgress;
                _deepLService.TranslationCompleted += OnTranslationCompleted;

                ErrorLogger.LogInfo("Aplicación inicializada correctamente", "MainForm Constructor");
            }
            catch (OutOfMemoryException ex)
            {
                ErrorLogger.LogError(ex, "MainForm Constructor - Memory Error");
                MessageBox.Show("Error de memoria insuficiente. Por favor cierra otras aplicaciones e intenta nuevamente.",
                    "Error de Memoria", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorLogger.LogError(ex, "MainForm Constructor - Access Error");
                MessageBox.Show("Error de permisos. Por favor ejecuta la aplicación como administrador.",
                    "Error de Permisos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "MainForm Constructor - General Error");
                MessageBox.Show($"Error crítico al inicializar la aplicación: {ex.Message}\n\n" +
                    $"Detalles técnicos guardados en: {ErrorLogger.GetLogFilePath()}",
                    "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static bool IsInDesignMode()
        {
            return LicenseManager.UsageMode == LicenseUsageMode.Designtime ||
                   Process.GetCurrentProcess().ProcessName.ToLower().Contains("devenv");
        }

        private void SetupEventHandlers()
        {
            _translateButton.Click += async (sender, e) => await TranslateText();
            _clearButton.Click += (sender, e) => ClearAll();
            _listenButton.Click += async (sender, e) => await PlayTranslation();
            _copyButton.Click += (sender, e) => CopyToClipboard();
            _swapButton.Click += (sender, e) => SwapLanguages();
            
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
                
                // Mostrar idioma detectado si aplica
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
            // Si los controles no están inicializados (p.ej. en modo diseño), salir
            if (_sourceLanguageComboBox == null || _targetLanguageComboBox == null)
            {
                return;
            }


            // Cargar idiomas de origen (incluir "Auto-detect" como primera opción)
            _sourceLanguageComboBox.Items.Clear();
            _sourceLanguageComboBox.Items.Add("🌐 Auto-detect");

            var formattedLanguages = _languages.Select(lang => $"{lang.FlagEmoji} {lang.Name}").ToArray();
            _sourceLanguageComboBox.Items.AddRange(formattedLanguages);
            _sourceLanguageComboBox.SelectedIndex = 0; // Auto-detect por defecto

            // Cargar idiomas de destino
            _targetLanguageComboBox.Items.Clear();
            _targetLanguageComboBox.Items.AddRange(formattedLanguages);

            // Seleccionar inglés como idioma de destino por defecto
            var defaultIndex = _languages.FindIndex(l => l.Code.Equals("EN-US", StringComparison.OrdinalIgnoreCase));
            _targetLanguageComboBox.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
        }

        private async Task TranslateText()
        {
            var inputText = _inputTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(inputText))
            {
                MessageBox.Show("Por favor ingresa texto para traducir.", "Sin Texto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _inputTextBox.Focus();
                return;
            }

            if (_targetLanguageComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Por favor selecciona un idioma de destino.", "Sin Idioma Seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _targetLanguageComboBox.Focus();
                return;
            }

            if (inputText.Length > AppConfig.MaxTextLength)
            {
                var result = MessageBox.Show($"El texto es más largo que {AppConfig.MaxTextLength} caracteres. Esto puede tomar más tiempo y consumir más cuota de API. ¿Continuar?", 
                    "Texto Largo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.No)
                    return;
            }

            try
            {
                ErrorLogger.LogInfo($"Iniciando traducción de texto ({inputText.Length} caracteres)", "TranslateText");

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                // Mostrar progreso
                _progressBar.Visible = true;
                _progressBar.Value = 0;
                _translateButton.Enabled = false;
                _translateButton.Text = "Preparando traducción...";

                if (_targetLanguageComboBox.SelectedIndex < 0 || _targetLanguageComboBox.SelectedIndex >= _languages.Count)
                {
                    throw new ArgumentOutOfRangeException("Selección de idioma de destino inválida");
                }

                var targetLanguage = _languages[_targetLanguageComboBox.SelectedIndex];

                string? sourceLanguageCode = null;
                if (_sourceLanguageComboBox.SelectedIndex > 0) // Si no es "Auto-detect"
                {
                    var sourceIndex = _sourceLanguageComboBox.SelectedIndex - 1;

                    if (sourceIndex < 0 || sourceIndex >= _languages.Count)
                    {
                        throw new ArgumentOutOfRangeException("Selección de idioma de origen inválida");
                    }

                    var sourceLanguage = _languages[sourceIndex];
                    sourceLanguageCode = sourceLanguage.Code;
                }

                System.Diagnostics.Debug.WriteLine($"Source: {sourceLanguageCode ?? "Auto-detect"}, Target: {targetLanguage.Code}");

                var (translatedText, detectedLanguage) = await _deepLService.TranslateTextAsync(
                    inputText,
                    targetLanguage.Code,
                    sourceLanguageCode,
                    _cancellationTokenSource.Token);

                if (string.IsNullOrWhiteSpace(translatedText))
                {
                    throw new InvalidOperationException("La traducción devolvió un resultado vacío");
                }

                ErrorLogger.LogInfo($"Traducción completada exitosamente. Idioma detectado: {detectedLanguage}", "TranslateText");

                System.Diagnostics.Debug.WriteLine($"Translation completed. Detected: {detectedLanguage}, Target: {targetLanguage.Code}");
            }
            catch (OperationCanceledException)
            {
                ErrorLogger.LogInfo("Traducción cancelada por el usuario", "TranslateText");
                System.Diagnostics.Debug.WriteLine("Translation was cancelled by user");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ErrorLogger.LogError(ex, "TranslateText - Selection Error");
                MessageBox.Show($"Error de selección: {ex.Message}\n\nPor favor verifica que hayas seleccionado idiomas válidos.", 
                    "Error de Selección", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Selection error: {ex}");
            }
            catch (HttpRequestException ex)
            {
                ErrorLogger.LogError(ex, "TranslateText - Network Error");
                MessageBox.Show($"Error de conexión: {ex.Message}\n\nVerifica tu conexión a internet e intenta nuevamente.", 
                    "Error de Red", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"TranslateText - General Error. Input length: {inputText?.Length}, Target: {_targetLanguageComboBox.SelectedIndex}");
                
                var errorMessage = ex.Message;
                if (ex.Message.Contains("Invalid API key") || ex.Message.Contains("authentication failed"))
                {
                    errorMessage += "\n\nPor favor verifica tu clave API de DeepL. Puedes configurar la variable de entorno DEEPL_API_KEY con tu clave personal.";
                }
                else if (ex.Message.Contains("quota exceeded") || ex.Message.Contains("access denied"))
                {
                    errorMessage += "\n\nTu cuota de API de DeepL ha sido excedida. Verifica tu cuenta o intenta más tarde.";
                }
                else if (ex.Message.Contains("Rate limit exceeded"))
                {
                    errorMessage += "\n\nLímite de velocidad excedido. Por favor espera un momento antes de intentar nuevamente.";
                }
                else if (ex.Message.Contains("Network error") || ex.Message.Contains("timeout"))
                {
                    errorMessage += "\n\nProblema de conexión. Verifica tu conexión a internet e intenta nuevamente.";
                }
                
                MessageBox.Show($"Error de traducción: {errorMessage}\n\n" +
                    $"Detalles técnicos guardados en: {ErrorLogger.GetLogFilePath()}", 
                    "Error de Traducción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Translation error: {ex}");
            }
            finally
            {
                // Ocultar progreso
                _progressBar.Visible = false;
                _translateButton.Enabled = true;
                _translateButton.Text = "🔄 Traducir";
            }
        }

        private void SwapLanguages()
        {
            // Solo intercambiar si no está en modo auto-detect
            if (_sourceLanguageComboBox.SelectedIndex > 0)
            {
                var sourceIndex = _sourceLanguageComboBox.SelectedIndex;
                var targetIndex = _targetLanguageComboBox.SelectedIndex;

                if (targetIndex < 0 || targetIndex >= _languages.Count ||
                    sourceIndex <= 0 || sourceIndex - 1 >= _languages.Count)
                {
                    MessageBox.Show("Selección de idioma inválida para la operación de intercambio.",
                        "Error de Intercambio", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Intercambiar selecciones (ajustar por el offset de auto-detect)
                _sourceLanguageComboBox.SelectedIndex = targetIndex + 1;
                _targetLanguageComboBox.SelectedIndex = sourceIndex - 1;

                // Intercambiar textos si hay contenido
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
                MessageBox.Show("No se pueden intercambiar idiomas cuando se usa Auto-detectar. Por favor selecciona un idioma de origen específico.",
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
                ErrorLogger.LogInfo($"Iniciando reproducción de texto ({_lastTranslatedText.Length} caracteres)", "PlayTranslation");

                _listenButton.Enabled = false;
                _listenButton.Text = "🔊 Reproduciendo...";

                if (_targetLanguageComboBox.SelectedIndex < 0 || _targetLanguageComboBox.SelectedIndex >= _languages.Count)
                {
                    throw new ArgumentOutOfRangeException("Selección de idioma inválida para reproducción");
                }

                var selectedLanguage = _languages[_targetLanguageComboBox.SelectedIndex];
                
                System.Diagnostics.Debug.WriteLine($"Speaking in language: {selectedLanguage.Code}");
                
                await _textToSpeechService.SpeakAsync(_lastTranslatedText, selectedLanguage.Code);

                ErrorLogger.LogInfo($"Reproducción completada exitosamente en idioma: {selectedLanguage.Code}", "PlayTranslation");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                ErrorLogger.LogError(ex, "PlayTranslation - Language Selection Error");
                MessageBox.Show("Error en la selección de idioma para reproducción. Por favor selecciona un idioma válido.", 
                    "Error de Selección", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ex)
            {
                ErrorLogger.LogError(ex, "PlayTranslation - TTS Error");
                MessageBox.Show($"Error en el sistema de texto a voz: {ex.Message}\n\nVerifica que tu sistema tenga configurado el sintetizador de voz.", 
                    "Error de Texto a Voz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"PlayTranslation - General Error. Text length: {_lastTranslatedText?.Length}, Language index: {_targetLanguageComboBox.SelectedIndex}");
                MessageBox.Show($"Error de texto a voz: {ex.Message}\n\n" +
                    $"Detalles técnicos guardados en: {ErrorLogger.GetLogFilePath()}", 
                    "Error de Reproducción", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"TTS error: {ex}");
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
                ErrorLogger.LogInfo($"Copiando texto al portapapeles ({_lastTranslatedText.Length} caracteres)", "CopyToClipboard");

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
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                ErrorLogger.LogError(ex, "CopyToClipboard - Clipboard Error");
                MessageBox.Show("Error al acceder al portapapeles. Otro programa puede estar usándolo. Intenta nuevamente.", 
                    "Error de Portapapeles", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, $"CopyToClipboard - General Error. Text length: {_lastTranslatedText?.Length}");
                MessageBox.Show($"Error al copiar texto: {ex.Message}\n\n" +
                    $"Detalles técnicos guardados en: {ErrorLogger.GetLogFilePath()}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                ErrorLogger.LogInfo("Cerrando aplicación DeepL Translator", "OnFormClosed");

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                
                _deepLService?.Dispose();
                _textToSpeechService?.Dispose();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "OnFormClosed - Cleanup Error");
            }
            finally
            {
                base.OnFormClosed(e);
            }
        }
    }
}
