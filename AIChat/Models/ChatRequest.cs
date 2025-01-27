using System.Text.Json.Serialization;

namespace AIChat.Models;

public record ChatRequest(
    ChatMessage[] Messages,
    string Model,
    [property: JsonPropertyName("max_tokens")]
    int MaxTokens,
    bool Stream
);
