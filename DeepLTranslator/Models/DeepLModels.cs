using Newtonsoft.Json;

namespace DeepLTranslator.Models
{
    public class DeepLTranslationResponse
    {
        [JsonProperty("translations")]
        public List<Translation> Translations { get; set; } = new();
    }

    public class Translation
    {
        [JsonProperty("detected_source_language")]
        public string DetectedSourceLanguage { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class DeepLLanguagesResponse
    {
        [JsonProperty("language")]
        public string Language { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("supports_formality")]
        public bool SupportsFormality { get; set; }
    }

    public class LanguageInfo
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string FlagEmoji { get; set; } = string.Empty;
    }
}
