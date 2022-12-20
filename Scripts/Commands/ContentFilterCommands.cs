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
    public class ContentFilterCommands : BaseCommandModule
    {
        [Command("content_filtering")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ContentFiltering(CommandContext ctx, bool enabled)
        {
            try
            {
                bool detectionEnabled = ctx.Guild.GetValue("content_filtering").Bool();
                if(detectionEnabled == enabled)
                    await ctx.Channel.SendEmbedMessage($"⚠️  Content filtering is already {(enabled ? "en" : "dis")}abled", "", Bot.ColorSystem);
                else
                {
                    ctx.Guild.SetValue("content_filtering", enabled ? "1" : "0");
                    await ctx.Channel.SendEmbedMessage($"🙊  Content filtering {(enabled ? "en" : "dis")}abled", "", Bot.ColorSystem);
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("content_filtering")]
        [Description("Toggle inappropriate content detection")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ContentFiltering(CommandContext ctx)
        {
            try
            {
                bool enabled = ctx.Guild.GetValue("content_filtering").Bool();
                await ctx.Channel.SendEmbedMessage($"🙊  Content filtering is {(enabled ? "en" : "dis")}abled", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("filtered_content_deletion")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ContentDeletion(CommandContext ctx, bool enabled)
        {
            try
            {
                bool detectionEnabled = ctx.Guild.GetValue("filtered_content_deletion").Bool();
                if (detectionEnabled == enabled)
                    await ctx.Channel.SendEmbedMessage($"⚠️  Filtered content deletion is already {(enabled ? "en" : "dis")}abled", "", Bot.ColorWarning);
                else
                {
                    ctx.Guild.SetValue("filtered_content_deletion", enabled ? "1" : "0");
                    await ctx.Channel.SendEmbedMessage($"🙊  Filtered content deletion {(enabled ? "en" : "dis")}abled", "", Bot.ColorSystem);
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("filtered_content_deletion")]
        [Description("Toggle inappropriate content deletion")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ContentDeletion(CommandContext ctx)
        {
            try
            {
                bool enabled = ctx.Guild.GetValue("filtered_content_deletion").Bool();
                await ctx.Channel.SendEmbedMessage($"Filtered content deletion is {(enabled ? "en" : "dis")}abled", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("content_filter_add_ignored_channel")]
        [Description("Add a channel to a list be ignored by content monitoring")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task AddIgnoreChannel(CommandContext ctx, DiscordChannel channel)
        {
            try
            {
                if (Bot.ContentFilter.AddIgnoredChannel(channel.Guild, channel.Id))
                    await ctx.Channel.SendEmbedMessage($"🙊  Channel added to the content filter's ignored channel list", channel.Mention, Bot.ColorSystem);
                else
                    await ctx.Channel.SendEmbedMessage($"⚠️  Channel already in content filter's ignored channel list", "", Bot.ColorWarning);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("content_filter_remove_ignored_channel")]
        [Description("Remove a channel from a list so it's no longer ignored by content monitoring")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task RemoveIgnoreChannel(CommandContext ctx, DiscordChannel channel)
        {
            try
            {
                if (Bot.ContentFilter.RemoveIgnoredChannel(channel.Guild, channel.Id))
                    await ctx.Channel.SendEmbedMessage($"🙊  Channel removed from the content filter's ignored channel list", channel.Mention, Bot.ColorSystem);
                else
                    await ctx.Channel.SendEmbedMessage($"⚠️  Channel already absent from the content filter's ignored channel list", "", Bot.ColorWarning);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("content_filter_ignored_channel_list")]
        [Description("List all channels ignored by content monitoring")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task IgnoredChannelList(CommandContext ctx)
        {
            try
            {
                string ignoredChannels = Bot.ContentFilter.ListIgnoredChannels(ctx.Guild);
                await ctx.Channel.SendEmbedMessage($"🙊  Channels ignored by content monitoring:", string.IsNullOrWhiteSpace(ignoredChannels) ? "[None]" : ignoredChannels, Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        /*[Command("test")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Test(CommandContext ctx, ulong id)
        {
            try
            {
                DiscordEmoji emoji = DiscordEmoji.FromGuildEmote(ctx.Client, id);
                await ctx.Channel.SendMessageAsync($"<:{emoji.Name}:{emoji.Id}>");

                
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }*/
    }
}