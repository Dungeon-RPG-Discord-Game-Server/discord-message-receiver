using Discord.WebSocket;
using DiscordMessageReceiver.Clients;
using DiscordMessageReceiver.Services;
using DiscordMessageReceiver.Services.Messengers;
using Discord.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;

Env.Load();
string? token = Environment.GetEnvironmentVariable("BOT_TOKEN");
string? gameServiceBaseUrl = Environment.GetEnvironmentVariable("GAME_SERVICE_BASE_URL");
gameServiceBaseUrl = "http://127.0.0.1:5048/api/";

var builder = WebApplication.CreateBuilder(args);

// 서비스 등록 (DI)
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<IDiscordClientManager, DiscordClientManager>();
builder.Services.AddSingleton<APIRequestWrapper>();
builder.Services.AddSingleton<HttpClient>();

builder.Services.AddSingleton<GameProgressMessenger>(provider=>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    var apiWrapper = provider.GetRequiredService<APIRequestWrapper>();
    var url = gameServiceBaseUrl;
    return new GameProgressMessenger(client, apiWrapper, url);
});
builder.Services.AddSingleton<BattleMessenger>(provider=>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    var apiWrapper = provider.GetRequiredService<APIRequestWrapper>();
    var url = gameServiceBaseUrl;
    return new BattleMessenger(client, apiWrapper, url);
});
builder.Services.AddSingleton<AdventureMessenger>(provider=>
{
    var client = provider.GetRequiredService<DiscordSocketClient>();
    var apiWrapper = provider.GetRequiredService<APIRequestWrapper>();
    var url = gameServiceBaseUrl;
    return new AdventureMessenger(client, apiWrapper, url);
});

// 여기에 컨트롤러 추가도 가능
builder.Services.AddControllers(); // <- REST API 만들고 싶으면 필요

var app = builder.Build();
var serviceProvider = app.Services;

// Web API 라우팅
app.MapControllers();

// 디스코드 봇 실행
var clientManager = app.Services.GetRequiredService<IDiscordClientManager>();
await clientManager.InitClientAsync();
await clientManager.StartClientAsync(token);

// 실행
await app.RunAsync();