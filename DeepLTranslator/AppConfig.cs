using System.Drawing;

namespace DeepLTranslator
{
    public static class AppConfig
    {
        private const string _defaultApiKey = "2afcf527-da2a-4209-9fd3-cdb7d68242f7:fx";
        
        public static string DeepLApiKey 
        { 
            get 
            {
                // Try to get API key from environment variable first
                var envApiKey = Environment.GetEnvironmentVariable("DEEPL_API_KEY");
                if (!string.IsNullOrWhiteSpace(envApiKey))
                    return envApiKey;
                
                // Fall back to default key (should be replaced in production)
                return _defaultApiKey;
            }
        }
        
        public const string AppName = "DeepL Translator";
        public const string AppVersion = "1.0.0";
        
        public const int MaxTextLength = 5000;
        public const int ApiTimeoutSeconds = 30;
        public const int MaxRetries = 3;
        
        // Configuraciones de UI
        public static readonly Color PrimaryColor = Color.FromArgb(13, 110, 253);
        public static readonly Color SecondaryColor = Color.FromArgb(108, 117, 125);
        public static readonly Color SuccessColor = Color.FromArgb(25, 135, 84);
        public static readonly Color WarningColor = Color.FromArgb(255, 193, 7);
        public static readonly Color ErrorColor = Color.FromArgb(220, 53, 69);
        public static readonly Color BackgroundColor = Color.FromArgb(248, 249, 250);
        public static readonly Color TextColor = Color.FromArgb(33, 37, 41);
        public static readonly Color MutedTextColor = Color.FromArgb(73, 80, 87);
        
        // Configuraciones de fuente
        public static readonly Font TitleFont = new("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font HeaderFont = new("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font BodyFont = new("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font ButtonFont = new("Segoe UI", 9F, FontStyle.Regular);
        
        public static bool ValidateApiKey()
        {
            var apiKey = DeepLApiKey;
            return !string.IsNullOrWhiteSpace(apiKey) && 
                   apiKey.Contains(':') && 
                   apiKey.Length > 10;
        }
        
        public static bool IsValidApiKey(string apiKey)
        {
            return !string.IsNullOrWhiteSpace(apiKey) && 
                   apiKey.Contains(':') && 
                   apiKey.Length > 10;
        }
    }
}
