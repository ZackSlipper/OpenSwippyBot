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
    public class ThreadEnforcementCommands : BaseCommandModule
    {
        [Command("thread_enforcement_level")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ThreadEnforcementLevel(CommandContext ctx, DiscordChannel channel, ulong level)
        {
            try
            {
                if (level > 3)
                {
                    await ctx.Channel.SendEmbedMessage($"Invalid thread enforcement level: {level}", "", Bot.ColorError);
                    return;
                }

                ulong oldLevel = ctx.Guild.GetValue($"thread_enforcement_level_{channel.Id}").ULong();
                if (oldLevel == level)
                {
                    await ctx.Channel.SendEmbedMessage($"Channel thread enforcement level already set to: {level}", "", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue($"thread_enforcement_level_{channel.Id}", level);
                await ctx.Channel.SendEmbedMessage($"Channel thread enforcement level set to: {level}", channel.Mention, Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("thread_enforcement_level")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ThreadEnforcementLevel(CommandContext ctx, ulong level)
        {
            await ThreadEnforcementLevel(ctx, ctx.Channel, level);
        }

        [Command("thread_enforcement_level")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ThreadEnforcementLevel(CommandContext ctx, DiscordChannel channel)
        {
            ulong level = ctx.Guild.GetValue($"thread_enforcement_level_{channel.Id}").ULong();
            await ctx.Channel.SendEmbedMessage($"Channel thread enforcement level is set to: {level}", channel.Mention, Bot.ColorSystem);
        }

        [Command("thread_enforcement_level")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        [Description($"Set or get a thread enforcement level for the given channel\n0 - None\n1 - Inform user to use threads\n2 - Auto create threads for every message and inform the user that the channel is an auto thread channel\n3 - Only auto create threads")]
        public async Task ThreadEnforcementLevel(CommandContext ctx)
        {
            await ThreadEnforcementLevel(ctx, ctx.Channel);
        }
    }
}