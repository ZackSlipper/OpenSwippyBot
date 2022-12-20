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
    public class CounterCommands : BaseCommandModule
    {
        [Command("counter_set_channel")]
        [Description("Set the channel the counter should operate in")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetChannel(CommandContext ctx, DiscordChannel channel)
        {
            try
            {
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"⛔  \"{ctx.Command.Name}\" Command Error: Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                ulong oldChannelID = ctx.Guild.GetValue("counting_channel").ULong();
                if (oldChannelID != 0 && oldChannelID == channel.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"⚠️  Counting channel already set to \'{channel.Name}\'", "", Bot.ColorWarning);
                    return;
                }

                Bot.Counter.RemoveServerData(ctx.Guild);
                ctx.Guild.SetValue("counting_channel", channel.Id);
                Bot.Counter.AddServerData(ctx.Guild);

                await ctx.Channel.SendEmbedMessage($"🔢  Counting channel set to \'{channel.Name}\'", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_unset_channel")]
        [Description("Unset the counter channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetChannel(CommandContext ctx)
        {
            try
            {
                ulong oldChannelID = ctx.Guild.GetValue("counting_channel").ULong();
                if (oldChannelID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"⚠️  Counting channel unset already", "", Bot.ColorWarning);
                    return;
                }

                Bot.Counter.RemoveServerData(ctx.Guild);
                ctx.Guild.SetValue("counting_channel", "");
                await ctx.Channel.SendEmbedMessage($"🔢  Counting channel unset", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_get_channel")]
        [Description("Get the currently set counting channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetChannel(CommandContext ctx)
        {
            try
            {
                DiscordChannel channel = ctx.Guild.GetValueChannel("counting_channel");
                if (channel == null)
                    await ctx.Channel.SendEmbedMessage($"🔢  Counting channel not set", "", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"🔢  Counting channel set to \'{channel.Name}\'", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_number")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Number(CommandContext ctx, ulong number)
        {
            try
            {
                if (number > 10000000000) //Ten billion
                {
                    await ctx.Channel.SendTimedEmbedMessage($"⚠️  Count number can not be set to a value higher then 10000000000 (ten billion)", "", Bot.ColorWarning, TimeSpan.FromSeconds(20));
                    return;
                }

                Bot.Counter.SetCurrentNumber(ctx.Guild, number);
                if (number == 0)
                    await ctx.Channel.SendEmbedMessage($"🔢  Count number set to 0. Lives have been reset", "", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"🔢  Count number set to {number}", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_number")]
        [Description("Get or set the current count number")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Number(CommandContext ctx)
        {
            try
            {
                ulong number = ctx.Guild.GetValue("counter_number").ULong();

                await ctx.Channel.SendEmbedMessage($"🔢  Current count number is {number}", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_max_lives")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task MaxLives(CommandContext ctx, int lives)
        {
            try
            {
                if (lives < 1)
                {
                    await ctx.Channel.SendEmbedMessage($"⚠️  Max number of lives can not be less than 1", "", Bot.ColorWarning);
                    return;
                }

                if (lives > 10)
                {
                    await ctx.Channel.SendEmbedMessage($"⚠️  Max number of lives can not be more than 10", "", Bot.ColorWarning);
                    return;
                }

                ctx.Guild.SetValue("counting_max_lives", lives);
                if (ctx.Guild.GetValue("counting_lives").Int() > lives)
                    ctx.Guild.SetValue("counting_lives", lives);

                await ctx.Channel.SendEmbedMessage($"🔢  Maximum number of lives set to {lives}", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_max_lives")]
        [Description("Get or set the maximum amount of lives in a single counting round")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task MaxLives(CommandContext ctx)
        {
            try
            {
                int lives = ctx.Guild.GetValue("counting_max_lives").Int();
                if (lives < 1) lives = 1;
                else if (lives > 10) lives = 10;

                await ctx.Channel.SendEmbedMessage($"🔢  Maximum number of lives is set to {lives}", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_lives")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Lives(CommandContext ctx, int lives)
        {
            try
            {
                int maxLives = ctx.Guild.GetValue("counting_max_lives").Int();
                if (maxLives < 1) maxLives = 1;
                else if (maxLives > 10) maxLives = 10;

                if (lives < 0)
                {
                    await ctx.Channel.SendEmbedMessage($"⚠️  Number of lives can not be less than 0", "", Bot.ColorWarning);
                    return;
                }

                if (lives > maxLives)
                {
                    await ctx.Channel.SendEmbedMessage($"⚠️  Number of lives can not be more than max lives ({maxLives})", "", Bot.ColorWarning);
                    return;
                }


                if (lives == 0)
                {
                    Bot.Counter.SetCurrentNumber(ctx.Guild, 0);
                    await ctx.Channel.SendEmbedMessage($"🔢  Lives set to zero. Resetting count to 0", "", Bot.ColorMain);
                }
                else
                {
                    ctx.Guild.SetValue("counting_lives", lives);
                    await ctx.Channel.SendEmbedMessage($"🔢  Number of lives set to {lives}", "", Bot.ColorMain);
                }

            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("counter_lives")]
        [Description("Get or set the amount of lives remaining in the current counting round")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Lives(CommandContext ctx)
        {
            try
            {
                int lives = ctx.Guild.GetValue("counting_lives").Int();

                await ctx.Channel.SendEmbedMessage($"🔢  Number of lives is {lives}", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("lives")]
        [Description("Get the amount of lives remaining in the current counting round")]
        public async Task LivesUser(CommandContext ctx)
        {
            await Lives(ctx);
        }


        //Self No Counting Role
        [Command("set_no_counting_role")]
        [Description("Sets the role making the user restricted from counting in the counting channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetNoCountingRole(CommandContext ctx, DiscordRole role)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("no_counting_role").ULong();
                if (currentRoleID == role.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"🔢  No counting role already set to: ", $"{role.Mention}", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("no_counting_role", role.Id);
                await ctx.Channel.SendEmbedMessage($"🔢  No counting role set to: ", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unset_no_counting_role")]
        [Description("Unsets the role making the user restricted from counting in the counting channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetNoCountingRole(CommandContext ctx)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("no_counting_role").ULong();
                if (currentRoleID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"🔢  No counting role already not set", $"", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("no_counting_role", "");
                await ctx.Channel.SendEmbedMessage($"🔢  No counting role unset", $"", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_no_counting_role")]
        [Description("Gets the role making the user restricted from counting in the counting channel")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetNoCountingRole(CommandContext ctx)
        {
            try
            {
                DiscordRole role = ctx.Guild.GetValue("no_counting_role").DiscordRole(ctx.Guild);
                if (role == null)
                {
                    await ctx.Channel.SendEmbedMessage($"🔢  No counting role not set", $"", Bot.ColorSystem);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"🔢  No counting role:", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }
    }
}