using AIChat.Enums;

namespace AIChat.Models;

public record ChatItem
{
    public string Question { get; init; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public bool IsMultiline { get; set; }
    public string Model { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public ChatItemStatusType Status { get; set; }
}
