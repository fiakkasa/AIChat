using System.ComponentModel.DataAnnotations;

namespace AIChat.Models;

public record AiChatConfig
{
    public required Uri BaseUri { get; init; }

    [StringLength(256, MinimumLength = 1)]
    [RegularExpression(@"^[a-zA-Z][a-zA-Z0-9\/\-]+")]
    public required string ChatCompletionsUrlFragment { get; init; }

    [StringLength(256, MinimumLength = 1)]
    [RegularExpression(@"[a-zA-Z0-9\-\.]+")]
    public required string Model { get; init; }

    [StringLength(256, MinimumLength = 1)]
    [RegularExpression(@"[a-zA-Z0-9\-]+")]
    public required string Role { get; init; }

    [Range(2, 8192)]
    public int MaxTokens { get; init; }

    public bool Stream { get; init; }

    [MinLength(1)]
    public TimeSpan[] WaitAndRetryIntervals { get; init; } = [];
}
