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
    public class WordChainCommands : BaseCommandModule
    {
        [Command("word_chain_set_channel")]
        [Description("Set the channel the word chain should operate in")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetChannel(CommandContext ctx, DiscordChannel channel)
        {
            try
            {
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                ulong oldChannelID = ctx.Guild.GetValue("word_chain_channel").ULong();
                if (oldChannelID != 0 && oldChannelID == channel.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Word chain channel already set to:", channel.Mention, Bot.ColorWarning);
                    return;
                }

                Bot.WordChain.RemoveServerData(ctx.Guild);
                ctx.Guild.SetValue("word_chain_channel", channel.Id);
                Bot.WordChain.AddServerData(ctx.Guild);
                
                await ctx.Channel.SendEmbedMessage($"Word chain channel set to:", channel.Mention, Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("word_chain_unset_channel")]
        [Description("Unset the word chain channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetChannel(CommandContext ctx)
        {
            try
            {
                ulong oldChannelID = ctx.Guild.GetValue("word_chain_channel").ULong();
                if (oldChannelID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"Word chain channel not set already", "", Bot.ColorWarning);
                    return;
                }

                Bot.WordChain.RemoveServerData(ctx.Guild);
                ctx.Guild.SetValue("word_chain_channel", "");
                await ctx.Channel.SendEmbedMessage($"Word chain channel unset", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("word_chain_get_channel")]
        [Description("Get the currently set word chain channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetChannel(CommandContext ctx)
        {
            try
            {
                DiscordChannel channel = ctx.Guild.GetValueChannel("word_chain_channel");
                if (channel == null)
                    await ctx.Channel.SendEmbedMessage($"Word chain channel not set", "", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"Word chain channel set to:", channel.Mention, Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}