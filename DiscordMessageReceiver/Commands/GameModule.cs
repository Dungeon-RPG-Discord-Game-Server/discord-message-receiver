using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Services;
using DiscordMessageReceiver.Services.Messengers;

namespace DiscordMessageReceiver.Commands
{
    public class GameModule : ModuleBase<SocketCommandContext>
    {
        private readonly GameProgressMessenger _gameProgressMessenger;
        private readonly AdventureMessenger _adventureMessenger;
        private readonly BattleMessenger _battleMessenger;

        public GameModule(GameProgressMessenger gameProgressMessenger, AdventureMessenger adventureMessenger, BattleMessenger battleMessenger)
        {
            _gameProgressMessenger = gameProgressMessenger;
            _adventureMessenger = adventureMessenger;
            _battleMessenger = battleMessenger;
        }

        [Command("help")]
        [Summary("Provides help information for game commands.")]
        public async Task HelpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📜 RPG Game Command Help")
                .WithDescription("Here are the available commands to guide your adventure:")
                .WithColor(Color.Blue)
                .AddField("`!menu`", "⚔️ Starts a new game. Use this command to begin a new adventure or continue from a saved game.")
                .AddField("`!save`", "💾 Saves your current game progress. Use this to **permanently store** your adventure state.")
                .AddField("`!help`", "📖 Displays this help message.")
                .WithFooter("Good luck on your journey, adventurer!");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("menu")]
        [Summary("Start the game.")]
        public async Task StartGameAsync()
        {
            await _gameProgressMessenger.StartMainStateAsync(Context.User.Id);
        }

        [Command("save")]
        [Summary("Save the game progress.")]
        public async Task SaveGameAsync()
        {
            string? response = await _gameProgressMessenger.SaveGameAsync(Context.User.Id);
            if (response!=null)
            {
                response = "✅ Your progress has been successfully saved.";
                await ReplyAsync(response);
                return;
            }else
            {
                await ReplyAsync("⚠️ Unable to connect to the game service. Please try again later.");
                return;
            }
        }
    }
}