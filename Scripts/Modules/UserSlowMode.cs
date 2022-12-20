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
    public class UserSlowMode
    {
        Bot bot;

        public UserSlowMode(Bot bot)
        {
            this.bot = bot;

            bot.Client.MessageCreated += MessageCreated;
        }

        async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            if (e.Guild == null || e.Author.IsBot)
                return;

            TimeSpan timeout = TimeSpan.FromSeconds(e.Guild.GetUserValue(e.Author.Id, "slowmode_timeout").ULong());
            if (timeout.TotalSeconds > 0)
            {
                string lastMessageTimeValue = e.Guild.GetUserValue(e.Author.Id, "last_slowmode_message_time");
                DateTime lastMessageTime = string.IsNullOrEmpty(lastMessageTimeValue) ? DateTime.MinValue : DateTime.FromBinary(lastMessageTimeValue.Long());

                if (DateTime.UtcNow < lastMessageTime + timeout)
                    await e.Message.DeleteAsync();
                else
                    e.Guild.SetUserValue(e.Author.Id, "last_slowmode_message_time", e.Message.CreationTimestamp.UtcDateTime.ToBinary());
            }
        }
    }
}