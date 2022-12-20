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
    public class RoleReactionCommands : BaseCommandModule
    {
        [Command("create_role_reaction_message")]
        [Description("Create an embed message in the specified channel where users can react to get roles")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task CreateMessage(CommandContext ctx, DiscordChannel messageChannel)
        {
            try
            {
                if (messageChannel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                string title;
                List<Tuple<DiscordRole, DiscordEmoji>> rolesAndEmojis = new List<Tuple<DiscordRole, DiscordEmoji>>();

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                //Title
                DiscordMessage instructionMessage = await ctx.Channel.SendEmbedMessage("Please enter the title of the reaction message embed", "Type \".cancel\" to cancel this command", Bot.ColorMain);
                DiscordMessage message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && !x.Author.IsBot && (x.Content.ToLower() == ".cancel" || !x.Content.StartsWith(Bot.Prefix)), TimeSpan.FromMinutes(5))).Result;
                await instructionMessage.DeleteAsync();

                if (message == null || message.Content.ToLower() == ".cancel")
                {
                    await ctx.Channel.Error(new Exception("Waiting for title timed out or was canceled"));
                    await message.DeleteAsync();
                    return;
                }

                title = message.Content;
                await message.DeleteAsync();

                //Roles and reactions
                instructionMessage = await ctx.Channel.SendEmbedMessage("Please mention the roles and their reaction emojis in separate mesages (max 20) using the following format: @[Role Name] [Emoji]", "Type \".done\" to finish or \".cancel\" to cancel this command", Bot.ColorMain);

                while (true)
                {
                    message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && !x.Author.IsBot && (x.Content.ToLower() == ".done" || x.Content.ToLower() == ".cancel" || !x.Content.StartsWith(Bot.Prefix)), TimeSpan.FromMinutes(5))).Result;

                    if (message != null && message.Content.ToLower() == ".done")
                    {
                        await message.DeleteAsync();
                        break;
                    }

                    if (message == null || message.Content.ToLower() == ".cancel")
                    {
                        await ctx.Channel.SendEmbedMessage("Waiting for roles and emojis timed out or canceled", "", Bot.ColorMain);
                        await message?.DeleteAsync();
                        return;
                    }

                    if (message.MentionedRoles.Count != 1)
                    {
                        ctx.Channel.SendTimedEmbedMessage("Invalid mentioned role count. Only one role should be mentioned at a time", "", Bot.ColorMain, TimeSpan.FromSeconds(10)).GetAwaiter();
                        await message.DeleteAsync();
                        continue;
                    }

                    string emojiName = message.Content.Replace(" ", "").Replace(message.MentionedRoles[0].Mention, "");
                    DiscordEmoji emoji = null;
                    if (!DiscordEmoji.IsValidUnicode(emojiName) || !DiscordEmoji.TryFromUnicode(ctx.Client, emojiName, out emoji))
                    {
                        ctx.Channel.SendTimedEmbedMessage("Reaction isn't a valid unicode emoji", "", Bot.ColorError, TimeSpan.FromSeconds(10)).GetAwaiter();
                        await message.DeleteAsync();
                        continue;
                    }

                    bool duplicate = false;
                    foreach (var value in rolesAndEmojis)
                    {
                        if (value.Item1 == message.MentionedRoles[0] || value.Item2.Name == emoji.Name)
                        {
                            duplicate = true;
                            break;
                        }
                    }

                    if (duplicate)
                    {
                        ctx.Channel.SendTimedEmbedMessage("Duplicate role or emoji", "", Bot.ColorError, TimeSpan.FromSeconds(10)).GetAwaiter();
                        await message.DeleteAsync();
                        continue;
                    }
                    else
                    {
                        rolesAndEmojis.Add(new Tuple<DiscordRole, DiscordEmoji>(message.MentionedRoles[0], emoji));

                        ctx.Channel.SendTimedEmbedMessage("Role and emoji (reaction) combination added", $"{emoji.Name} - {message.MentionedRoles[0].Mention}", Bot.ColorMain, TimeSpan.FromSeconds(5)).GetAwaiter();
                        await message.DeleteAsync();

                        if (rolesAndEmojis.Count >= 20)
                        {
                            await ctx.Channel.SendEmbedMessage("Role and emoji 20 entry limit reached", "", Bot.ColorMain);
                            break;
                        }
                    }
                }
                await instructionMessage.DeleteAsync();

                //Build message
                StringBuilder sb = new StringBuilder();
                foreach (var value in rolesAndEmojis)
                    sb.AppendLine($"{value.Item2} - {value.Item1.Mention}");

                message = await messageChannel.SendEmbedMessage(title, sb.ToString(), Bot.ColorMain);
                RoleReactionMessage roleReactionMessage = new RoleReactionMessage(messageChannel, message);

                foreach (var value in rolesAndEmojis)
                {
                    await message.CreateReactionAsync(value.Item2);
                    roleReactionMessage.ReactionRoles.Add(value.Item2.Name, value.Item1);
                }

                Bot.RoleReaction.AddMessage(roleReactionMessage);

                await ctx.Channel.SendEmbedMessage("Role reaction message created successfully", $"Message channel: {messageChannel.Mention}", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("insert_role_reaction")]
        [Description("Insert a role reaction to an existing embed message")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task InsertReaction(CommandContext ctx, DiscordChannel messageChannel, ulong messageID, int index)
        {
            try
            {
                if (messageChannel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                ServerValues values = Bot.GetValues(ctx.Guild.Id);

                if (!values.RoleReaction.Messages.ContainsKey(messageID))
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Role reaction message with provided ID doesn't exist", "", Bot.ColorError);
                    return;
                }

                RoleReactionMessage rrMessage = values.RoleReaction.Messages[messageID];
                DiscordMessage discordMessage = await messageChannel.GetMessageAsync(messageID);

                if (index < 0 || index > rrMessage.ReactionRoles.Count)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: index out of range", "", Bot.ColorError);
                    return;
                }

                if (rrMessage.ReactionRoles.Count + 1 > 20)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: too many reaction entries on the reaction role message", "", Bot.ColorError);
                    return;
                }

                InteractivityExtension interactivity = ctx.Client.GetInteractivity();

                //Roles and reactions
                DiscordMessage instructionMessage = await ctx.Channel.SendEmbedMessage("Please mention the role and its reaction emoji using the following format: @[Role Name][Emoji]", "Type \".cancel\" to cancel this command", Bot.ColorMain);
                DiscordMessage message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && !x.Author.IsBot && (x.Content.ToLower() == ".cancel" || !x.Content.StartsWith(Bot.Prefix)), TimeSpan.FromMinutes(5))).Result;

                if (message == null || message.Content.ToLower() == ".cancel")
                {
                    await ctx.Channel.SendEmbedMessage("Waiting for role and emoji timed out or canceled", "", Bot.ColorMain);
                    await message.DeleteAsync();
                    return;
                }

                if (message.MentionedRoles.Count != 1)
                {
                    ctx.Channel.SendTimedEmbedMessage("Invalid mentioned role count. Only one role should be mentioned at a time", "", Bot.ColorMain, TimeSpan.FromSeconds(10)).GetAwaiter();
                    await message.DeleteAsync();
                    return;
                }

                string emojiName = message.Content.Replace(" ", "").Replace(message.MentionedRoles[0].Mention, "");
                DiscordEmoji emoji = null;
                if (!DiscordEmoji.IsValidUnicode(emojiName) || !DiscordEmoji.TryFromUnicode(ctx.Client, emojiName, out emoji))
                {
                    ctx.Channel.SendTimedEmbedMessage("Reaction isn't a valid unicode emoji", "", Bot.ColorError, TimeSpan.FromSeconds(10)).GetAwaiter();
                    await message.DeleteAsync();
                    await instructionMessage.DeleteAsync();
                    return;
                }

                bool duplicate = false;
                foreach (string key in rrMessage.ReactionRoles.Keys)
                {
                    if (key == emoji.Name || rrMessage.ReactionRoles[key].Id == message.MentionedRoles[0].Id)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (duplicate)
                {
                    ctx.Channel.SendTimedEmbedMessage("Duplicate role or emoji", "", Bot.ColorError, TimeSpan.FromSeconds(10)).GetAwaiter();
                    await message.DeleteAsync();
                    return;
                }

                Dictionary<string, DiscordRole> newReactionRoles = new Dictionary<string, DiscordRole>();
                int i = 0;
                foreach (string key in rrMessage.ReactionRoles.Keys)
                {
                    if (i == index)
                        newReactionRoles.Add(emoji.Name, message.MentionedRoles[0]);
                    newReactionRoles.Add(key, rrMessage.ReactionRoles[key]);
                    i++;
                }
                rrMessage.ReactionRoles = newReactionRoles;

                ctx.Channel.SendEmbedMessage($"Role and emoji (reaction) combination added at index: {index}", $"{emoji.Name} - {message.MentionedRoles[0].Mention}", Bot.ColorMain).GetAwaiter();

                await message.DeleteAsync();
                await instructionMessage.DeleteAsync();

                //Build message
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                {
                    Title = discordMessage.Embeds[0].Title,
                    Color = discordMessage.Embeds[0].Color,
                };

                StringBuilder sb = new StringBuilder();
                foreach (string key in rrMessage.ReactionRoles.Keys)
                    sb.AppendLine($"{key} - {rrMessage.ReactionRoles[key].Mention}");

                embedBuilder.Description = sb.ToString();

                await discordMessage.ModifyAsync(embedBuilder.Build());
                RoleReactionMessage roleReactionMessage = new RoleReactionMessage(messageChannel, message);

                foreach (string key in rrMessage.ReactionRoles.Keys)
                {
                    DiscordEmoji reactionEmoji = DiscordEmoji.FromUnicode(ctx.Client, key);
                    await discordMessage.CreateReactionAsync(reactionEmoji);
                }

                Bot.RoleReaction.StoreServerMessages(ctx.Guild);

                await ctx.Channel.SendEmbedMessage("Role reaction message updated successfully", $"Message channel: {messageChannel.Mention}", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}