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
    public class ThreadEnforcement
    {
        Bot bot;

        public ThreadEnforcement(Bot bot)
        {
            this.bot = bot;

            bot.Client.MessageCreated += MessageCreated;
        }

        async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            try
            {
                if (e == null || e.Guild == null || e.Channel == null || e.Message == null || e.Message.Author.IsBot || e.Channel.IsThread || (!string.IsNullOrWhiteSpace(e.Message.Content) && e.Message.Content.StartsWith(Bot.Prefix)))
                    return;

                //Check if message channel has a ThreadEnforcement level set (0 - none, 1 - inform to use threads, 2 - Auto create threads for every message and inform that the channel is an auto thread channel, 3 - like 2 but without informing)
                ulong level = e.Guild.GetValue($"thread_enforcement_level_{e.Channel.Id}").ULong();
                if (level > 3)
                    level = 0;

                switch (level)
                {
                    case 1:
                        e.Channel.SendTimedEmbedMessage("↳  This is a channel that (annoyingly) encourages use of threads", "Please use a thread if your message is a response to an existing post on this channel", Bot.ColorMain, TimeSpan.FromSeconds(30)).GetAwaiter();
                        break;
                    case 2:
                        await e.Message.CreateThreadAsync("Thread", AutoArchiveDuration.Day, "Auto Thread");
                        e.Channel.SendTimedEmbedMessage("↳  This is a thread enforced channel", "A thread has been created for your post. If your message is a response to an existing post please delete your message and respond in a related thread", Bot.ColorMain, TimeSpan.FromSeconds(30)).GetAwaiter();
                        break;
                    case 3:
                        if (e.Message.Attachments.Count == 0 && !e.Message.Content.Contains("https://") && !e.Message.Content.Contains("http://"))
                        {
                            await e.Message.DeleteAsync();
                            return;
                        }

                        await e.Message.CreateThreadAsync("Thread", AutoArchiveDuration.Day, "Auto Thread");
                        e.Channel.SendTimedEmbedMessage("↳  This is a thread enforced channel", "A thread has been created for your post. If your message is a response to an existing post please post it in a related thread", Bot.ColorMain, TimeSpan.FromSeconds(30)).GetAwaiter();
                        break;
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}