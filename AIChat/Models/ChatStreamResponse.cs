namespace AIChat.Models;

public record ChatStreamResponse(ChatStreamChoice[] Choices, string Model);
