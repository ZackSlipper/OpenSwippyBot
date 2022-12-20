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
    public class MassPingProtection
    {
        Bot bot;

        public MassPingProtection(Bot bot)
        {
            this.bot = bot;

            bot.Client.MessageCreated += MessageCreated;
            bot.Client.MessageUpdated += MessageUpdated;
        }

        public async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            await ScanForMassPing(e.Message);
        }

        public async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            await ScanForMassPing(e.Message);
        }

        async Task ScanForMassPing(DiscordMessage msg)
        {
            try
            {
                if (msg == null || msg.Channel == null || msg.Channel.Guild == null || msg.Author == null || msg.Author.IsBot || (msg.Author.IsSystem.HasValue && msg.Author.IsSystem.Value))
                    return;

                DiscordMember author = await msg.Channel.Guild.GetMemberAsync(msg.Author.Id);

                int maxPings = msg.Channel.Guild.GetValue("maxPings").Int();
                int timeoutHours = msg.Channel.Guild.GetValue("maxPingsTimeoutHours").Int();

                if (maxPings <= 0 || timeoutHours <= 0)
                    return;

                

                if (msg.MentionedUsers.Count > maxPings)
                {
                    if (Bot.Staff.IsStaff(author))
                        return;

                    await author.TimeoutAsync(DateTimeOffset.Now + TimeSpan.FromHours(timeoutHours));
                    await msg.DeleteAsync();
                    await msg.Channel.SendEmbedMessage("⛔ Mass Ping Detected!", $"{author.Mention} pinged **{msg.MentionedUsers.Count}** users when the maximum allowed ping count is **{maxPings}**{Environment.NewLine}{Environment.NewLine}User timed out for {timeoutHours} hours and server staff pinged", Bot.ColorError);

                    List<StaffMember> staff = await Bot.Staff.GetStaffMembers(msg.Channel.Guild);
                    foreach (StaffMember staffMember in staff)
                    {
                        await staffMember.Member.SendEmbedDM($"⛔ Mass Ping Detected in \"{msg.Channel.Guild.Name}\" Server!", $"User {author.Mention} ({author.Id}) pinged **{msg.MentionedUsers.Count}** users of max **{maxPings}** allowed pings in channel {msg.Channel.Mention}", Bot.ColorError);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}