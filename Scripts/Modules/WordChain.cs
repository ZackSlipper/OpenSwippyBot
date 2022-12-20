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
    public class WordChain
    {
        Bot bot;
        HashSet<ulong> channels = new HashSet<ulong>();

        public WordChain(Bot bot)
        {
            this.bot = bot;

            bot.Client.MessageCreated += MessageCreated;
            bot.Client.MessageUpdated += MessageUpdated;
        }

        async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            try
            {
                DiscordMessage message = e.Message;
                if (message == null || message.Author.IsBot || !channels.Contains(e.Channel.Id) || string.IsNullOrWhiteSpace(message.Content) || message.Content.Substring(0, 1) == Bot.Prefix)
                    return;

                bool messageDeleted = false;

                //2 message per person check
                var messages = await message.Channel.GetMessagesAsync(20);
                if (messages.Count > 1)
                {
                    int messageCount = 0;
                    for (int i = 1; i < messages.Count; i++)
                    {
                        if (!messages[i].Author.IsBot)
                        {
                            if (message.Author.Id == messages[i].Author.Id)
                                messageCount++;
                            else
                                break;
                        }
                    }

                    if (messageCount >= 2)
                    {
                        messageCount--;
                        for (int j = 0; j < messages.Count; j++)
                        {
                            if (!messages[j].Author.IsBot && message.Author.Id == messages[j].Author.Id)
                            {
                                await messages[j].DeleteAsync();
                                if (messages[j].Id == message.Id)
                                    messageDeleted = true;

                                messageCount--;
                                if (messageCount <= 0)
                                    break;
                            }
                        }

                        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = "Only 2 words per user at a time",
                            Color = Bot.ColorError
                        };
                        messageBuilder.AddEmbed(embedBuilder);
                        message.Channel.TimedMessage(TimeSpan.FromSeconds(7), messageBuilder).GetAwaiter();
                        return;
                    }
                }

                //Text check
                if (messageDeleted)
                    return;

                string content = message.Content.Replace(Environment.NewLine, " ");

                bool invalid = false;
                if (content.Length <= 50 && content.Length > 2)
                {
                    for (int i = 1; i < content.Length - 1; i++)
                    {
                        if (content[i] == ' ' && content[i - 1] != ' ' && content[i + 1] != ' ')
                        {
                            invalid = true;
                            break;
                        }
                    }
                }
                else if (content.Length > 2)
                    invalid = true;

                if (invalid)
                {
                    await message.DeleteAsync();

                    DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "Invalid word",
                        Description = "Only single words of 50 characters or less are allowed",
                        Color = Bot.ColorError
                    };
                    messageBuilder.AddEmbed(embedBuilder);
                    message.Channel.TimedMessage(TimeSpan.FromSeconds(7), messageBuilder).GetAwaiter();
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            try
            {
                if (e.Message == null || e.Message.Author == null || e.Message.Author.IsBot || !channels.Contains(e.Channel.Id))
                    return;

                await e.Message.DeleteAsync();
                e.Channel.SendTimedEmbedMessage($"No cheating", e.Message.Author.Mention, Bot.ColorMain, TimeSpan.FromSeconds(15)).GetAwaiter();
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        public void AddServerData(DiscordGuild server)
        {
            ulong channelID = server.GetValue("word_chain_channel").ULong();

            if (!channels.Contains(channelID))
                channels.Add(channelID);
        }

        public void RemoveServerData(DiscordGuild server)
        {
            ulong channelID = server.GetValue("word_chain_channel").ULong();

            if (channels.Contains(channelID))
                channels.Remove(channelID);
        }
    }
}