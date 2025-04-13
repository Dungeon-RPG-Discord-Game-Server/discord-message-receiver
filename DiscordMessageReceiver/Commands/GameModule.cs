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

        [Command("game")]
        [Summary("Send a DM to the user to start the game.")]
        public async Task OpenDMAsync()
        {
            var dmChannel = await Context.User.CreateDMChannelAsync();
            
            await dmChannel.SendMessageAsync("⚔️ Your journey begins! Type !start to enter the world of adventure! 🌍");
            await ReplyAsync("💌 Check your DMs! Your journey begins there.");
        }

        [Command("start")]
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