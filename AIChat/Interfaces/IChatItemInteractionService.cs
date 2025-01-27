namespace AIChat.Interfaces;

public interface IChatItemInteractionService
{
    public ValueTask HandleStreamingRequest(
        ChatItem chatItem,
        AiChatConfig config,
        Action? progressNotifier = default,
        CancellationToken cancellationToken = default
    );

    public ValueTask HandlePlainRequest(
        ChatItem chatItem,
        AiChatConfig config,
        Action? progressNotifier = default,
        CancellationToken cancellationToken = default
    );
}
