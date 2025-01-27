using System.Text.Json.Serialization;

namespace AIChat.Models;

public record ChatStreamChoice(
    [property: JsonPropertyName("delta")]
    ChatMessage? Message
);
