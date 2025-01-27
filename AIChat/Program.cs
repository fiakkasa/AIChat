using AIChat;
using AIChat.Components;
using AIChat.Services;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var isDev = builder.Environment.IsDevelopment();

services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddResponseCompression();

services
    .AddOptions<AiChatConfig>()
    .BindConfiguration(Consts.AiChatConfigKey)
    .ValidateDataAnnotations()
    .ValidateOnStart();
services
    .AddHttpClient(
        Consts.AiChatClientName,
        (sp, options) =>
        {
            var config = sp.GetRequiredService<IOptionsMonitor<AiChatConfig>>().CurrentValue;
            options.BaseAddress = config.BaseUri;
        }
    )
    .AddPolicyHandler(
        (sp, _) =>
        {
            var config = sp.GetRequiredService<IOptionsMonitor<AiChatConfig>>().CurrentValue;

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(config.WaitAndRetryIntervals);
        }
    );
services.AddTransient<IChatItemInteractionService, ChatItemInteractionService>();

var app = builder.Build();

app.UseStatusCodePagesWithRedirects(Consts.NotFoundPageUrl);

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (!isDev)
{
    app.UseExceptionHandler(Consts.ErrorPageUrl, true);
}

app.UseStaticFiles();
app.UseAntiforgery();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
