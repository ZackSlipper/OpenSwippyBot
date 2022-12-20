using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Threading.Tasks;
using DSharpPlus;
using Emzi0767.Utilities;
using DSharpPlus.Exceptions;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace SwippyBot
{
    public class StaffCommands : BaseCommandModule
    {
        [Command("set_staff_role")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetStaffRole(CommandContext ctx, DiscordRole role, string tag)
        {
            try
            {
                if (!Bot.Staff.SetStaffRole(ctx.Guild, role, tag))
                {
                    await ctx.Channel.SendEmbedMessage($"🐁  Failed to set role with tag \"{tag}\" as staff role", role.Mention, Bot.ColorError);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"🐁  Successfully set role with tag \"{tag}\" as staff role", role.Mention, Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unset_staff_role")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetStaffRole(CommandContext ctx, DiscordRole role)
        {
            try
            {
                if (!Bot.Staff.UnsetStaffRole(ctx.Guild, role))
                {
                    await ctx.Channel.SendEmbedMessage($"🐁  Failed to unset staff role", role.Mention, Bot.ColorError);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"🐁  Successfully unset staff role", role.Mention, Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("list_staff_roles")]
        public async Task ListStaffRoles(CommandContext ctx)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                List<StaffRole> roles = Bot.Staff.GetStaffRoles(ctx.Guild);

                if (roles.Count == 0)
                {
                    await ctx.Channel.SendTimedEmbedMessage($"🐁  No staff roles set", "", Bot.ColorMain, TimeSpan.FromMinutes(3));
                    return;
                }

                StringBuilder sb = new StringBuilder();
                foreach (StaffRole role in roles)
                    sb.AppendLine($"• {role.Role.Mention} - {role.Tag}");

                await ctx.Channel.SendTimedEmbedMessage($"🐁  Staff Roles:", sb.ToString(), Bot.ColorMain, TimeSpan.FromMinutes(3));
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("list_staff"), Aliases("staff")]
        public async Task ListStaff(CommandContext ctx)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                List<StaffMember> members = await Bot.Staff.GetStaffMembers(ctx.Guild);

                if (members.Count == 0)
                {
                    await ctx.Channel.SendTimedEmbedMessage($"🐁  No staff members", "", Bot.ColorSystem, TimeSpan.FromMinutes(3));
                    return;
                }

                StringBuilder sb = new StringBuilder();
                foreach (StaffMember member in members)
                    sb.AppendLine($"• {member.Member.Mention} - {member.Role.Role.Mention} ({member.Role.Tag})");

                await ctx.Channel.SendTimedEmbedMessage($"🐁  Staff:", sb.ToString(), Bot.ColorSystem, TimeSpan.FromMinutes(3));
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}