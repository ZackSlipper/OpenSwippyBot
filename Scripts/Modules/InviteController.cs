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
    public class InviteController
    {
        Bot bot;

        HashSet<string> blockedInvites = new HashSet<string>();

        public InviteController(Bot bot)
        {
            this.bot = bot;

            bot.Client.InviteCreated += InviteCreated;
            bot.Client.InviteDeleted += InviteDeleted;
        }

        async Task InviteCreated(DiscordClient client, InviteCreateEventArgs e)
        {
            try
            {
                if (e.Invite == null)
                    return;

                bool serverBlockEnabled = e.Guild.GetValue("invites_blocked") == "1";
                DiscordRole bypassRole = e.Guild.GetValue("invite_blocker_bypass_role").DiscordRole(e.Guild);
                
                if (!serverBlockEnabled || e.Invite.Inviter.Id == Bot.ID || (bypassRole != null && e.Guild.GetMember(e.Invite.Inviter.Id).Roles.Contains(bypassRole))) //Report non-blocked invite's creation
                {
                    DiscordChannel channel = Bot.AuditLog.GetAdminLogChannel(e.Guild.Id);
                    if (channel == null)
                        return;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Code: {e.Invite.Code}");
                    sb.AppendLine($"Inviter: {e.Invite.Inviter.Mention}");
                    sb.AppendLine($"Inviter ID: {e.Invite.Inviter.Id}");
                    sb.AppendLine($"Created At: {e.Invite.CreatedAt}");
                    sb.AppendLine($"Max Uses: {e.Invite.MaxUses}");
                    sb.AppendLine($"MaxAge: {(float)e.Invite.MaxAge / 60 / 60} hours");
                    if (e.Invite.ExpiresAt.HasValue)
                        sb.AppendLine($"Expiration date: {e.Invite.ExpiresAt}");

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "📩 Invite Created",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem
                    };
                    await channel.SendMessageAsync(embedBuilder);
                    return;
                }

                //Delete blocked invite
                await e.Invite.DeleteAsync();
                blockedInvites.Add(e.Invite.Code);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task InviteDeleted(DiscordClient client, InviteDeleteEventArgs e)
        {
            DiscordChannel channel = Bot.AuditLog.GetAdminLogChannel(e.Guild.Id);
            if (channel == null)
                return;

            StringBuilder sb = new StringBuilder();
            bool blocked = false;

            if (e.Invite != null && e.Invite.Code != null)
            {
                blocked = blockedInvites.Contains(e.Invite.Code);
                blockedInvites.Remove(e.Invite.Code);

                sb.AppendLine($"Code: {e.Invite.Code}");
                sb.AppendLine($"Inviter: {e.Invite.Inviter.Mention}");
                sb.AppendLine($"Inviter ID: {e.Invite.Inviter.Id}");
                sb.AppendLine($"Created At: {e.Invite.CreatedAt}");
                sb.AppendLine($"Max Uses: {e.Invite.MaxUses}");
                sb.AppendLine($"MaxAge: {(float)e.Invite.MaxAge / 60 / 60} hours");
                if (e.Invite.ExpiresAt.HasValue)
                    sb.AppendLine($"Expiration date: {e.Invite.ExpiresAt}");
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"{(blocked ? "🛡️" : "✂️")} Invite {(blocked ? "Blocked" : "Deleted")}",
                Description = sb.ToString(),
                Color = blocked ? Bot.ColorWarning : Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }
    }
}