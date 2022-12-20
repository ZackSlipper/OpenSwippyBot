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
    public class WarningSystemCommands : BaseCommandModule
    {
        [Command("set_warning_expiration_time")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetWarningExpirationTime(CommandContext ctx, int hours)
        {
            try
            {
                if (hours <= 0)
                {
                    ctx.Guild.SetValue("warningExpirationHours", "0");
                    await ctx.Channel.SendEmbedMessage($"⚠️  Warning system disabled", "", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("warningExpirationHours", hours);
                await ctx.Channel.SendEmbedMessage($"⚠️  Warning expiration time set to: {hours} hours", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_warning_expiration_time")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetWarningExpirationTime(CommandContext ctx)
        {
            try
            {
                int hours = ctx.Guild.GetValue("warningExpirationHours").Int();
                await ctx.Channel.SendEmbedMessage($"⚠️  Warning expiration time is set to: {hours} hours", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_warning_count_to_timeout")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetWarningCountToTimeout(CommandContext ctx, int count)
        {
            try
            {
                if (count <= 0)
                {
                    ctx.Guild.SetValue("warningCountToTimeout", "0");
                    await ctx.Channel.SendEmbedMessage($"⚠️  Warning system disabled", "", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("warningCountToTimeout", count);
                await ctx.Channel.SendEmbedMessage($"⚠️  Warning count to timeout set to: {count} warnings", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_warning_count_to_timeout")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetWarningCountToTimeout(CommandContext ctx)
        {
            try
            {
                int count = ctx.Guild.GetValue("warningCountToTimeout").Int();
                await ctx.Channel.SendEmbedMessage($"⚠️  Warning count to timeout is set to: {count} warnings", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_warning_timeout_hours")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetWarningTimeoutHours(CommandContext ctx, int hours)
        {
            try
            {
                if (hours <= 0)
                {
                    ctx.Guild.SetValue("warningTimeoutHours", "0");
                    await ctx.Channel.SendEmbedMessage($"⚠️  Warning system disabled", "", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("warningTimeoutHours", hours);
                await ctx.Channel.SendEmbedMessage($"⚠️  Warning timeout time set to: {hours} hours", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_warning_timeout_hours")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetWarningTimeoutHours(CommandContext ctx)
        {
            try
            {
                int hours = ctx.Guild.GetValue("warningTimeoutHours").Int();
                await ctx.Channel.SendEmbedMessage($"⚠️  Warning timeout time is set to: {hours} hours", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("warn")]
        [RequireRoles(RoleCheckMode.Any, "Swippy", "🐺 | Bwo", "🐇 | BunnyHop")]
        public async Task WarnUser(CommandContext ctx, DiscordMember user)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                if (!Bot.WarningSystem.Enabled(user.Guild))
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  Warning system is disabled", "", Bot.ColorError, TimeSpan.FromMinutes(1));
                    return;
                }

                int warningCount = Bot.WarningSystem.WarnUser(user);
                if (warningCount < 0)
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  Can't warn", user.Mention, Bot.ColorError, TimeSpan.FromMinutes(1));
                    return;
                }

                int count = ctx.Guild.GetValue("warningCountToTimeout").Int();
                int hours = ctx.Guild.GetValue("warningTimeoutHours").Int();

                if (warningCount < count)
                    await ctx.Channel.SendEmbedMessage($"⚠️  Warned!", $"{user.Mention} has **{warningCount} out of {count}** warnings", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"⚠️  Warned and timed out for **{hours}** hours  🕔", $"{user.Mention}", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unwarn"), Aliases("rmwarn")]
        [RequireRoles(RoleCheckMode.Any, "Swippy", "🐺 | Bwo", "🐇 | BunnyHop")]
        public async Task UnwarnUser(CommandContext ctx, DiscordMember user)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                if (!Bot.WarningSystem.Enabled(user.Guild))
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  Warning system is disabled", "", Bot.ColorError, TimeSpan.FromMinutes(1));
                    return;
                }

                List<DateTimeOffset> warnings = Bot.WarningSystem.GetUserWarnings(user);
                if (warnings.Count == 0)
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  User doesnt have any warnings", user.Mention, Bot.ColorError, TimeSpan.FromMinutes(1));
                    return;
                }

                int count = ctx.Guild.GetValue("warningCountToTimeout").Int();
                if (Bot.WarningSystem.RemoveLastUserWarning(user))
                    await ctx.Channel.SendEmbedMessage($"⚠️  Removed last warning from", $"{user.Mention}{Environment.NewLine}{Environment.NewLine}{warnings.Count-1} out of {count} warnings", Bot.ColorMain);
                else
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  User doesnt have any warnings", user.Mention, Bot.ColorError, TimeSpan.FromMinutes(1));
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("list_warnings"), Aliases("list_warn", "lw")]
        public async Task ListWarnings(CommandContext ctx, DiscordMember user)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                if (!Bot.WarningSystem.Enabled(user.Guild))
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  Warning system is disabled", "", Bot.ColorError, TimeSpan.FromMinutes(1));
                    return;
                }

                List<DateTimeOffset> warnings = Bot.WarningSystem.GetUserWarnings(user);
                if (warnings.Count == 0)
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  {user.Name()} doesn't have any warnings", "", Bot.ColorError, TimeSpan.FromMinutes(1));
                    return;
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < warnings.Count; i++)
                    sb.AppendLine($"{i + 1}. {warnings[i].ToString().Split("+")[0]} (Time since warning: {(DateTimeOffset.UtcNow - warnings[i]).ToString().Split(".")[0]})");

                await ctx.Channel.SendTimedEmbedMessage($"⚠️  {user.Name()}'s warnings:", sb.ToString(), Bot.ColorMain, TimeSpan.FromMinutes(2));
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}