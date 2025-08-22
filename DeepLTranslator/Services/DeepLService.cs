using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using DeepLTranslator.Models;
using DeepLTranslator.Logging; // Assuming ErrorLogger is in the DeepLTranslator.Logging namespace

namespace DeepLTranslator.Services
{
    public class DeepLService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api-free.deepl.com/v2";
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 1000;

        public event EventHandler<TranslationProgressEventArgs>? TranslationProgress;
        public event EventHandler<TranslationCompletedEventArgs>? TranslationCompleted;

        public DeepLService(string apiKey)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));
                    
                _apiKey = apiKey;
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"DeepL-Auth-Key {_apiKey}");
                _httpClient.Timeout = TimeSpan.FromSeconds(30);

                ErrorLogger.LogInfo("DeepL Service inicializado correctamente", "DeepLService Constructor");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "DeepLService Constructor");
                throw;
            }
        }

        public async Task<(string translatedText, string detectedLanguage)> TranslateTextAsync(
            string text, 
            string targetLanguage, 
            string? sourceLanguage = null, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    throw new ArgumentException("Text to translate cannot be null or empty", nameof(text));
                
                if (string.IsNullOrWhiteSpace(targetLanguage))
                    throw new ArgumentException("Target language cannot be null or empty", nameof(targetLanguage));

                ErrorLogger.LogInfo($"Iniciando traducción: {text.Length} caracteres, de '{sourceLanguage ?? "AUTO"}' a '{targetLanguage}'", "TranslateTextAsync");

                var tasks = new List<Task>();
                
                // Tarea 1: Validar conectividad
                var connectivityTask = ValidateConnectivityAsync(cancellationToken);
                tasks.Add(connectivityTask);
                
                // Tarea 2: Detectar idioma si es necesario (en paralelo)
                Task<string> languageDetectionTask = null;
                if (string.IsNullOrEmpty(sourceLanguage))
                {
                    languageDetectionTask = Task.Run(() => DetectLanguageFromText(text), cancellationToken);
                    tasks.Add(languageDetectionTask);
                }

                // Esperar que todas las tareas preparatorias terminen
                await Task.WhenAll(tasks);

                // Reportar progreso
                OnTranslationProgress(new TranslationProgressEventArgs("Iniciando traducción...", 25));

                for (int attempt = 1; attempt <= MaxRetries; attempt++)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        OnTranslationProgress(new TranslationProgressEventArgs($"Intento {attempt} de {MaxRetries}...", 25 + (attempt * 25)));

                        var parameters = new List<KeyValuePair<string, string>>
                        {
                            new("text", text.Trim()),
                            new("target_lang", targetLanguage.ToUpper())
                        };

                        if (!string.IsNullOrEmpty(sourceLanguage))
                        {
                            parameters.Add(new("source_lang", sourceLanguage.ToUpper()));
                        }

                        var content = new FormUrlEncodedContent(parameters);
                        
                        var response = await _httpClient.PostAsync($"{BaseUrl}/translate", content, cancellationToken);

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            
                            System.Diagnostics.Debug.WriteLine($"DeepL API Response: {jsonResponse}");
                            
                            if (string.IsNullOrWhiteSpace(jsonResponse))
                                throw new Exception("Empty response from DeepL API");

                            var translationResponse = JsonConvert.DeserializeObject<DeepLTranslationResponse>(jsonResponse);

                            if (translationResponse?.Translations == null || translationResponse.Translations.Count == 0)
                                throw new Exception("No translations returned from DeepL API");

                            var translation = translationResponse.Translations[0];
                            
                            if (string.IsNullOrWhiteSpace(translation.Text))
                                throw new Exception("Empty translation text received");

                            var detectedLang = translation.DetectedSourceLanguage;
                            
                            if (string.IsNullOrWhiteSpace(detectedLang) && languageDetectionTask != null)
                            {
                                detectedLang = await languageDetectionTask;
                            }
                            
                            if (string.IsNullOrWhiteSpace(detectedLang))
                            {
                                detectedLang = "AUTO";
                            }
                            
                            System.Diagnostics.Debug.WriteLine($"Detected language: '{detectedLang}', Source specified: '{sourceLanguage}'");

                            ErrorLogger.LogInfo($"Traducción exitosa en intento {attempt}. Idioma detectado: {detectedLang}", "TranslateTextAsync");

                            OnTranslationProgress(new TranslationProgressEventArgs("Traducción completada", 100));
                            OnTranslationCompleted(new TranslationCompletedEventArgs(translation.Text.Trim(), detectedLang, true));

                            return (translation.Text.Trim(), detectedLang);
                        }
                        else
                        {
                            var errorContent = await response.Content.ReadAsStringAsync();
                            
                            ErrorLogger.LogError(new HttpRequestException($"API Error: {response.StatusCode} - {errorContent}"), 
                                $"TranslateTextAsync - Attempt {attempt}");
                            
                            var errorMessage = response.StatusCode switch
                            {
                                System.Net.HttpStatusCode.Unauthorized => "Invalid API key or authentication failed",
                                System.Net.HttpStatusCode.Forbidden => "API key quota exceeded or access denied",
                                System.Net.HttpStatusCode.BadRequest => $"Invalid request parameters: {errorContent}",
                                System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded, please try again later",
                                _ => $"API Error ({response.StatusCode}): {errorContent}"
                            };
                            
                            // Don't retry on authentication or quota errors
                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                OnTranslationCompleted(new TranslationCompletedEventArgs(string.Empty, string.Empty, false, errorMessage));
                                throw new Exception(errorMessage);
                            }
                            
                            // Retry on other errors
                            if (attempt == MaxRetries)
                            {
                                OnTranslationCompleted(new TranslationCompletedEventArgs(string.Empty, string.Empty, false, errorMessage));
                                throw new Exception(errorMessage);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        ErrorLogger.LogInfo($"Traducción cancelada en intento {attempt}", "TranslateTextAsync");
                        OnTranslationCompleted(new TranslationCompletedEventArgs(string.Empty, string.Empty, false, "Operación cancelada por el usuario"));
                        throw;
                    }
                    catch (HttpRequestException ex)
                    {
                        ErrorLogger.LogError(ex, $"TranslateTextAsync - Network Error - Attempt {attempt}");
                        if (attempt == MaxRetries)
                        {
                            OnTranslationCompleted(new TranslationCompletedEventArgs(string.Empty, string.Empty, false, $"Network error: {ex.Message}"));
                            throw new Exception($"Network error: {ex.Message}");
                        }
                    }
               
                    catch (JsonException ex)
                    {
                        ErrorLogger.LogError(ex, $"TranslateTextAsync - JSON Parse Error - Attempt {attempt}");
                        OnTranslationCompleted(new TranslationCompletedEventArgs(string.Empty, string.Empty, false, $"Failed to parse API response: {ex.Message}"));
                        throw new Exception($"Failed to parse API response: {ex.Message}");
                    }
                    catch (Exception ex) when (!(ex is ArgumentException))
                    {
                        ErrorLogger.LogError(ex, $"TranslateTextAsync - General Error - Attempt {attempt}");
                        if (attempt == MaxRetries)
                        {
                            OnTranslationCompleted(new TranslationCompletedEventArgs(string.Empty, string.Empty, false, $"Translation failed: {ex.Message}"));
                            throw new Exception($"Translation failed: {ex.Message}");
                        }
                    }

                    if (attempt < MaxRetries)
                    {
                        ErrorLogger.LogInfo($"Esperando {RetryDelayMs * attempt}ms antes del reintento {attempt + 1}", "TranslateTextAsync");
                        await Task.Delay(RetryDelayMs * attempt, cancellationToken);
                    }
                }

                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "TranslateTextAsync - Unhandled Error");
                throw;
            }
        }

        public async Task<List<(string original, string translated, string detectedLanguage)>> TranslateMultipleTextsAsync(
            IEnumerable<string> texts, 
            string targetLanguage, 
            string? sourceLanguage = null,
            CancellationToken cancellationToken = default)
        {
            var textList = texts.ToList();
            var results = new List<(string original, string translated, string detectedLanguage)>();
            var lockObject = new object();

            await Task.Run(() =>
            {
                Parallel.ForEach(textList, new ParallelOptions 
                { 
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount 
                }, 
                async text =>
                {
                    try
                    {
                        var (translated, detected) = await TranslateTextAsync(text, targetLanguage, sourceLanguage, cancellationToken);
                        
                        lock (lockObject)
                        {
                            results.Add((text, translated, detected));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error translating text '{text}': {ex.Message}");
                        lock (lockObject)
                        {
                            results.Add((text, $"Error: {ex.Message}", "ERROR"));
                        }
                    }
                });
            }, cancellationToken);

            return results.OrderBy(r => textList.IndexOf(r.original)).ToList();
        }

        private async Task ValidateConnectivityAsync(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/languages?type=target", cancellationToken);
                System.Diagnostics.Debug.WriteLine($"Connectivity check: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connectivity check failed: {ex.Message}");
            }
        }

        public async Task<(string translatedText, string detectedLanguage)> TranslateTextAsync(string text, string targetLanguage, string? sourceLanguage = null)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text to translate cannot be null or empty", nameof(text));
            
            if (string.IsNullOrWhiteSpace(targetLanguage))
                throw new ArgumentException("Target language cannot be null or empty", nameof(targetLanguage));

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var parameters = new List<KeyValuePair<string, string>>
                    {
                        new("text", text.Trim()),
                        new("target_lang", targetLanguage.ToUpper())
                    };

                    if (!string.IsNullOrEmpty(sourceLanguage))
                    {
                        parameters.Add(new("source_lang", sourceLanguage.ToUpper()));
                    }

                    var content = new FormUrlEncodedContent(parameters);
                    var response = await _httpClient.PostAsync($"{BaseUrl}/translate", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        
                        System.Diagnostics.Debug.WriteLine($"DeepL API Response: {jsonResponse}");
                        
                        if (string.IsNullOrWhiteSpace(jsonResponse))
                            throw new Exception("Empty response from DeepL API");

                        var translationResponse = JsonConvert.DeserializeObject<DeepLTranslationResponse>(jsonResponse);

                        if (translationResponse?.Translations == null || translationResponse.Translations.Count == 0)
                            throw new Exception("No translations returned from DeepL API");

                        var translation = translationResponse.Translations[0];
                        
                        if (string.IsNullOrWhiteSpace(translation.Text))
                            throw new Exception("Empty translation text received");

                        var detectedLang = translation.DetectedSourceLanguage;
                        
                        if (string.IsNullOrWhiteSpace(detectedLang) && string.IsNullOrEmpty(sourceLanguage))
                        {
                            detectedLang = DetectLanguageFromText(text);
                        }
                        
                        if (string.IsNullOrWhiteSpace(detectedLang))
                        {
                            detectedLang = "AUTO";
                        }
                        
                        System.Diagnostics.Debug.WriteLine($"Detected language: '{detectedLang}', Source specified: '{sourceLanguage}'");

                        return (translation.Text.Trim(), detectedLang);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        
                        var errorMessage = response.StatusCode switch
                        {
                            System.Net.HttpStatusCode.Unauthorized => "Invalid API key or authentication failed",
                            System.Net.HttpStatusCode.Forbidden => "API key quota exceeded or access denied",
                            System.Net.HttpStatusCode.BadRequest => $"Invalid request parameters: {errorContent}",
                            System.Net.HttpStatusCode.TooManyRequests => "Rate limit exceeded, please try again later",
                            _ => $"API Error ({response.StatusCode}): {errorContent}"
                        };
                        
                        // Don't retry on authentication or quota errors
                        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                        {
                            throw new Exception(errorMessage);
                        }
                        
                        // Retry on other errors
                        if (attempt == MaxRetries)
                            throw new Exception(errorMessage);
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (attempt == MaxRetries)
                        throw new Exception($"Network error: {ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    if (attempt == MaxRetries)
                        throw new Exception("Request timeout - please check your internet connection");
                }
                catch (JsonException ex)
                {
                    throw new Exception($"Failed to parse API response: {ex.Message}");
                }
                catch (Exception ex) when (!(ex is ArgumentException))
                {
                    if (attempt == MaxRetries)
                        throw new Exception($"Translation failed: {ex.Message}");
                }

                if (attempt < MaxRetries)
                    await Task.Delay(RetryDelayMs * attempt);
            }

            return (string.Empty, string.Empty);
        }

        public async Task<List<DeepLLanguagesResponse>> GetSupportedLanguagesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/languages?type=target");
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var languages = JsonConvert.DeserializeObject<List<DeepLLanguagesResponse>>(jsonResponse);
                    return languages ?? new List<DeepLLanguagesResponse>();
                }
            }
            catch (Exception ex)
            {
                // En caso de error, devolver idiomas predeterminados
                Console.WriteLine($"Error getting languages: {ex.Message}");
            }

            return GetDefaultLanguages();
        }

        private List<DeepLLanguagesResponse> GetDefaultLanguages()
        {
            return new List<DeepLLanguagesResponse>
            {
                new() { Language = "EN-US", Name = "English (American)" },
                new() { Language = "ES", Name = "Spanish" },
                new() { Language = "FR", Name = "French" },
                new() { Language = "DE", Name = "German" },
                new() { Language = "IT", Name = "Italian" },
                new() { Language = "PT-PT", Name = "Portuguese" },
                new() { Language = "RU", Name = "Russian" },
                new() { Language = "JA", Name = "Japanese" },
                new() { Language = "ZH", Name = "Chinese" }
            };
        }

        private string DetectLanguageFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "AUTO";

            text = text.ToLower().Trim();
            
            // Contadores para diferentes idiomas usando LINQ
            var languagePatterns = new Dictionary<string, (string[] commonWords, string[] specialChars, string[] patterns)>
            {
                ["ES"] = (
                    new[] { " el ", " la ", " de ", " que ", " en ", " es ", " se ", " con ", " por ", " para ", " una ", " del " },
                    new[] { "ñ", "¿", "¡", "á", "é", "í", "ó", "ú" },
                    new[] { "ción", "dad", "mente" }
                ),
                ["FR"] = (
                    new[] { " le ", " la ", " de ", " et ", " est ", " dans ", " avec ", " pour ", " que ", " une ", " des ", " du " },
                    new[] { "ç", "à", "é", "è", "ê", "ë", "î", "ï", "ô", "ù", "û", "ü", "ÿ" },
                    new[] { "tion", "ment", "ique" }
                ),
                ["DE"] = (
                    new[] { " der ", " die ", " das ", " und ", " ist ", " mit ", " von ", " zu ", " auf ", " für ", " ein ", " eine " },
                    new[] { "ä", "ö", "ü", "ß" },
                    new[] { "ung", "keit", "lich" }
                ),
                ["IT"] = (
                    new[] { " il ", " la ", " di ", " che ", " con ", " per ", " una ", " del ", " della ", " sono ", " essere " },
                    new[] { "à", "è", "é", "ì", "í", "î", "ò", "ó", "ù", "ú" },
                    new[] { "zione", "mente", "ità" }
                ),
                ["PT"] = (
                    new[] { " o ", " a ", " de ", " que ", " em ", " para ", " com ", " uma ", " do ", " da ", " são ", " ser " },
                    new[] { "ã", "õ", "ç", "á", "à", "â", "é", "ê", "í", "ó", "ô", "ú" },
                    new[] { "ção", "mente", "dade" }
                ),
                ["EN"] = (
                    new[] { " the ", " and ", " of ", " to ", " in ", " is ", " that ", " for ", " with ", " on ", " are ", " this " },
                    new string[0],
                    new[] { "ing", "tion", "ness" }
                ),
                ["RU"] = (
                    new[] { " и ", " в ", " не ", " на ", " с ", " что ", " как ", " по ", " за ", " от ", " для " },
                    new[] { "а", "б", "в", "г", "д", "е", "ё", "ж", "з", "и", "й", "к", "л", "м", "н", "о", "п", "р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ", "ъ", "ы", "ь", "э", "ю", "я" },
                    new string[0]
                )
            };

            var scores = languagePatterns.ToDictionary(
                kvp => kvp.Key,
                kvp => 
                {
                    var (commonWords, specialChars, patterns) = kvp.Value;
                    
                    var wordScore = commonWords.Count(word => text.Contains(word)) * 3;
                    var charScore = specialChars.Count(ch => text.Contains(ch)) * 2;
                    var patternScore = patterns.Count(pattern => text.Contains(pattern));
                    
                    return wordScore + charScore + patternScore;
                }
            );
            
            var bestMatch = scores
                .Where(s => s.Value >= 3)
                .OrderByDescending(s => s.Value)
                .FirstOrDefault();
            
            if (bestMatch.Key != null)
            {
                System.Diagnostics.Debug.WriteLine($"Language detection: {bestMatch.Key} with score {bestMatch.Value}");
                return bestMatch.Key;
            }
            
            System.Diagnostics.Debug.WriteLine($"Language detection failed, best score was {scores.Max(s => s.Value)}");
            return "AUTO";
        }

        protected virtual void OnTranslationProgress(TranslationProgressEventArgs e)
        {
            TranslationProgress?.Invoke(this, e);
        }

        protected virtual void OnTranslationCompleted(TranslationCompletedEventArgs e)
        {
            TranslationCompleted?.Invoke(this, e);
        }

        public void Dispose()
        {
            try
            {
                ErrorLogger.LogInfo("Liberando recursos del DeepL Service", "Dispose");
                _httpClient?.Dispose();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, "DeepLService Dispose");
            }
        }
    }

    public class TranslationProgressEventArgs : EventArgs
    {
        public string Message { get; }
        public int ProgressPercentage { get; }

        public TranslationProgressEventArgs(string message, int progressPercentage)
        {
            Message = message;
            ProgressPercentage = progressPercentage;
        }
    }

    public class TranslationCompletedEventArgs : EventArgs
    {
        public string TranslatedText { get; }
        public string DetectedLanguage { get; }
        public bool Success { get; }
        public string? ErrorMessage { get; }

        public TranslationCompletedEventArgs(string translatedText, string detectedLanguage, bool success, string? errorMessage = null)
        {
            TranslatedText = translatedText;
            DetectedLanguage = detectedLanguage;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}
