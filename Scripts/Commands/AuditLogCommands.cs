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
    public class AuditLogCommands : BaseCommandModule
    {
        readonly string[] types = { "admin", "user", "message", "vc" }; //, "presence" <---- removed

        [Command("log_set_channel")]
        [Description("Set the audit log channel (Types: admin, user, message, vc)")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetChannel(CommandContext ctx, [Description("Types: admin, user, message, vc")] string type, DiscordChannel channel)
        {
            try
            {
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                if (!types.Contains(type))
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Invalid type", "", Bot.ColorError);
                    return;
                }

                ulong oldChannelID = ctx.Guild.GetValue($"audit_log_{type}_channel").ULong();
                if (oldChannelID != 0 && oldChannelID == channel.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Audit log {type} channel already set to \"{channel.Name}\"", "", Bot.ColorWarning);
                    return;
                }

                ctx.Guild.SetValue($"audit_log_{type}_channel", channel.Id);
                Bot.AuditLog.UpdateServerChannels(ctx.Guild);

                await ctx.Channel.SendEmbedMessage($"Audit log {type} channel set to \"{channel.Name}\"", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("log_unset_channel")]
        [Description("Unset the audit log channel (Types: admin, user, message, vc)")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetChannel(CommandContext ctx, [Description("Types: admin, user, message, vc")] string type)
        {
            try
            {
                if (!types.Contains(type))
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Invalid type", "", Bot.ColorError);
                    return;
                }

                ulong oldChannelID = ctx.Guild.GetValue($"audit_log_{type}_channel").ULong();
                if (oldChannelID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"Audit log {type} channel unset already", "", Bot.ColorWarning);
                    return;
                }

                Bot.Counter.RemoveServerData(ctx.Guild);
                ctx.Guild.SetValue($"audit_log_{type}_channel", "");
                await ctx.Channel.SendEmbedMessage($"Audit log {type} channel unset", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("log_get_channels")]
        [Description("Get the currently set audit log channels")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetChannels(CommandContext ctx)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (string type in types)
                {
                    DiscordChannel channel = ctx.Guild.GetValueChannel($"audit_log_{type}_channel");
                    if (channel != null)
                        sb.AppendLine($"{type}: {channel.Mention}");
                    else
                        sb.AppendLine($"{type}: [Unset]");
                }

                await ctx.Channel.SendEmbedMessage($"Audit log channels:", sb.ToString(), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}