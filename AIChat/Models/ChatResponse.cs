namespace AIChat.Models;

public record ChatResponse(ChatChoice[] Choices, string Model);
