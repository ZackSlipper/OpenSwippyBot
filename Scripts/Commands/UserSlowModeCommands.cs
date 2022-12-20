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
    public class UserSlowModeCommands : BaseCommandModule
    {
        [Command("enable_user_slowmode"), Aliases("slow")]
        [Description("Set a given user's timeout between messages")]
        [RequirePermissions(Permissions.ModerateMembers)]
        public async Task EnableUserSlowmode(CommandContext ctx, DiscordMember user, uint minutes, uint seconds)
        {
            try
            {
                if (user.IsBot)
                {
                    await ctx.Channel.SendEmbedMessage(":x: Slowmode is not applicable to bots", "", Bot.ColorError);
                    return;
                }

                int callerPermissionLevel = GetPermissionLevel(ctx.Member);
                if (callerPermissionLevel != 5 && callerPermissionLevel <= GetPermissionLevel(user))
                {
                    if (ctx.Member.Id == user.Id)
                        await ctx.Channel.SendEmbedMessage($":x: You don't have sufficient permissions to set slowmode for yourself", "", Bot.ColorError);
                    else
                        await ctx.Channel.SendEmbedMessage($":x: You don't have sufficient permissions to set slowmode for", user.Mention, Bot.ColorError);
                    return;
                }

                if (seconds == 0 && minutes == 0)
                {
                    await DisableUserSlowmode(ctx, user);
                    return;
                }

                if ((double)minutes + ((double)seconds / 60) > 360) //6h max
                {
                    await ctx.Channel.SendEmbedMessage(":x: Maximum message timeout time of 6h exceeded", "Please choose a lower timeout value", Bot.ColorError);
                    return;
                }

                TimeSpan timeout = TimeSpan.FromSeconds(seconds + minutes * 60);
                ctx.Guild.SetUserValue(user.Id, "slowmode_timeout", (ulong)timeout.TotalSeconds);
                await ctx.Channel.SendEmbedMessage($"🐌 Slowmode enabled for user: ", $"{user.Mention}{Environment.NewLine}Message timeout: {timeout.Minutes}min {timeout.Seconds}s", Bot.ColorMain);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = "🐌 Slowmode has been enabled for you in:",
                    Description = $"{ctx.Guild.Name}{Environment.NewLine}{Environment.NewLine}Timeout between messages was set to: {timeout.Minutes}min {timeout.Seconds}s",
                    Color = Bot.ColorMain
                };
                await ctx.Guild.GetMember(user.Id).SendMessageAsync(embed);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("disable_user_slowmode"), Aliases("speed")]
        [Description("Disable a given user's timeout between messages")]
        [RequirePermissions(Permissions.ModerateMembers)]
        public async Task DisableUserSlowmode(CommandContext ctx, DiscordMember user)
        {
            try
            {
                if (user.IsBot)
                {
                    await ctx.Channel.SendEmbedMessage(":x: Slowmode is not applicable to bots", "", Bot.ColorError);
                    return;
                }

                int callerPermissionLevel = GetPermissionLevel(ctx.Member);
                if (callerPermissionLevel != 5 && callerPermissionLevel <= GetPermissionLevel(user))
                {
                    if (ctx.Member.Id == user.Id)
                        await ctx.Channel.SendEmbedMessage($":x: You don't have sufficient permissions to disable slowmode for yourself", "", Bot.ColorError);
                    else
                        await ctx.Channel.SendEmbedMessage($":x: You don't have sufficient permissions to disable slowmode for", user.Mention, Bot.ColorError);
                    return;
                }

                if (ctx.Guild.GetUserValue(user.Id, "slowmode_timeout").ULong() == 0)
                {
                    await ctx.Channel.SendEmbedMessage("🐌 Slowmode already disabled for:", $"{user.Mention}", Bot.ColorWarning);
                    return;
                }

                ctx.Guild.SetUserValue(user.Id, "slowmode_timeout", "");
                await ctx.Channel.SendEmbedMessage($"🐌 Slowmode disabled for user: ", $"{user.Mention}", Bot.ColorMain);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                {
                    Title = "🐌 Slowmode has been disabled for you in:",
                    Description = ctx.Guild.Name,
                    Color = Bot.ColorMain
                };
                await ctx.Guild.GetMember(user.Id).SendMessageAsync(embed);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_user_slowmode"), Aliases("user_slowmode")]
        [Description("Get a given user's timeout between messages")]
        [RequirePermissions(Permissions.ModerateMembers)]
        public async Task GetUserSlowmode(CommandContext ctx, DiscordUser user)
        {
            try
            {
                TimeSpan timeout = TimeSpan.FromSeconds(ctx.Guild.GetUserValue(user.Id, "slowmode_timeout").ULong());
                if (timeout.TotalSeconds == 0)
                {
                    await ctx.Channel.SendEmbedMessage("🐌 Slowmode is disabled for:", $"{user.Mention}", Bot.ColorWarning);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"🐌 Slowmode is enabled for user: ", $"{user.Mention}{Environment.NewLine}Message timeout: {timeout.Minutes}min {timeout.Seconds}s", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        int GetPermissionLevel(DiscordMember member)
        {
            if (member.Roles.Any(x => x.Name.EndsWith("Bwo") || x.Name.EndsWith("Swippy")))
                return 5;
            if (member.Roles.Any(x => x.Name.EndsWith("BunnyHop")))
                return 4;
            return 0;
        }
    }
}