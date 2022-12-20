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
    public class MassPingProtectionCommands : BaseCommandModule
    {
        [Command("set_max_pings")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetMaxPings(CommandContext ctx, int count)
        {
            try
            {
                if (count <= 0)
                {
                    ctx.Guild.SetValue("maxPings", "0");
                    await ctx.Channel.SendEmbedMessage($"⛔  Mass ping protection disabled", "", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("maxPings", count);
                await ctx.Channel.SendEmbedMessage($"⛔  Max ping count set to: {count}", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_max_pings")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetMaxPings(CommandContext ctx)
        {
            try
            {
                int count = ctx.Guild.GetValue("maxPings").Int();
                await ctx.Channel.SendEmbedMessage($"⛔  Max ping count is set to: {count}", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_mass_ping_timeout_hours")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetMassPingTimeoutHours(CommandContext ctx, int hours)
        {
            try
            {
                if (hours <= 0)
                {
                    ctx.Guild.SetValue("maxPingsTimeoutHours", "0");
                    await ctx.Channel.SendEmbedMessage($"⛔  Mass ping protection disabled", "", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("maxPingsTimeoutHours", hours);
                await ctx.Channel.SendEmbedMessage($"⛔  Mass ping timeout time set to: {hours} hours", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_mass_ping_timeout_hours")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetMassPingTimeoutHours(CommandContext ctx)
        {
            try
            {
                int hours = ctx.Guild.GetValue("maxPingsTimeoutHours").Int();
                await ctx.Channel.SendEmbedMessage($"⛔  Mass ping timeout time is set to: {hours} hours", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}