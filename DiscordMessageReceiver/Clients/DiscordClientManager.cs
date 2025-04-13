using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Interactions;
using System.Reflection;

using DiscordMessageReceiver.Utils;

namespace DiscordMessageReceiver.Clients
{
    public class DiscordClientManager : IDiscordClientManager
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;

        public DiscordClientManager(
            DiscordSocketClient client,
            CommandService commands,
            InteractionService interactions,
            IServiceProvider services,
            IConfiguration configuration)
        {
            _client = client;
            _commands = commands;
            _interactions = interactions;
            _services = services;
            _configuration = configuration;
            if (null == _configuration)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            string serviceName = configuration["Logging:ServiceName"];
            _logger = new Logger(serviceName);

            _client.Log += LogAsync;
            _client.MessageReceived += MessageReceivedAsync;
            _client.Ready += ReadyAsync;
            _client.InteractionCreated += HandleInteractionAsync;
        }

        public async Task InitClientAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
            await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
        private async Task ReadyAsync()
        {
            await _interactions.RegisterCommandsGloballyAsync(true);
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            using var log = _logger.StartMethod(nameof(HandleInteractionAsync));
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                log.HandleException(ex);
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    await interaction.RespondAsync("⚠️ 슬래시 명령어 처리 중 오류가 발생했습니다.", ephemeral: true);
                }
            }
        }

        public async Task StartClientAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        public async Task StopClientAsync()
        {
            await _client.StopAsync();
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            var log = _logger.StartMethod(nameof(LogAsync));
            log.SetAttribute("client.log", logMessage.ToString());
            log.Dispose();
        }

        private async Task MessageReceivedAsync(SocketMessage messageParam)
        {
            using (var log = _logger.StartMethod(nameof(MessageReceivedAsync)))
            {
                try
                {
                    if (!(messageParam is SocketUserMessage message)) return;
                    if (message.Author.IsBot) return;

                    log.SetAttribute("client.received-message", message.Content);

                    int argPos = 0;
                    if (!message.HasCharPrefix('!', ref argPos))
                    {
                        throw new Exception("Prefix not found.");
                    }

                    var context = new SocketCommandContext(_client, message);
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess)
                    {
                        log.SetAttribute("client.error", result.ErrorReason);
                    }
                }
                catch (UserErrorException e)
                {
                    log.LogUserError(e.Message);
                }
                catch (Exception e)
                {
                    log.HandleException(e);
                }
            }
        }
    }
}
