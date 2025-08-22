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

        public TextToSpeechService()
        {
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }

        public async Task SpeakAsync(string text, string languageCode = "en-US")
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text to speak cannot be null or empty", nameof(text));

            try
            {
                // Mapear códigos de idioma de DeepL a códigos de voz
                var voiceLanguage = MapLanguageCodeToVoice(languageCode);
                
                System.Diagnostics.Debug.WriteLine($"Original language code: {languageCode}, Mapped to: {voiceLanguage}");
                
                // Intentar establecer la voz para el idioma
                var voices = _synthesizer.GetInstalledVoices();
                
                if (voices == null || voices.Count == 0)
                {
                    throw new Exception("No text-to-speech voices are installed on this system");
                }
                
                // Buscar una voz que coincida con el idioma
                var voice = voices.FirstOrDefault(v => 
                    v.VoiceInfo.Culture.Name.StartsWith(voiceLanguage, StringComparison.OrdinalIgnoreCase) ||
                    v.VoiceInfo.Culture.TwoLetterISOLanguageName.Equals(voiceLanguage.Split('-')[0], StringComparison.OrdinalIgnoreCase));

                if (voice != null)
                {
                    _synthesizer.SelectVoice(voice.VoiceInfo.Name);
                    System.Diagnostics.Debug.WriteLine($"Selected voice: {voice.VoiceInfo.Name} for culture: {voice.VoiceInfo.Culture.Name}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No voice found for language: {voiceLanguage}, using default");
                    // Usar voz por defecto si no se encuentra una específica
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

                // Configurar velocidad y volumen
                _synthesizer.Rate = 0; // Velocidad normal
                _synthesizer.Volume = 100; // Volumen máximo

                if (text.Length > 1000)
                {
                    text = text.Substring(0, 1000) + "...";
                }

                await Task.Run(() => _synthesizer.Speak(text));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TTS Error: {ex.Message}");
                throw new Exception($"Text-to-speech failed: {ex.Message}");
            }
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
                "ES" => "es-ES",
                "FR" => "fr-FR",
                "DE" => "de-DE",
                "IT" => "it-IT",
                "PT" or "PT-PT" => "pt-PT",
                "PT-BR" => "pt-BR",
                "RU" => "ru-RU",
                "JA" => "ja-JP",
                "ZH" => "zh-CN",
                "KO" => "ko-KR",
                "NL" => "nl-NL",
                "PL" => "pl-PL",
                "SV" => "sv-SE",
                "DA" => "da-DK",
                "NO" => "nb-NO",
                "FI" => "fi-FI",
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
}
