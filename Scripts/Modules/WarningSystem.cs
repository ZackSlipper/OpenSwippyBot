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
    public class WarningSystem
    {
        Bot bot;

        public WarningSystem(Bot bot)
        {
            this.bot = bot;
        }

        public bool Enabled(DiscordGuild server)
        {
            return server.GetValue("warningCountToTimeout").Int() > 0 && server.GetValue("warningTimeoutHours").Int() > 0 && server.GetValue("warningExpirationHours").Int() > 0;
        }

        public List<DateTimeOffset> GetUserWarnings(DiscordMember member)
        {
            List<DateTimeOffset> warnings = LoadWarnings(member);
            int expirationHours = member.Guild.GetValue("warningExpirationHours").Int();

            for (int i = warnings.Count - 1; i >= 0; i--)
            {
                if ((DateTimeOffset.UtcNow - warnings[i]).TotalHours >= expirationHours)
                    warnings.RemoveAt(i);
            }
            StoreWarnings(member, warnings);

            return warnings;
        }

        public int WarnUser(DiscordMember member)
        {
            List<DateTimeOffset> warnings = GetUserWarnings(member);
            warnings.Add(DateTimeOffset.UtcNow);
            StoreWarnings(member, warnings);

            int warningCountToTimeout = member.Guild.GetValue("warningCountToTimeout").Int();
            int warningTimeoutHours = member.Guild.GetValue("warningTimeoutHours").Int();
            if (warningCountToTimeout <= 0 || warningTimeoutHours <= 0 || Bot.Staff.IsStaff(member))
            {
                return -1;
            }

            if (warnings.Count >= warningCountToTimeout)
            {
                member.TimeoutAsync(DateTimeOffset.UtcNow + TimeSpan.FromHours(warningTimeoutHours));
                ClearUserWarnings(member);
            }

            return warnings.Count;
        }

        public bool RemoveLastUserWarning(DiscordMember member)
        {
            List<DateTimeOffset> warnings = GetUserWarnings(member);
            if (warnings.Count > 0)
            {
                warnings.RemoveAt(warnings.Count - 1);
                StoreWarnings(member, warnings);
                return true;
            }
            return false;
        }

        public void ClearUserWarnings(DiscordMember member)
        {
            StoreWarnings(member, null);
        }

        List<DateTimeOffset> LoadWarnings(DiscordMember member)
        {
            List<DateTimeOffset> warnings = new List<DateTimeOffset>();

            string[] warningText = member.GetValue("warnings").Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (string warning in warningText)
            {
                long ticks;
                if (!long.TryParse(warning, out ticks))
                    continue;

                warnings.Add(new DateTimeOffset(ticks, TimeSpan.Zero));
            }

            return warnings;
        }

        void StoreWarnings(DiscordMember member, List<DateTimeOffset> warnings)
        {
            if (warnings == null || warnings.Count == 0)
                member.SetValue("warnings", "");
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (DateTimeOffset warning in warnings)
                    sb.Append($"{warning.Ticks}|");

                member.SetValue("warnings", sb);
            }
        }
    }
}