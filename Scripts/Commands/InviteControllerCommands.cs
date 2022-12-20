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
    public class InviteControllerCommands : BaseCommandModule
    {
        //Invite Channel
        [Command("set_invite_channel")]
        [Description("Sets the channel to which invites created using the \".invite\" invite to")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetInviteChannel(CommandContext ctx, DiscordChannel channel)
        {
            try
            {
                ulong currentChannelID = ctx.Guild.GetValue("invite_channel").ULong();
                if (currentChannelID == channel.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Invite channel already set to: ", $"{channel.Mention}", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("invite_channel", channel.Id);
                await ctx.Channel.SendEmbedMessage($"Invite channel set to: ", $"{channel.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unset_invite_channel")]
        [Description("Unsets the channel to which invites created using the \".invite\" invite to")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetInviteChannel(CommandContext ctx)
        {
            try
            {
                ulong currentChannelID = ctx.Guild.GetValue("invite_channel").ULong();
                if (currentChannelID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"Invite channel already not set", $"", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("invite_channel", "");
                await ctx.Channel.SendEmbedMessage($"Invite channel unset", $"", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_invite_channel")]
        [Description("Gets the channel to which invites created using the \".invite\" invite to")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetInviteChannel(CommandContext ctx)
        {
            try
            {
                DiscordChannel channel = ctx.Guild.GetValue("invite_channel").DiscordChannel(ctx.Guild);
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"Invite channel not set", $"", Bot.ColorSystem);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"Invite channel:", $"{channel.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Inviter role
        [Command("set_inviter_role")]
        [Description("Sets the role that can user the \".invite\" command to create invites")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetInviterRole(CommandContext ctx, DiscordRole role)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("inviter_role").ULong();
                if (currentRoleID == role.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Inviter role already set to: ", $"{role.Mention}", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("inviter_role", role.Id);
                await ctx.Channel.SendEmbedMessage($"Inviter role set to: ", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unset_inviter_role")]
        [Description("Unsets the role that can use the \".invite\" command to create invites")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetInviterRole(CommandContext ctx)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("inviter_role").ULong();
                if (currentRoleID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"Inviter role already not set", $"", Bot.ColorWarning);
                    return;
                }

                ctx.Guild.SetValue("inviter_role", "");
                await ctx.Channel.SendEmbedMessage($"Inviter role unset", $"", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_inviter_role")]
        [Description("Gets the role that can user the \".invite\" command to create invites")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetInviterRole(CommandContext ctx)
        {
            try
            {
                DiscordRole role = ctx.Guild.GetValue("inviter_role").DiscordRole(ctx.Guild);
                if (role == null)
                {
                    await ctx.Channel.SendEmbedMessage($"Inviter role not set", $"", Bot.ColorSystem);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"Inviter role:", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Blocker bypass role
        [Command("set_invite_blocker_bypass_role")]
        [Description("Sets the role that can bypass invite blocks")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetInviteBlockerBypassRole(CommandContext ctx, DiscordRole role)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("invite_blocker_bypass_role").ULong();
                if (currentRoleID == role.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Invite blocker bypass role already set to: ", $"{role.Mention}", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("invite_blocker_bypass_role", role.Id);
                await ctx.Channel.SendEmbedMessage($"Invite blocker bypass role set to: ", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unset_invite_blocker_bypass_role")]
        [Description("Unsets the role that can bypass invite blocks")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetInviteBlockerBypassRole(CommandContext ctx)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("invite_blocker_bypass_role").ULong();
                if (currentRoleID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"Invite blocker bypass role already not set", $"", Bot.ColorWarning);
                    return;
                }

                ctx.Guild.SetValue("invite_blocker_bypass_role", "");
                await ctx.Channel.SendEmbedMessage($"Invite blocker bypass role unset", $"", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_invite_blocker_bypass_role")]
        [Description("Gets the role that can bypass invite blocks")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetInviteBlockerBypassRole(CommandContext ctx)
        {
            try
            {
                DiscordRole role = ctx.Guild.GetValue("invite_blocker_bypass_role").DiscordRole(ctx.Guild);
                if (role == null)
                {
                    await ctx.Channel.SendEmbedMessage($"Invite blocker bypass role not set", $"", Bot.ColorSystem);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"Invite blocker bypass role:", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Invite block
        [Command("invite_block")]
        [Description("Shows whether the invite blocker is enabled or disabled")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task InviteBlock(CommandContext ctx)
        {
            try
            {
                await ctx.Channel.SendEmbedMessage($"Invite block is {(ctx.Guild.GetValue("invites_blocked") == "1" ? "en" : "dis")}abled", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("invite_block")]
        [Description("Enable/Disable the invite blocker")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task InviteBlockSet(CommandContext ctx, bool enable)
        {
            try
            {
                bool currentlyEnabled = ctx.Guild.GetValue("invites_blocked") == "1";
                if (enable == currentlyEnabled)
                    await ctx.Channel.SendEmbedMessage($"Invite block is already {(ctx.Guild.GetValue("invites_blocked") == "1" ? "en" : "dis")}abled", "", Bot.ColorSystem);
                else
                {
                    ctx.Guild.SetValue("invites_blocked", enable ? "1" : "0");
                    await ctx.Channel.SendEmbedMessage($"Invite block {(enable ? "en" : "dis")}abled", "", Bot.ColorSystem);
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("create_invite"), Aliases("invite")]
        [Description("Creates an invite to the set invite channel for one user that expires in one hour")]
        public async Task CreateInvite(CommandContext ctx)
        {
            if (!ctx.Member.Roles.Any(x => x.Id == ctx.Guild.GetValue("inviter_role").ULong() || x.Id == ctx.Guild.GetValue("invite_blocker_bypass_role").ULong()))
            {
                await ctx.Channel.SendEmbedMessage($"{ctx.User.Username} not allowed to create invites", $"", Bot.ColorError);
                return;
            }

            try
            {
                DiscordChannel channel = ctx.Guild.GetValue("invite_channel").DiscordChannel(ctx.Guild);
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"Could not create invite", $"Invite channel has not been set", Bot.ColorWarning);
                    return;
                }

                DiscordInvite invite = await channel.CreateInviteAsync(3600, 1, false, true, $"{ctx.User.Username} ({ctx.User.Id}) user requested invite");
                await ctx.Channel.SendEmbedMessage($"Invite created", $"📨 Link sent to {ctx.Member.Mention} DMs", Bot.ColorMain);
                await ctx.Member.SendMessageAsync($"https://discord.gg/{invite.Code}");
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("nuke_invites")]
        [Description("Deletes all invites")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task DeleteUserInvites(CommandContext ctx)
        {
            try
            {
                var invites = await ctx.Guild.GetInvitesAsync();
                if (invites.Count == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"No invites to delete", "", Bot.ColorSystem);
                    return;
                }

                foreach (DiscordInvite invite in invites)
                    await invite.DeleteAsync();

                await ctx.Channel.SendEmbedMessage($"All {invites.Count} invites deleted", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("delete_user_invites")]
        [Description("Deletes all given user's invites")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task DeleteUserInvites(CommandContext ctx, DiscordUser user)
        {
            try
            {
                int deletedCount = 0;
                var invites = await ctx.Guild.GetInvitesAsync();
                foreach (DiscordInvite invite in invites)
                {
                    if (invite.Inviter.Id == user.Id)
                    {
                        await invite.DeleteAsync();
                        deletedCount++;
                    }
                }

                await ctx.Channel.SendEmbedMessage($"{deletedCount} {user.Username}'s invites deleted", "", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("delete_my_invites")]
        [Description("Deletes all command caller's invites")]
        public async Task DeleteMyInvites(CommandContext ctx)
        {
            await DeleteUserInvites(ctx, ctx.User);
        }
    }
}