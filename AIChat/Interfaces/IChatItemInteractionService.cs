namespace AIChat.Interfaces;

public interface IChatItemInteractionService
{
    public IAsyncEnumerable<string> HandleStreamingRequest(
        string question,
        AiChatConfig config,
        CancellationToken cancellationToken = default
    );

    public IAsyncEnumerable<string> HandlePlainRequest(
        string question,
        AiChatConfig config,
        CancellationToken cancellationToken = default
    );
}
