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
    public class RoleReaction
    {
        Bot bot;

        public RoleReaction(Bot bot)
        {
            this.bot = bot;

            bot.Client.MessageReactionAdded += MessageReactionAdded;
            bot.Client.MessageReactionRemoved += MessageReactionRemoved;
            bot.Client.MessageDeleted += MessageDeleted;
        }

        async Task MessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs e)
        {
            if (!e.User.IsBot)
            {
                ServerValues values = Bot.GetValues(e.Guild.Id);
                if (values != null && values.RoleReaction.Messages.ContainsKey(e.Message.Id))
                {
                    if (values.RoleReaction.Messages[e.Message.Id].ReactionRoles.ContainsKey(e.Emoji.Name))
                    { 
                        try
                        {
                            await (await e.Guild.GetMemberAsync(e.User.Id)).GrantRoleAsync(values.RoleReaction.Messages[e.Message.Id].ReactionRoles[e.Emoji.Name]);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException) { }
                        catch (System.Exception ex)
                        {
                            Bot.ReportError(ex);
                        }
                    }
                    else
                        await e.Message.DeleteReactionAsync(e.Emoji, e.User);
                }
            }
        }

        async Task MessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs e)
        {
            if (!e.User.IsBot)
            {
                ServerValues values = Bot.GetValues(e.Guild.Id);
                if (values != null && values.RoleReaction.Messages.ContainsKey(e.Message.Id))
                {
                    if (values.RoleReaction.Messages[e.Message.Id].ReactionRoles.ContainsKey(e.Emoji.Name))
                    {
                        try
                        {
                            await (await e.Guild.GetMemberAsync(e.User.Id)).RevokeRoleAsync(values.RoleReaction.Messages[e.Message.Id].ReactionRoles[e.Emoji.Name]);
                        }
                        catch (DSharpPlus.Exceptions.NotFoundException) { }
                        catch (System.Exception ex)
                        {
                            Bot.ReportError(ex);
                        }
                    }
                }
            }
        }

        Task MessageDeleted(DiscordClient client, MessageDeleteEventArgs e)
        {
            ServerValues values = Bot.GetValues(e.Guild.Id);
            if (values.RoleReaction.Messages.ContainsKey(e.Message.Id))
            {
                values.RoleReaction.Messages.Remove(e.Message.Id);
                StoreServerMessages(e.Guild);

                e.Channel.SendTimedEmbedMessage("Reaction Role Message Deleted", "", Bot.ColorMain, TimeSpan.FromSeconds(5)).GetAwaiter();
            }
            return Task.CompletedTask;
        }

        public void AddMessage(RoleReactionMessage rrMessage)
        {
            ServerValues values = Bot.GetValues(rrMessage.Channel.Guild.Id);
            if (values != null && !values.RoleReaction.Messages.ContainsKey(rrMessage.Message.Id))
            {
                values.RoleReaction.Messages.Add(rrMessage.Message.Id, rrMessage);
                StoreServerMessages(rrMessage.Channel.Guild);
            }
        }

        public void LoadServerMessages(DiscordGuild server)
        {
            ServerValues values = Bot.GetValues(server.Id);
            if (values != null)
            {
                values.RoleReaction.Messages.Clear();

                string[] keyValuePairs = server.GetValue("role_reaction_data").Split('/', StringSplitOptions.RemoveEmptyEntries);
                foreach (string keyValuePair in keyValuePairs)
                {
                    string[] messageDataStrings = keyValuePair.Split('|', StringSplitOptions.RemoveEmptyEntries);
                    if (messageDataStrings.Length < 2)
                        continue;

                    ulong messageID, channelID;
                    string[] messageAndChannelStringValues = messageDataStrings[0].Split(':', StringSplitOptions.RemoveEmptyEntries);
                    if (messageAndChannelStringValues.Length != 2 || !ulong.TryParse(messageAndChannelStringValues[0], out messageID) || !ulong.TryParse(messageAndChannelStringValues[1], out channelID))
                        continue;

                    DiscordChannel channel = server.GetChannel(channelID);
                    if (channel == null)
                        continue;

                    try
                    {
                        DiscordMessage message = channel.GetMessageAsync(messageID).GetAwaiter().GetResult();
                        RoleReactionMessage rrMessage = new RoleReactionMessage(channel, message);

                        for (int i = 1; i < messageDataStrings.Length; i++)
                        {
                            string[] emojiAndRoleStringValues = messageDataStrings[i].Split(':', StringSplitOptions.RemoveEmptyEntries);
                            ulong roleID;
                            if (emojiAndRoleStringValues.Length != 2 || !ulong.TryParse(emojiAndRoleStringValues[1], out roleID))
                                continue;

                            DiscordRole role = server.GetRole(roleID);
                            if (role == null)
                                continue;

                            rrMessage.ReactionRoles.Add(emojiAndRoleStringValues[0], role);
                        }

                        if (rrMessage.Message != null && rrMessage.ReactionRoles.Count > 0)
                            values.RoleReaction.Messages.Add(message.Id, rrMessage);
                    }
                    catch (System.Exception ex)
                    {
                        Bot.ReportError(ex);

                        values.RoleReaction.Messages.Remove(messageID);
                        StoreServerMessages(server);
                    }
                }
            }
        }

        public void StoreServerMessages(DiscordGuild server)
        {
            ServerValues values = Bot.GetValues(server.Id);
            if (values != null)
            {
                StringBuilder sb = new StringBuilder();

                foreach (RoleReactionMessage rrMessage in values.RoleReaction.Messages.Values)
                {
                    sb.Append($"/{rrMessage.Message.Id}:{rrMessage.Channel.Id}|");
                    foreach (string emoji in rrMessage.ReactionRoles.Keys)
                        sb.Append($"{emoji}:{rrMessage.ReactionRoles[emoji].Id}|");
                }

                server.SetValue("role_reaction_data", sb);
            }
        }
    }

    public class RoleReactionMessage
    {
        public DiscordChannel Channel { get; }
        public DiscordMessage Message { get; }

        //Key: EmojiName
        public Dictionary<string, DiscordRole> ReactionRoles { get; set; } = new Dictionary<string, DiscordRole>();

        public RoleReactionMessage(DiscordChannel channel, DiscordMessage mesage)
        {
            Channel = channel;
            Message = mesage;
        }
    }
}