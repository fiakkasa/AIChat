namespace AIChat.Services;

public sealed partial class ChatItemInteractionService(
    IHttpClientFactory httpClientFactory
) : IChatItemInteractionService, IDisposable
{
    private const int _streamingInitialDelay = 375;
    private const int _streamingUpdateDelay = 125;
    private const int _streamingCyclesCommitCount = 10;
    private static readonly Regex _newLineRegex = NewLineRegex();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HttpClient? _client;

    private HttpClient Client =>
        _client ??= httpClientFactory.CreateClient(Consts.AiChatClientName);

    public async ValueTask HandleStreamingRequest(
        ChatItem chatItem,
        AiChatConfig config,
        Action? progressNotifier = default,
        CancellationToken cancellationToken = default
    )
    {
        var response = await SendHttpRequest(config, chatItem.Question, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        await NotifyProgress(progressNotifier, _streamingInitialDelay, cancellationToken);

        var refreshCounter = 0L;
        var answer = new StringBuilder();

        while (!reader.EndOfStream)
        {
            var rawItem = await reader.ReadLineAsync(cancellationToken);

            if (
                rawItem?.Replace("data:", string.Empty).Trim() is not { Length: > 0 } item
                || item.Contains("[DONE]", StringComparison.InvariantCultureIgnoreCase)
                || JsonSerializer.Deserialize<ChatStreamResponse>(item, _jsonOptions) is not
                    { Choices.Length: > 0 } chatStreamedItem
                || chatStreamedItem.Choices.Length switch
                {
                    > 1 =>
                        string.Join(
                            string.Empty,
                            chatStreamedItem
                                .Choices
                                .Where(x => x is { Message.Content.Length: > 0 })
                                .Select(x => x.Message!.Content)
                        ),
                    _ => chatStreamedItem.Choices[0].Message?.Content
                } is not { Length: > 0 } answerFragment
            )
            {
                continue;
            }

            answer.Append(answerFragment);

            if (refreshCounter++ % _streamingCyclesCommitCount != 0)
            {
                continue;
            }

            chatItem.Answer = answer.ToString();
            chatItem.IsMultiline = _newLineRegex.IsMatch(chatItem.Answer);

            await NotifyProgress(progressNotifier, _streamingUpdateDelay, cancellationToken);
        }

        chatItem.Answer = answer.ToString().Trim();
        chatItem.IsMultiline = _newLineRegex.IsMatch(chatItem.Answer);

        await NotifyProgress(progressNotifier, _streamingUpdateDelay, cancellationToken);
    }

    public async ValueTask HandlePlainRequest(
        ChatItem chatItem,
        AiChatConfig config,
        Action? progressNotifier = default,
        CancellationToken cancellationToken = default
    )
    {
        var response = await SendHttpRequest(config, chatItem.Question, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<ChatResponse>(_jsonOptions, cancellationToken);

        var answer = result switch
        {
            { Choices: { Length: > 1 } choices } =>
                string
                    .Join(
                        string.Empty,
                        choices
                            .Where(x => x is { Message.Content.Length: > 0 })
                            .Select(x => x.Message!.Content)
                    )
                    .Trim(),
            { Choices: { Length: 1 } choices } => choices[0].Message.Content.Trim(),
            _ => string.Empty
        };

        chatItem.Answer = answer;
        chatItem.IsMultiline = _newLineRegex.IsMatch(chatItem.Answer);

        progressNotifier?.Invoke();
    }

    public void Dispose() => _client?.Dispose();

    private async ValueTask<HttpResponseMessage> SendHttpRequest(
        AiChatConfig config,
        string question,
        CancellationToken cancellationToken
    )
    {
        var response = await Client!.PostAsJsonAsync(
            config.ChatCompletionsUrlFragment,
            new ChatRequest(
                [new(Content: question, Role: config.Role)],
                config.Model,
                config.MaxTokens,
                config.Stream
            ),
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        return response;
    }

    private static async ValueTask NotifyProgress(
        Action? progressNotifier,
        int delay,
        CancellationToken cancellationToken)
    {
        if (progressNotifier is null)
        {
            return;
        }

        await Task.Delay(delay, cancellationToken);
        progressNotifier.Invoke();
    }

    [GeneratedRegex(@"\n|\r\n")]
    private static partial Regex NewLineRegex();
}
