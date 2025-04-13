using Discord;
using Discord.Interactions;

namespace DiscordMessageReceiver.Commands;
public class GameSlashModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("start", "Start a new game adventure.")]
    public async Task StartGame()
    {
        await DeferAsync(ephemeral: true);

        var dmChannel = await Context.User.CreateDMChannelAsync();
        await dmChannel.SendMessageAsync("⚔️ Your journey begins! Type !start to enter the world of adventure! 🌍");

        await FollowupAsync("💌 Check your DMs! Your journey begins there.");
    }

    [SlashCommand("help", "Displays a list of available game commands.")]
    public async Task HelpAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("📜 RPG Game Command Help")
            .WithColor(Color.DarkPurple)
            .WithDescription("All game actions are performed in **Direct Messages (DMs)**.\nUse the commands below to manage your adventure:")
            .AddField("/start", "💌 Sends a DM to start your journey.")
            .AddField("`/help`", "📖 Displays this help message.")
            .AddField("`!menu`", "⚔️ Starts a new game. Use this command to begin a new adventure or continue from a saved game. *(DM only)*")
            .AddField("`!save`", "💾 Saves your current game progress. Use this to **permanently store** your adventure state. *(DM only)*")
            .AddField("`!help`", "📖 Displays this help message. *(DM only)*")
            .WithFooter("Your legend begins now. Choose your path wisely...");

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

}