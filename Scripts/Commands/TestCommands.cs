using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SwippyBot
{
    public class TestCommands : BaseCommandModule
    {
        /*[Command("test")]
        [Description("I dwo testings")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Ewwo i ish guds ^-^").ConfigureAwait(false);
        }

        [Command("test2")]
        [Description("I dwo testings")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Test2(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Ewwos i ish amazings!!!!! >w<").ConfigureAwait(false);
        }

        [Command("hungy")]
        public async Task Hungy(CommandContext ctx)
        {
            await ctx.Channel.DeleteMessageAsync(ctx.Message);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Ish chu hungy?",
            };

            List<DiscordButtonComponent> buttons = new List<DiscordButtonComponent> {
                new DiscordButtonComponent(ButtonStyle.Danger, "hungy", "Yesh :("),
                new DiscordButtonComponent(ButtonStyle.Success, "nwot_hungy", "Nu ^-^")
            };

            await Prebuilt.MessageWithButtons(ctx, embed, buttons, TimeSpan.FromMinutes(1), async (client, e) =>
                {
                    if (e.Id == "hungy")
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = "Hav cookie :cookie:",
                            Description = $"{e.User.Mention} took cookie :p"
                        }));
                    }
                    else
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Title = "Okie ^-^",
                            Description = $"{e.User.Mention} ish nwots hungies"
                        }));
                    }
                });
        }

        [Command("mafs")]
        [Description("MAFS!!!! UwU")]
        public async Task Mafs(CommandContext ctx, [Description("Numbew on")] float num1, [Description("+ ow - ow * ow / ow ^")] string op, [Description("Numbew two")] float num2)
        {
            switch (op)
            {
                case "+":
                    await ctx.Channel.SendMessageAsync((num1 + num2).ToString()).ConfigureAwait(false);
                    break;
                case "-":
                    await ctx.Channel.SendMessageAsync((num1 - num2).ToString()).ConfigureAwait(false);
                    break;
                case "*":
                    await ctx.Channel.SendMessageAsync((num1 * num2).ToString()).ConfigureAwait(false);
                    break;
                case "/":
                    await ctx.Channel.SendMessageAsync((num1 / num2).ToString()).ConfigureAwait(false);
                    break;
                case "^":
                    await ctx.Channel.SendMessageAsync(MathF.Pow(num1, num2).ToString()).ConfigureAwait(false);
                    break;
                default:
                    await ctx.Channel.SendMessageAsync("O.O Wat dats?").ConfigureAwait(false);
                    break;
            }
        }

        [Command("inttest")]
        [Description("Interactivity test")]
        //[RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task IntTest(CommandContext ctx)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            DiscordMessage message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel).ConfigureAwait(false)).Result;

            try
            {
                await ctx.Channel.SendMessageAsync(message.Content.Replace(message.MentionedUsers[0].Id.ToString(), message.Author.Id.ToString())).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                await Prebuilt.Error(ctx.Channel, ex);
            }
        }

        [Command("tj")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        //[RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task TestJoin(CommandContext ctx)
        {
            DiscordMessage msg = null;

            try
            {
                DiscordEmbed embed = new DiscordEmbedBuilder
                {
                    Title = "Some title here",
                    ImageUrl = (await ctx.Guild.GetMemberAsync(341316396497764355)).GuildAvatarUrl,
                    Color = DiscordColor.DarkBlue
                };

                msg = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                DiscordEmoji like = DiscordEmoji.FromName(ctx.Client, ":+1:");
                DiscordEmoji dislike = DiscordEmoji.FromName(ctx.Client, ":-1:");

                await msg.CreateReactionAsync(like).ConfigureAwait(false);
                await msg.CreateReactionAsync(dislike).ConfigureAwait(false);

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                InteractivityResult<MessageReactionAddEventArgs> result = await interactivity.WaitForReactionAsync(x => x.Message == msg && !x.User.IsBot && (x.Emoji == like || x.Emoji == dislike)).ConfigureAwait(false);

                if (result.Result.Emoji == like)
                {
                    DiscordRole role = ctx.Guild.GetRole(918959314713059348);
                    await ((DiscordMember)result.Result.User).GrantRoleAsync(role).ConfigureAwait(false);
                    //await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
                }
                else if (result.Result.Emoji == dislike)
                {
                    DiscordRole role = ctx.Guild.GetRole(918959314713059348);
                    await ((DiscordMember)result.Result.User).RevokeRoleAsync(role).ConfigureAwait(false);
                }
            }
            catch (System.Exception ex)
            {
                await ctx.Channel.SendMessageAsync($"ERROR: {ex.Message}{Environment.NewLine}{ex.StackTrace}").ConfigureAwait(false);
            }

            await ctx.Channel.DeleteMessageAsync(msg).ConfigureAwait(false);
        }

        [Command("gg")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task GetGae(CommandContext ctx)
        {
            try
            {
                DiscordRole role = ctx.Guild.GetRole(918959314713059348);
                await ctx.Member.GrantRoleAsync(role).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                await Prebuilt.Error(ctx.Channel, ex);
            }
        }

        [Command("nohomo")]
        [RequireBotPermissions(DSharpPlus.Permissions.ManageRoles)]
        public async Task NoHomo(CommandContext ctx)
        {
            try
            {
                DiscordRole role = ctx.Guild.GetRole(918959314713059348);
                await ctx.Member.RevokeRoleAsync(role).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                await Prebuilt.Error(ctx.Channel, ex);
            }
        }

        [Command("poll")]
        public async Task Poll(CommandContext ctx, string title, string description, TimeSpan duration, params DiscordEmoji[] reactions)
        {
            await ctx.Channel.DeleteMessageAsync(ctx.Message);

            if (duration.TotalSeconds <= 0)
            {
                await ctx.Channel.SendMessageAsync($"POLL ERROR: Invalid duration of: {duration.Seconds}s").ConfigureAwait(false);
                return;
            }

            if (reactions.Length == 0)
            {
                await ctx.Channel.SendMessageAsync($"POLL ERROR: No reactions").ConfigureAwait(false);
                return;
            }

            DiscordMessage msg = null;
            Task readyMsgTask = null;

            try
            {
                title = title.Replace("_", " ");
                description = description.Replace("_", " ");

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Title = $"Poll: {title}",
                    Description = description,
                    Color = DiscordColor.Purple
                };

                msg = await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
                foreach (DiscordEmoji reaction in reactions)
                    await msg.CreateReactionAsync(reaction).ConfigureAwait(false);

                await Task.Delay(1000);
                readyMsgTask = Prebuilt.TimedMessage(ctx.Channel, TimeSpan.FromSeconds(4), new DiscordMessageBuilder { Content = "Poll ready! ^-^" });
                IEnumerable<Reaction> reactionCounts = (await interactivity.CollectReactionsAsync(msg, duration).ConfigureAwait(false)).Distinct();

                //Results
                Dictionary<string, int> results = new Dictionary<string, int>();
                for (int i = 0; i < reactions.Length; i++)
                {
                    results.Add(reactions[i].ToString(), 0);
                    foreach (var userReaction in reactionCounts)
                    {
                        //await ctx.Channel.SendMessageAsync(userReaction.Emoji.ToString() + " " + userReaction.Total).ConfigureAwait(false);
                        if (userReaction.Emoji.ToString() == reactions[i].ToString())
                            results[reactions[i].ToString()]++;
                    }
                }

                StringBuilder sb_results = new StringBuilder();
                foreach (var result in results.Keys)
                    sb_results.AppendLine($"{result} {results[result]} {Environment.NewLine}");

                embed = new DiscordEmbedBuilder
                {
                    Title = $"Poll \"{title}\" results",
                    Description = $"{sb_results}",
                    Color = DiscordColor.Purple
                };

                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                if (ex.Message.Contains("Bad request: 400"))
                    await ctx.Channel.SendMessageAsync($"POLL ERROR: Please avoid using server specific emojis for reactions").ConfigureAwait(false);
                else
                    await Prebuilt.Error(ctx.Channel, ex);
            }

            if (msg != null)
                await ctx.Channel.DeleteMessageAsync(msg);

            if (readyMsgTask != null)
                await readyMsgTask;
        }

        [Command("savedata")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SaveData(CommandContext ctx)
        {
            Bot.GetSeverData(ctx.Guild.Id).WriteToDisk();
            await ctx.Channel.SendMessageAsync("Data saved").ConfigureAwait(false);
        }

        [Command("get")]
        public async Task Get(CommandContext ctx, string key)
        {
            string value = Bot.GetSeverData(ctx.Guild.Id).GetUserValue(ctx.User.Id, key);
            if (string.IsNullOrWhiteSpace(value))
                value = "[EMPTY]";
            await ctx.Channel.SendMessageAsync($"Value at key \"{key}\": {value}");
        }

        [Command("set")]
        public async Task Set(CommandContext ctx, string key, string value)
        {
            Bot.GetSeverData(ctx.Guild.Id).SetUserValue(ctx.User.Id, key, value);
            await ctx.Channel.SendMessageAsync($"Value with key \"{key}\" set to: {value}").ConfigureAwait(false);
        }
        
        [Command("test")]
        [Description("")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Test(CommandContext ctx, ulong emojiID)
        {
            try
            {
                DiscordEmoji e;

                e = DiscordEmoji.FromGuildEmote(ctx.Client, emojiID);
                await ctx.Channel.SendMessageAsync($"<{(e.IsAnimated ? "a" : "")}:{e.Name}:{e.Id}>");
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("test2")]
        [Description("")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Test2(CommandContext ctx, DiscordEmoji emoji)
        {
            try
            {
                await ctx.Channel.SendMessageAsync($"{emoji.Name} {emoji.Id}");
                await ctx.Channel.SendMessageAsync($"<{(emoji.IsAnimated ? "a" : "")}:{emoji.Name}:{emoji.Id}>");
                await ctx.Channel.SendMessageAsync($"{emoji.Url}");
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("e")]
        [Description("")]
        public async Task Emoji(CommandContext ctx, ulong emojiID)
        {
            try
            {
                await ctx.Message.DeleteAsync();
                DiscordEmoji e = DiscordEmoji.FromGuildEmote(ctx.Client, emojiID);
                await ctx.Channel.SendMessageAsync($"{e.Url}");
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("el")]
        [Description("")]
        public async Task EmojiList(CommandContext ctx, ulong serverID)
        {
            try
            {
                await ctx.Message.DeleteAsync();
                var emojis = (await (await ctx.Client.GetGuildAsync(serverID)).GetEmojisAsync());
                StringBuilder sb = new StringBuilder();

                foreach (var emoji in emojis)
                {
                    sb.AppendLine($"<{(emoji.IsAnimated ? "a" : "")}:{emoji.Name}:{emoji.Id}>  {emoji.Id}");
                    if (sb.Length > 1900)
                    {
                        await ctx.Channel.SendMessageAsync(sb.ToString());
                        sb.Clear();
                    }
                }
                await ctx.Channel.SendMessageAsync(sb.ToString());
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
        */
    }
}

