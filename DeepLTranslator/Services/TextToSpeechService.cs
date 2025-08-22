using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;

namespace DeepLTranslator.Services
{
    public class TextToSpeechService : IDisposable
    {
        private readonly SpeechSynthesizer _synthesizer;
        private readonly Dictionary<string, VoiceSettings> _languageSettings;

        public TextToSpeechService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
            _languageSettings = InitializeLanguageSettings();
        }

        public async Task SpeakAsync(string text, string languageCode = "en-US")
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text to speak cannot be null or empty", nameof(text));

            try
            {
                // Detener cualquier reproducción anterior
                _synthesizer.SpeakAsyncCancelAll();

                // Mapear códigos de idioma de DeepL a códigos de voz
                var voiceLanguage = MapLanguageCodeToVoice(languageCode);
                var voiceSettings = GetVoiceSettings(languageCode);

                System.Diagnostics.Debug.WriteLine($"Original language code: {languageCode}, Mapped to: {voiceLanguage}");

                // Intentar establecer la voz para el idioma
                var voices = _synthesizer.GetInstalledVoices()?.Where(v => v.Enabled).ToList();

                if (voices == null || voices.Count == 0)
                {
                    throw new Exception("No text-to-speech voices are installed on this system");
                }

                // Buscar la mejor voz disponible con fallbacks inteligentes
                var selectedVoice = SelectBestVoice(voices, voiceLanguage, languageCode);

                if (selectedVoice != null)
                {
                    _synthesizer.SelectVoice(selectedVoice.VoiceInfo.Name);
                    System.Diagnostics.Debug.WriteLine($"Selected voice: {selectedVoice.VoiceInfo.Name} for culture: {selectedVoice.VoiceInfo.Culture.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No voice found for language: {voiceLanguage}, using default");
                    var defaultVoice = voices.FirstOrDefault();
                    if (defaultVoice != null)
                    {
                        _synthesizer.SelectVoice(defaultVoice.VoiceInfo.Name);
                    }
                    else
                    {
                        throw new Exception("No default voice available");
                    }
                }

                // Aplicar configuraciones específicas del idioma
                _synthesizer.Rate = voiceSettings.Rate;
                _synthesizer.Volume = voiceSettings.Volume;

                // Limitar texto si es muy largo
                if (text.Length > 2000)
                {
                    text = text.Substring(0, 2000) + "...";
                }

                await Task.Run(() => _synthesizer.Speak(text));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS Error: {ex.Message}");
                throw new Exception($"Text-to-speech failed: {ex.Message}");
            }
        }

        private InstalledVoice SelectBestVoice(List<InstalledVoice> voices, string targetLanguage, string originalCode)
        {
            // 1. Buscar coincidencia exacta de cultura
            var exactMatch = voices.FirstOrDefault(v =>
                v.VoiceInfo.Culture.Name.Equals(targetLanguage, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null) return exactMatch;

            // 2. Buscar por código de idioma principal
            var languageCode = targetLanguage.Split('-')[0];
            var languageMatch = voices.FirstOrDefault(v =>
                v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals(languageCode, StringComparison.OrdinalIgnoreCase));
            if (languageMatch != null) return languageMatch;

            // 3. Buscar por familia de idiomas (ej: español de cualquier región)
            var familyMatch = voices.FirstOrDefault(v =>
                v.VoiceInfo.Culture.Name.StartsWith(languageCode + "-", StringComparison.OrdinalIgnoreCase));
            if (familyMatch != null) return familyMatch;

            // 4. Fallback a inglés si está disponible
            var englishFallback = voices.FirstOrDefault(v =>
                v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals("en", StringComparison.OrdinalIgnoreCase));
            if (englishFallback != null) return englishFallback;

            return null;
        }

        private VoiceSettings GetVoiceSettings(string languageCode)
        {
            var key = languageCode.ToUpper();
            return _languageSettings.ContainsKey(key) ? _languageSettings[key] : _languageSettings["DEFAULT"];
        }

        private Dictionary<string, VoiceSettings> InitializeLanguageSettings()
        {
            return new Dictionary<string, VoiceSettings>
            {
                ["EN"] = new VoiceSettings { Rate = 0, Volume = 100 },
                ["EN-US"] = new VoiceSettings { Rate = 0, Volume = 100 },
                ["EN-GB"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["ES"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["ES-ES"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["ES-MX"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["FR"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["FR-FR"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["FR-CA"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["DE"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["DE-AT"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["DE-CH"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["IT"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["PT"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["PT-BR"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["RU"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["JA"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["ZH"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["ZH-TW"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["KO"] = new VoiceSettings { Rate = -2, Volume = 100 },
                ["AR"] = new VoiceSettings { Rate = -1, Volume = 100 },
                ["DEFAULT"] = new VoiceSettings { Rate = 0, Volume = 100 }
            };
        }

        public void Stop()
        {
            _synthesizer.SpeakAsyncCancelAll();
        }

        private string MapLanguageCodeToVoice(string deepLLanguageCode)
        {
            return deepLLanguageCode.ToUpper() switch
            {
                "EN" or "EN-US" => "en-US",
                "EN-GB" => "en-GB",
                "EN-AU" => "en-AU",
                "EN-CA" => "en-CA",
                "ES" => "es-ES",
                "ES-ES" => "es-ES",
                "ES-MX" => "es-MX",
                "ES-AR" => "es-AR",
                "FR" => "fr-FR",
                "FR-FR" => "fr-FR",
                "FR-CA" => "fr-CA",
                "DE" => "de-DE",
                "DE-AT" => "de-AT",
                "DE-CH" => "de-CH",
                "IT" => "it-IT",
                "PT" or "PT-PT" => "pt-PT",
                "PT-BR" => "pt-BR",
                "RU" => "ru-RU",
                "JA" => "ja-JP",
                "ZH" => "zh-CN",
                "ZH-TW" => "zh-TW",
                "KO" => "ko-KR",
                "NL" => "nl-NL",
                "NL-BE" => "nl-BE",
                "PL" => "pl-PL",
                "SV" => "sv-SE",
                "DA" => "da-DK",
                "NO" => "nb-NO",
                "FI" => "fi-FI",
                "CS" => "cs-CZ",
                "SK" => "sk-SK",
                "HU" => "hu-HU",
                "RO" => "ro-RO",
                "BG" => "bg-BG",
                "HR" => "hr-HR",
                "SL" => "sl-SI",
                "ET" => "et-EE",
                "LV" => "lv-LV",
                "LT" => "lt-LT",
                "EL" => "el-GR",
                "TR" => "tr-TR",
                "AR" => "ar-SA",
                "HE" => "he-IL",
                "HI" => "hi-IN",
                "TH" => "th-TH",
                "VI" => "vi-VN",
                "ID" => "id-ID",
                "MS" => "ms-MY",
                "UK" => "uk-UA",
                _ => "en-US"
            };
        }

        public List<string> GetAvailableVoices()
        {
            var voices = _synthesizer.GetInstalledVoices();
            return voices.Select(v => $"{v.VoiceInfo.Name} ({v.VoiceInfo.Culture.Name})").ToList();
        }

        public void Dispose()
        {
            _synthesizer?.Dispose();
        }
    }

    public class VoiceSettings
    {
        public int Rate { get; set; } = 0;
        public int Volume { get; set; } = 100;
    }
}
