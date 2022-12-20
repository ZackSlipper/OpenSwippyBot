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
    public class NewJoinerRoles
    {
        Bot bot;
        HashSet<ulong> channels = new HashSet<ulong>();

        public NewJoinerRoles(Bot bot)
        {
            this.bot = bot;

            bot.Client.GuildMemberAdded += UserJoined;
        }

        async Task UserJoined(DiscordClient client, GuildMemberAddEventArgs e)
        {
            List<DiscordRole> roles = Bot.GetValues(e.Guild.Id).NewJoinerRoles.Roles;
            foreach (DiscordRole role in roles)
            {
                try
                {
                    await e.Member.GrantRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }
        }

        public bool AddServerRole(DiscordGuild server, DiscordRole role)
        {
            List<DiscordRole> roles = Bot.GetValues(server.Id).NewJoinerRoles.Roles;
            if (!roles.Contains(role))
            {
                roles.Add(role);
                StoreServerRoles(server);
                return true;
            }
            return false;
        }

        public bool RemoveServerRole(DiscordGuild server, DiscordRole role)
        {
            List<DiscordRole> roles = Bot.GetValues(server.Id).NewJoinerRoles.Roles;
            if (roles.Contains(role))
            {
                roles.Remove(role);
                StoreServerRoles(server);
                return true;
            }
            return false;
        }

        public void LoadServerRoles(DiscordGuild server)
        {
            List<DiscordRole> roles = Bot.GetValues(server.Id).NewJoinerRoles.Roles;
            roles.Clear();

            string[] roleText = server.GetValue("new_joiner_roles").Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (string roleID in roleText)
            {
                try
                {
                    DiscordRole role = server.GetRole(roleID.ULong());
                    if (role != null)
                        roles.Add(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }
        }

        public void StoreServerRoles(DiscordGuild server)
        {
            List<DiscordRole> roles = Bot.GetValues(server.Id).NewJoinerRoles.Roles;
            StringBuilder sb = new StringBuilder();
            foreach (DiscordRole role in roles)
                sb.Append($"{role.Id}|");

            server.SetValue("new_joiner_roles", sb);
        }
    }
}