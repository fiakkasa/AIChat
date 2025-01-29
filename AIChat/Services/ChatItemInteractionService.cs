using System.Runtime.CompilerServices;

namespace AIChat.Services;

public sealed class ChatItemInteractionService(
    IHttpClientFactory httpClientFactory,
    ILogger<ChatItemInteractionService> logger
) : IChatItemInteractionService, IDisposable
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private HttpClient? _client;

    private HttpClient Client =>
        _client ??= httpClientFactory.CreateClient(Consts.AiChatClientName);

    public async IAsyncEnumerable<string> HandleStreamingRequest(
        string question,
        AiChatConfig config,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var response = await SendHttpRequest(config, question, cancellationToken);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var paddedJsonLike = await reader.ReadLineAsync(cancellationToken) ?? string.Empty;

            if (paddedJsonLike.Contains("[DONE]", StringComparison.InvariantCultureIgnoreCase))
            {
                break;
            }

            if (paddedJsonLike.Replace("data:", string.Empty).Trim() is not { Length: > 0 } jsonLike)
            {
                continue;
            }

            var chatChoices =
                Deserialize<ChatStreamResponse>(jsonLike)
                    ?.Choices
                    .Where(x => x is { Message.Content.Length: > 0 });

            foreach (var chatChoice in chatChoices ?? [])
            {
                yield return chatChoice.Message!.Content;
            }
        }
    }

    public async IAsyncEnumerable<string> HandlePlainRequest(
        string question,
        AiChatConfig config,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        var response = await SendHttpRequest(config, question, cancellationToken);

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var chatChoices =
            Deserialize<ChatResponse>(json)
                ?.Choices
                .Where(x => x is { Message.Content.Length: > 0 });

        foreach (var chatChoice in chatChoices ?? [])
        {
            yield return chatChoice.Message.Content;
        }
    }

    public void Dispose() => _client?.Dispose();

    private T? Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize JSON: '{Json}' for type '{Type}'", json, typeof(T).Name);
        }

        return default;
    }

    private async ValueTask<HttpResponseMessage> SendHttpRequest(
        AiChatConfig config,
        string question,
        CancellationToken cancellationToken
    )
    {
        var response = await Client.PostAsJsonAsync(
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
}
