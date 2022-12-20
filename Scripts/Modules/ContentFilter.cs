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
    public class ContentFilter
    {
        Bot bot;

        public ContentFilter(Bot bot)
        {
            this.bot = bot;
        }

        public async Task Filter(DiscordMessage message, string term)
        {
            try
            {
                if (message.Channel.Guild.GetValue("content_filtering").Bool() && !MessageInIgnoredChannel(message))
                {
                    if (message.Channel.Guild.GetValue("filtered_content_deletion").Bool())
                        await message.DeleteAsync();

                    StringBuilder sb = new StringBuilder();

                    if (Bot.WarningSystem.Enabled(message.Channel.Guild))
                    {
                        int warningCount = Bot.WarningSystem.WarnUser((DiscordMember)message.Author);

                        if (warningCount > 0)
                        {
                            int count = message.Channel.Guild.GetValue("warningCountToTimeout").Int();
                            int hours = message.Channel.Guild.GetValue("warningTimeoutHours").Int();

                            if (warningCount < count)
                                sb.AppendLine($"⚠️  {message.Author.Mention} has been **Warned!** User has **{warningCount} out of {count}** warnings");
                            else
                                sb.AppendLine($"⚠️  {message.Author.Mention}** Warned and timed out for {hours} hours**  🕔");
                        }
                        else
                            sb.AppendLine(message.Author.Mention);
                    }
                    else
                        sb.AppendLine(message.Author.Mention);

                    //sb.AppendLine("**WARNING! 🔧 This feature is in development and may be too sensitive and detect filtered words where there are none 🔧 **");

                    /*if (message.Author.Id == 737024210609766524 || message.Author.Id == 649006414534279219) //Twommy
                    {
                        try
                        {
                            DiscordEmoji emoji = DiscordEmoji.FromGuildEmote(bot.Client, 938917559259004973);
                            await message.RespondAsync($"<:{emoji.Name}:{emoji.Id}>");
                        }
                        catch (System.Exception){}
                    }*/

                    //User warning message
                    message.SendReplyEmbedMessage("🔇  Hey! No swearing! :(", sb.ToString(), Bot.ColorMain).GetAwaiter();


                    //Audit log warning
                    DiscordChannel channel = Bot.AuditLog.GetMessageLogChannel(message.Channel.Guild.Id);
                    if (channel == null)
                        return;

                    DiscordMessageBuilder builderMessage = new DiscordMessageBuilder();

                    StringBuilder auditLogSb = new StringBuilder();
                    auditLogSb.AppendLine("**Author:**");
                    auditLogSb.AppendLine(message.Author.Mention);
                    auditLogSb.AppendLine($"ID: {message.Author.Id}");

                    auditLogSb.AppendLine("");
                    auditLogSb.AppendLine("**Message:**");
                    auditLogSb.AppendLine(message.JumpLink.ToString());

                    auditLogSb.AppendLine("");
                    auditLogSb.AppendLine($"**Filtered term:** {term}");

                    string preview = "";

                    //Content
                    if (!string.IsNullOrWhiteSpace(message.Content))
                    {
                        auditLogSb.AppendLine("");
                        auditLogSb.AppendLine("**Message content:**");
                        auditLogSb.AppendLine(message.Content);
                    }

                    //Attachments
                    if (message.Attachments.Count > 0)
                    {
                        auditLogSb.AppendLine("");

                        if (message.Attachments.Count == 1)
                        {
                            auditLogSb.AppendLine("**Message attachment:**");
                            auditLogSb.AppendLine($"{message.Attachments[0].Url}");

                            string mime = message.Attachments[0].MediaType.Split('/')[0];
                            if (mime == "image")
                                preview = message.Attachments[0].Url;
                        }
                        else
                        {
                            auditLogSb.AppendLine("**Message attachments:**");
                            for (int i = 0; i < message.Attachments.Count; i++)
                                sb.AppendLine($"{i + 1}. {message.Attachments[i].Url}");
                        }
                    }

                    //Stickers
                    if (message.Stickers.Count > 0)
                    {
                        auditLogSb.AppendLine("");
                        auditLogSb.AppendLine("**Sticker:**");
                        auditLogSb.AppendLine($"{message.Stickers[0].Name} ({message.Stickers[0].Id})");
                        auditLogSb.AppendLine(message.Stickers[0].StickerUrl);
                        preview = message.Stickers[0].StickerUrl;
                    }

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "🙊 Filtered Content Detected",
                        Description = auditLogSb.ToString(),
                        Color = Bot.ColorSystem,
                        ImageUrl = preview
                    };

                    builderMessage.AddEmbed(embedBuilder);

                    if (message.Embeds.Count > 0)
                    {
                        embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = "Filtered Embeds: ",
                            Color = Bot.ColorSystem
                        };
                        builderMessage.AddEmbed(embedBuilder);
                        builderMessage.AddEmbeds(message.Embeds);
                    }

                    await channel.SendMessageAsync(builderMessage); //Audit log message
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        bool MessageInIgnoredChannel(DiscordMessage message)
        {
            return Bot.GetValues(message.Channel.Guild.Id).ContentFilter.IgnoredChannels.Contains(message.Channel.Id);
        }

        public void LoadIgnoredChannels(DiscordGuild server)
        {
            ContentFilterValues values = Bot.GetValues(server.Id).ContentFilter;
            values.IgnoredChannels.Clear();

            string[] channelListText = server.GetValue("content_filter_ignored_channels").Split(",", StringSplitOptions.RemoveEmptyEntries);
            DiscordChannel channel;
            foreach (string entry in channelListText)
            {
                channel = entry.DiscordChannel(server);
                if (channel != null)
                    values.IgnoredChannels.Add(channel.Id);
            }
        }

        public bool AddIgnoredChannel(DiscordGuild server, ulong id)
        {
            ContentFilterValues values = Bot.GetValues(server.Id).ContentFilter;
            if (!values.IgnoredChannels.Contains(id))
            {
                values.IgnoredChannels.Add(id);
                StoreIgnoredChannels(server);
                return true;
            }
            return false;
        }

        public bool RemoveIgnoredChannel(DiscordGuild server, ulong id)
        {
            ContentFilterValues values = Bot.GetValues(server.Id).ContentFilter;
            if (values.IgnoredChannels.Contains(id))
            {
                values.IgnoredChannels.Remove(id);
                StoreIgnoredChannels(server);
                return true;
            }
            return false;
        }

        public string ListIgnoredChannels(DiscordGuild server)
        {
            ContentFilterValues values = Bot.GetValues(server.Id).ContentFilter;

            StringBuilder sb = new StringBuilder();
            DiscordChannel channel;
            int index = 1;

            foreach (ulong item in values.IgnoredChannels)
            {
                channel = item.DiscordChannel(server);
                if (channel != null)
                {
                    sb.AppendLine($"{index}. {channel.Mention}");
                    index++;
                }
            }
            return sb.ToString();
        }

        void StoreIgnoredChannels(DiscordGuild server)
        {
            ContentFilterValues values = Bot.GetValues(server.Id).ContentFilter;
            StringBuilder sb = new StringBuilder();

            foreach (ulong item in values.IgnoredChannels)
                sb.Append($"{item},");

            server.SetValue("content_filter_ignored_channels", sb);
        }
    }
}