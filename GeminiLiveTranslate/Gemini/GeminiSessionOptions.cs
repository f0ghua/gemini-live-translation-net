namespace GeminiLiveTranslate.Gemini;

public sealed record GeminiSessionOptions(
    string ApiKey,
    string ApiBase,
    string ProxyUrl,
    string Model,
    string TargetLanguage,
    string SystemPrompt,
    bool EchoTargetLanguage);
