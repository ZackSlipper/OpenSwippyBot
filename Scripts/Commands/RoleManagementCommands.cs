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
    public class RoleManagementCommands : BaseCommandModule
    {
        //New joiner
        [Command("new_joiner_add_role")]
        [Description("Add a role to new joiner role list so it would be added to all new users")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task AddRole(CommandContext ctx, DiscordRole role, [Description("Give all existing users the role")] bool addToEveryone = true)
        {
            if (Bot.NewJoinerRoles.AddServerRole(ctx.Guild, role))
            {
                await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" added to the new joiner role list", "", Bot.ColorSystem);
                if (addToEveryone)
                    await AddRoleEveryone(ctx, role);
            }
            else
                await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" already in the new joiner role list", "", Bot.ColorWarning);
        }

        [Command("new_joiner_remove_role")]
        [Description("Remove a role from the new joiner role list")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task RemoveRole(CommandContext ctx, DiscordRole role)
        {
            if (Bot.NewJoinerRoles.RemoveServerRole(ctx.Guild, role))
                await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" removed from the new joiner role list", "", Bot.ColorSystem);
            else
                await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" already doesn't exist in the new joiner role list", "", Bot.ColorWarning);
        }


        [Command("new_joiner_list_roles")]
        [Description("List all role added to the server's new joiners")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ListRoles(CommandContext ctx)
        {
            List<DiscordRole> roles = Bot.GetValues(ctx.Guild.Id).NewJoinerRoles.Roles;
            StringBuilder sb = new StringBuilder();
            foreach (DiscordRole role in roles)
                sb.AppendLine(role.Mention);

            if (sb.Length > 0)
                await ctx.Channel.SendEmbedMessage("New Joiner Roles:", sb.ToString(), Bot.ColorSystem);
            else
                await ctx.Channel.SendEmbedMessage("No New Joiner Roles", "", Bot.ColorSystem);
        }

        //Everyone
        [Command("add_role_everyone")]
        [Description("Add a given role to every user of the server except bots")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task AddRoleEveryone(CommandContext ctx, DiscordRole role)
        {
            DiscordMessage loadingMessage = await ctx.Channel.SendEmbedMessage("Adding role...", "", Bot.ColorSystem);

            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                try
                {
                    if (!member.IsBot)
                        await member.GrantRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }

            await loadingMessage.DeleteAsync();
            await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" granted to everyone", "", Bot.ColorSystem);
        }

        [Command("remove_role_everyone")]
        [Description("Remove a given role from every user of the server except bots")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task RemoveRoleEveryone(CommandContext ctx, DiscordRole role)
        {
            DiscordMessage loadingMessage = await ctx.Channel.SendEmbedMessage("Removing role...", "", Bot.ColorSystem);

            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                try
                {
                    if (!member.IsBot)
                        await member.RevokeRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }

            await loadingMessage.DeleteAsync();
            await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" revoked from everyone", "", Bot.ColorSystem);
        }

        //With
        [Command("add_role_everyone_with_roles")]
        [Description("Add a given role to every user of the server that has the given roles")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task AddRoleEveryoneWithRoles(CommandContext ctx, DiscordRole role, params DiscordRole[] requiredRoles)
        {
            DiscordMessage loadingMessage = await ctx.Channel.SendEmbedMessage("Adding role...", "", Bot.ColorSystem);

            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                try
                {
                    if (requiredRoles.All(x => member.Roles.Contains(x)))
                        await member.GrantRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (DiscordRole requiredRole in requiredRoles)
                sb.AppendLine(requiredRole.Mention);

            await loadingMessage.DeleteAsync();
            await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" granted to everyone with roles:", sb.ToString(), Bot.ColorSystem);
        }

        [Command("remove_role_everyone_with_roles")]
        [Description("Remove a given role from every user of the server that has the given roles")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task RemoveRoleEveryoneWithRoles(CommandContext ctx, DiscordRole role, params DiscordRole[] requiredRoles)
        {
            DiscordMessage loadingMessage = await ctx.Channel.SendEmbedMessage("Removing role...", "", Bot.ColorSystem);

            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                try
                {
                    if (requiredRoles.All(x => member.Roles.Contains(x)))
                        await member.RevokeRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (DiscordRole requiredRole in requiredRoles)
                sb.AppendLine(requiredRole.Mention);

            await loadingMessage.DeleteAsync();
            await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" revoked from everyone with roles:", sb.ToString(), Bot.ColorSystem);
        }

        //Without
        [Command("add_role_everyone_without_roles")]
        [Description("Add a given role to every user of the server that doesn't have the given roles")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task AddRoleEveryoneWithoutRoles(CommandContext ctx, DiscordRole role, params DiscordRole[] roles)
        {
            DiscordMessage loadingMessage = await ctx.Channel.SendEmbedMessage("Adding role...", "", Bot.ColorSystem);

            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                try
                {
                    if (roles.All(x => !member.Roles.Contains(x)))
                        await member.GrantRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (DiscordRole requiredRole in roles)
                sb.AppendLine(requiredRole.Mention);

            await loadingMessage.DeleteAsync();
            await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" granted to everyone without roles:", sb.ToString(), Bot.ColorSystem);
        }

        [Command("remove_role_everyone_without_roles")]
        [Description("Remove a given role from every user of the server that doesn't have the given roles")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task RemoveRoleEveryoneWithoutRoles(CommandContext ctx, DiscordRole role, params DiscordRole[] roles)
        {
            DiscordMessage loadingMessage = await ctx.Channel.SendEmbedMessage("Removing role...", "", Bot.ColorSystem);

            var members = await ctx.Guild.GetAllMembersAsync();
            foreach (var member in members)
            {
                try
                {
                    if (roles.All(x => !member.Roles.Contains(x)))
                        await member.RevokeRoleAsync(role);
                }
                catch (System.Exception ex)
                {
                    Bot.ReportError(ex);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (DiscordRole requiredRole in roles)
                sb.AppendLine(requiredRole.Mention);

            await loadingMessage.DeleteAsync();
            await ctx.Channel.SendEmbedMessage($"Role \"{role.Name}\" revoked from everyone without roles:", sb.ToString(), Bot.ColorSystem);
        }
    }
}