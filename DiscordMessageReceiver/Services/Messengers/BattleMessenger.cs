using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using DiscordMessageReceiver.Dtos;
using DiscordMessageReceiver.Services;

namespace DiscordMessageReceiver.Services.Messengers{
    public class BattleMessenger : BaseMessenger
    {
        public BattleMessenger(DiscordSocketClient client, APIRequestWrapper apiWrapper, string gameServiceBaseUrl) : base(client, apiWrapper, gameServiceBaseUrl)
        {
        }

        public async Task<bool> BossClearedAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"battle/{userId}/boss");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저의 보스 클리어 요청에 실패하였습니다: {userId}");
                return false;
            }

            var status = JsonSerializerWrapper.Deserialize<bool>(response);
            if (status)
            {
                Console.WriteLine($"✅ 유저가 보스를 클리어 하였습니다: {userId}");
            }
            else
            {
                Console.WriteLine($"❌ 유저가 보스를 클리어 하지 못했습니다: {userId}");
            }

            return status;
        }
        public async Task<bool> MonsterExistsAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"battle/{userId}/monster");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저의 몬스터 존재 여부 요청에 실패하였습니다: {userId}");
                return false;
            }

            var status = JsonSerializerWrapper.Deserialize<bool>(response);
            if (status)
            {
                Console.WriteLine($"✅ 유저에게 몬스터가 존재합니다: {userId}");
            }
            else
            {
                Console.WriteLine($"❌ 유저에게 몬스터가 존재하지 않습니다: {userId}");
            }

            return status;
        }

        public async Task<string> AttackAsync(ulong userId, bool skillUsed)
        {
            var response = await _apiWrapper.PostAsync(_gameServiceBaseUrl + $"battle/{userId}/attack?skillUsed={skillUsed.ToString().ToLower()}");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저의 공격 요청에 실패하였습니다: {userId}");
                return string.Empty;
            }

            var result = response;
            if (result == null)
            {
                Console.WriteLine($"❌ 유저의 공격 결과를 가져오는 데 실패했습니다: {userId}");
                return string.Empty;
            }

            return result;
        }

        public async Task<string> MonsterAttackAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"battle/{userId}/monster-attack");
            if (response == null)
            {
                Console.WriteLine($"❌ 몬스터의 공격이 실패하였습니다: {userId}");
                return string.Empty;
            }

            var result = response;
            if (result == null)
            {
                Console.WriteLine($"❌ 몬스터의 공격 결과를 가져오는 데 실패했습니다: {userId}");
                return string.Empty;
            }

            return result;
        }

        public async Task<BattleEscapeResultDto> RunAwayAsync(ulong userId)
        {
            var response = await _apiWrapper.GetAsync(_gameServiceBaseUrl + $"battle/{userId}/run");
            if (response == null)
            {
                Console.WriteLine($"❌ 유저의 도망 요청에 실패하였습니다: {userId}");
                return new BattleEscapeResultDto { IsEscaped = false, Message = "Failed to escape." };
            }

            var result = JsonSerializerWrapper.Deserialize<BattleEscapeResultDto>(response);
            if (result == null)
            {
                Console.WriteLine($"❌ 유저의 도망 결과를 가져오는 데 실패했습니다: {userId}");
                return new BattleEscapeResultDto { IsEscaped = false, Message = "Failed to escape." };
            }

            return result;
        }

        /// <summary>
        /// 유저에게 버튼이 포함된 공격 타입입 선택지 메시지를 DM으로 보냅니다.
        /// </summary>
        public async Task SendAttackChoiceButtonsAsync(ulong userId)
        {
            await SendMessageAsync(userId, "⚔️ What type of attack would you like to use?", new ComponentBuilder()
                .WithButton("🗡 Normal Attack", "battle_normal_attack", ButtonStyle.Primary)
                .WithButton("✨ Skill Attack", "battle_skill_attack", ButtonStyle.Success));
        }

        /// <summary>
        /// 버튼 클릭 시 호출되는 이벤트 핸들러
        /// </summary>
        public override async Task OnButtonExecutedAsync(SocketMessageComponent interaction)
        {
            var user = interaction.User;
            var customId = interaction.Data.CustomId;

            // 1. 메시지 수정 내용 준비
            string content = customId switch
            {
                "battle_attack"   => "⚔ You have selected **Attack**.\nPreparing your weapon...",
                "battle_run"      => "🏃 You have selected **Run**.\nAttempting to escape...",
                "battle_normal_attack"   => "🗡 You have selected **Normal Attack**.\nReady to strike!",
                "battle_skill_attack"    => "✨ You have selected **Skill Attack**.\nUnleashing your special ability!",
                _                 => $"❌ You have selected an unknown option: **{customId}**.\nPlease try again."
            };

            // 2. 버튼 제거하고 메시지 수정
            await interaction.UpdateAsync(msg =>
            {
                msg.Content = content;
                msg.Components = new ComponentBuilder().Build();
            });

            string result = string.Empty;
            string monsterAttackResult;
            bool monsterAttack = false;

            switch (interaction.Data.CustomId)
            {
                case "battle_attack":
                    await SendAttackChoiceButtonsAsync(user.Id);
                    break;
                case "battle_run":  
                    await SendMessageAsync(user.Id, "🏃 You are attempting to escape the battle.");
                    var escapeResult = await RunAwayAsync(user.Id);
                    result = escapeResult.Message;
                    monsterAttack = !escapeResult.IsEscaped;
                    break;
                case "battle_normal_attack":    
                    await SendMessageAsync(user.Id, "🗡 You are using a normal attack.");
                    result = await AttackAsync(user.Id, false);
                    monsterAttack = await MonsterExistsAsync(user.Id);
                    break;
                case "battle_skill_attack":
                    await SendMessageAsync(user.Id, "✨ You are using a skill attack.");
                    result = await AttackAsync(user.Id, true);
                    monsterAttack = await MonsterExistsAsync(user.Id);
                    break;
                default:
                    await SendMessageAsync(user.Id, "❌ Unknown action.");
                    break;
            }

            //만약 게임 스테이트가 배틀이면 배틀 실행
            if (interaction.Data.CustomId != "battle_attack")
            {
                await SendMessageAsync(user.Id, result);
                if (monsterAttack)
                {
                    monsterAttackResult = await MonsterAttackAsync(user.Id);
                    await SendMessageAsync(user.Id, monsterAttackResult);
                }

                var gameState = await GetPlayerGameStateAsync(user.Id);
                switch (gameState)
                {
                    case "MainMenuState":
                        string message = $@"
                        ☠️ The monster’s blow was fatal...  
                        You fall, but legends never die.  
                        🌟 A new destiny awaits — your story starts again.
                        ".Trim();
                        await SendMessageAsync(user.Id, message);
                        await StartMainStateAsync(user.Id);
                        break;
                    case "ExplorationState":
                        bool bossCleared = await BossClearedAsync(user.Id);
                        if (bossCleared)
                        {
                            await SendMessageAsync(user.Id, "🏰 You have cleared the boss and now entering to the new dungeon.");
                            await EnterDungeonAsync(user.Id);
                        }
                        else
                        {
                            await ContiueExplorationAsync(user.Id);
                        }
                        break;
                    case "BattleState":
                        await ContiueBattleAsync(user.Id);
                        break;
                    default:
                        await SendMessageAsync(user.Id, "❌ Unknown game state.");
                        break;
                }
            }
        }
    }
}
