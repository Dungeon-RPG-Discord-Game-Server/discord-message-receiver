using Microsoft.Extensions.Caching.Memory;

using Discord.WebSocket;
using Discord.Commands;

using DotNetEnv;

using Azure.Identity;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Telemetry.Trace;

using DiscordMessageReceiver.Clients;
using DiscordMessageReceiver.Services;
using DiscordMessageReceiver.Services.Messengers;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var keyVaultUri = new Uri("https://vbn930-rpg-kv.vault.azure.net/");

builder.Configuration.AddAzureKeyVault(
    keyVaultUri,
    new DefaultAzureCredential());

IConfiguration configuration = builder.Configuration;

string? token = configuration["discord-bot-token"];
string? gameServiceBaseUrl = configuration["game-service-base-url"];

string serviceName = configuration["Logging:ServiceName"];
string serviceVersion = configuration["Logging:ServiceVersion"];

builder.Services.AddOpenTelemetry().WithTracing(tcb =>
{
    tcb
    .AddSource(serviceName)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
    .AddAspNetCoreInstrumentation()
    .AddJsonConsoleExporter();
});
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<IDiscordClientManager, DiscordClientManager>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSingleton<ApiKeyManager>(provider=>
{
    var cache = provider.GetRequiredService<IMemoryCache>();
    var httpClient = provider.GetRequiredService<HttpClient>();
    var url = gameServiceBaseUrl;
    return new ApiKeyManager(cache, configuration, url, httpClient);
});
builder.Services.AddSingleton<APIRequestWrapper>();

builder.Services.AddSingleton<BattleMessenger>(provider=>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    var apiWrapper = provider.GetRequiredService<APIRequestWrapper>();
    var url = gameServiceBaseUrl;
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new BattleMessenger(client, apiWrapper, url, configuration);
});
builder.Services.AddSingleton<AdventureMessenger>(provider=>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    var apiWrapper = provider.GetRequiredService<APIRequestWrapper>();
    var url = gameServiceBaseUrl;
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new AdventureMessenger(client, apiWrapper, url, configuration);
});

builder.Services.AddSingleton<GameProgressMessenger>(provider=>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    var apiWrapper = provider.GetRequiredService<APIRequestWrapper>();
    var url = gameServiceBaseUrl;
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new GameProgressMessenger(client, apiWrapper, url, configuration);
});

builder.Services.AddControllers();

var app = builder.Build();
var serviceProvider = app.Services;

app.MapControllers();

var clientManager = app.Services.GetRequiredService<IDiscordClientManager>();
await clientManager.InitClientAsync();
await clientManager.StartClientAsync(token);

var client = app.Services.GetRequiredService<DiscordSocketClient>();
var battle = app.Services.GetRequiredService<BattleMessenger>();
var adventure = app.Services.GetRequiredService<AdventureMessenger>();
var progress = app.Services.GetRequiredService<GameProgressMessenger>();

client.ButtonExecuted += async interaction =>
{
    var id = interaction.Data.CustomId;

    if (id.StartsWith("battle_"))
        await battle.OnButtonExecutedAsync(interaction);
    else if (id.StartsWith("adventure_"))
        await adventure.OnButtonExecutedAsync(interaction);
    else if (id.StartsWith("game_"))
        await progress.OnButtonExecutedAsync(interaction);
};

await app.RunAsync();