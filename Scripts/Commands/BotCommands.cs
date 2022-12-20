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
    public class BotCommands : BaseCommandModule
    {
        Dictionary<string, string> argTypeMap = new Dictionary<string, string>()
        {
            { "String", "Text" },
            { "Boolean", "true/false" },
            { "UInt32", "Positive Integer" },
            { "Int32", "Integer" },
            { "UInt64", "ID" },
            { "DiscordChannel", "Channel" },
            { "DiscordRole", "Role" },
            { "DiscordUser", "User" },
            { "DiscordMember", "User" },
            { "Uri", "URL" },
            { "Single", "Real Number" },
            { "Double", "Real Number" },
        };

        [Command("h"), Aliases("help")]
        //[Description("List all availble modules and their commands")]
        public async Task Help(CommandContext ctx)
        {
            try
            {
                List<Page> pages = new List<Page>();
                List<Tuple<string, List<string>>> content = new List<Tuple<string, List<string>>>();

                //Build content
                Tuple<string, List<string>> moduleContent;
                foreach (string moduleName in Bot.ModuleCommands.Keys)
                {
                    moduleContent = ModuleContent(moduleName, ctx);
                    if (moduleContent.Item2.Count == 0)
                        continue;

                    content.Add(moduleContent);
                }

                //Add module page
                StringBuilder sbModules = new StringBuilder();
                foreach (Tuple<string, List<string>> item in content)
                    sbModules.AppendLine($"> **{item.Item1}**{Environment.NewLine}");
                content.Insert(0, new Tuple<string, List<string>>("Modules", new List<string>() { sbModules.ToString() }));

                //Build pages
                int pageCount = 0;
                foreach (Tuple<string, List<string>> item in content)
                    pageCount += item.Item2.Count;

                int index = 0;
                foreach (Tuple<string, List<string>> item in content)
                {
                    foreach (string pageContent in item.Item2)
                    {
                        index++;

                        pages.Add(new Page("", new DiscordEmbedBuilder()
                        {
                            Title = $":grey_question: Help: {item.Item1}  ( page {index}/{pageCount} )",
                            Description = pageContent,
                            Color = Bot.ColorMain
                        }));
                    }
                }

                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, null, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteMessage, TimeSpan.FromMinutes(10));
                await ctx.Message.DeleteAsync();
            }
            catch (NotFoundException) { } //Ignore deleted message error
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("h"), Aliases("?")]
        [Description("List all modules and their commands or a specified module's or command's info")]
        public async Task ModuleOrCommandHelp(CommandContext ctx, [RemainingText] string name)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                string oldName = $"{name}";
                name = name.ToLower();

                List<Page> pages = new List<Page>();
                Tuple<string, List<string>> content = null;

                //Get module info
                foreach (string moduleName in Bot.ModuleCommands.Keys)
                {
                    if (Bot.ModuleNameMap[moduleName].ToLower() == name)
                    {
                        content = ModuleContent(moduleName, ctx);
                        break;
                    }
                }

                //Get command info
                if (content == null)
                {
                    if (Bot.Commands.RegisteredCommands.ContainsKey(name))
                        content = new Tuple<string, List<string>>(name, new List<string>() { CommandInfo(Bot.Commands.RegisteredCommands[name]) });
                    else
                    {
                        await ctx.Channel.SendTimedEmbedMessage($"No module or command with name \"{oldName}\" found", "", Bot.ColorError, TimeSpan.FromSeconds(20));
                        return;
                    }
                }

                if (content.Item2.Count == 0)
                {
                    await ctx.Channel.SendTimedEmbedMessage($"You dont have access to any commands in the \"{oldName}\" module", "", Bot.ColorError, TimeSpan.FromSeconds(20));
                    return;
                }

                if (content != null)
                {
                    //Build pages
                    int index = 0;
                    foreach (string pageContent in content.Item2)
                    {
                        index++;

                        pages.Add(new Page("", new DiscordEmbedBuilder()
                        {
                            Title = $":grey_question: Help: {content.Item1}  ( page {index}/{content.Item2.Count} )",
                            Description = pageContent
                        }));
                    }

                    await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, null, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteMessage, TimeSpan.FromMinutes(5));
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        Tuple<string, List<string>> ModuleContent(string moduleName, CommandContext ctx)
        {
            List<string> content = new List<string>();
            int commandsCount = 0;

            StringBuilder sb = new StringBuilder();
            StringBuilder sbModule = new StringBuilder();

            foreach (Command command in Bot.ModuleCommands[moduleName])
            {
                if (command.ExecutionChecks.Any(x => !x.ExecuteCheckAsync(ctx, true).GetAwaiter().GetResult()))
                    continue;

                commandsCount++;

                sb.Append(CommandInfo(command));

                if (sb.Length > 1100)
                {
                    content.Add(sb.ToString());
                    sb.Clear();
                }
                else
                    sb.AppendLine("");
            }
            content.Add(sb.ToString());

            if (commandsCount == 0)
                content.Clear();
            return new Tuple<string, List<string>>(Bot.ModuleNameMap[moduleName], content);
        }

        String CommandInfo(Command command)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbAliases = new StringBuilder();
            StringBuilder sbArgs = new StringBuilder();

            if (command.Aliases.Count > 0)
            {
                for (int i = 0; i < command.Aliases.Count; i++)
                    sbAliases.Append($"**{command.Aliases[i]}**{(i == command.Aliases.Count - 1 ? "" : ", ")}");
            }

            sb.AppendLine($"• **{command.Name}**{(sbAliases.Length > 0 ? $" ( {sbAliases.ToString()} )" : "")}{(string.IsNullOrWhiteSpace(command.Description) ? "" : $" - {command.Description}")}");
            sbAliases.Clear();

            foreach (CommandOverload overload in command.Overloads)
            {
                if (overload.Arguments.Count == 0)
                    continue;

                sbArgs.Clear();
                sb.Append("　　　- ");
                for (int i = 0; i < overload.Arguments.Count; i++)
                    sb.Append($"*{overload.Arguments[i].Name}* ({argTypeMap[overload.Arguments[i].Type.Name]}) {(i == overload.Arguments.Count - 1 ? "" : ", ")}");
                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }

        [Command("bot_stats"), Aliases("bstats", "bs")]
        [Description("Get bot stats and info")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Stats(CommandContext ctx)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                //Bot log channel
                ulong serverID, channelID;
                if (!ulong.TryParse(Bot.GlobalData.GetValue("bot_log_channel"), out channelID) || !ulong.TryParse(Bot.GlobalData.GetValue("bot_log_server"), out serverID) || await ctx.Client.GetChannelAsync(channelID) == null)
                    sb.AppendLine("• Bot log: [Unset]");
                else
                {
                    sb.AppendLine($"• Bot log: \"{(await ctx.Client.GetChannelAsync(channelID)).Name}\" () channel in \"{(await ctx.Client.GetGuildAsync(serverID)).Name}\" server");

                    DiscordChannel channel = await ctx.Client.GetChannelAsync(channelID);
                    sb.AppendLine($"　- Channel: {channel.Name} ({channel.Id})");
                    sb.AppendLine($"　　　{channel.Mention}");

                    DiscordGuild server = await ctx.Client.GetGuildAsync(serverID);
                    sb.AppendLine($"　- Server: {server.Name} ({server.Id})");
                }
                sb.AppendLine("");

                //Active music players
                StringBuilder playerSb = new StringBuilder();
                int activePlayerCount = 0;

                ServerValues[] serverValues = Bot.GetAllValues();
                foreach (ServerValues values in serverValues)
                {
                    if (values.Player.activeChannel != null)
                    {
                        activePlayerCount++;

                        playerSb.AppendLine($"　‣ **Active player** in \"{values.Player.activeChannel.Guild.Name}\" ({values.Player.activeChannel.Guild.Id}) server:");
                        playerSb.AppendLine($"　　- Channel: {values.Player.activeChannel.Name} ({values.Player.activeChannel.Id})");
                        playerSb.AppendLine($"　　- User Count: {values.Player.activeChannel.Users.Count - 1} (not counting the bot)");
                        if (values.Player.isPlaying && values.Player.connection.CurrentState != null && values.Player.connection.CurrentState.CurrentTrack != null)
                        {
                            playerSb.AppendLine($"　　- State: Playing");
                            playerSb.AppendLine($"　　　‣ Track:");
                            playerSb.AppendLine($"　　　　- URL: {values.Player.connection.CurrentState.CurrentTrack.Uri}");
                            playerSb.AppendLine($"　　　　- Author: {values.Player.connection.CurrentState.CurrentTrack.Author}");
                            playerSb.AppendLine($"　　　　- Title: {values.Player.connection.CurrentState.CurrentTrack.Title}");
                            playerSb.AppendLine($"　　　　- Length: {values.Player.connection.CurrentState.CurrentTrack.Length}");
                            playerSb.AppendLine($"　　　　- Playback Position: {values.Player.connection.CurrentState.PlaybackPosition}");
                        }
                        else
                            playerSb.AppendLine($"　　- State: Idle");
                    }
                }

                sb.AppendLine($"• Active player count: {activePlayerCount}");
                if (activePlayerCount > 0)
                    sb.AppendLine(playerSb.ToString());

                //Print stats
                await ctx.Message.DeleteAsync();
                await ctx.Channel.SendTimedEmbedMessage($"Bot stats:", sb.ToString(), Bot.ColorMain, TimeSpan.FromMinutes(1));
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Global values
        [Command("list_global_keys")]
        [Description("Get list global data keys")]
        [RequireOwner]
        public async Task ListGlobalKeys(CommandContext ctx)
        {
            try
            {
                string[] keys = Bot.GlobalData.GetKeys();
                StringBuilder sb = new StringBuilder();
                foreach (string key in keys)
                    sb.AppendLine(key);

                await ctx.Channel.SendEmbedMessage($"Global value keys :", sb.ToString(), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_global_value")]
        [Description("Get global data value at given key")]
        [RequireOwner]
        public async Task GetGlobalValue(CommandContext ctx, string key)
        {
            try
            {
                await ctx.Channel.SendEmbedMessage($"Value at key \'{key}\' is: ", Bot.GlobalData.GetValue(key), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_global_value")]
        [Description("Set global data value at given key")]
        [RequireOwner]
        public async Task SetGlobalValue(CommandContext ctx, string key, string value)
        {
            try
            {
                Bot.GlobalData.SetValue(key, value);
                await ctx.Channel.SendEmbedMessage($"Set value at key \'{key}\' to: ", value, Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Server values
        [Command("list_server_keys")]
        [Description("Get list server data keys")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ListServerKeys(CommandContext ctx)
        {
            try
            {
                string[] keys = Bot.GetSeverData(ctx.Guild.Id).GetServerKeys();
                StringBuilder sb = new StringBuilder();
                foreach (string key in keys)
                    sb.AppendLine(key);

                await ctx.Channel.SendEmbedMessage($"Server value keys :", sb.ToString(), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_server_value")]
        [Description("Get server data value at given key")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetServerValue(CommandContext ctx, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ctx.Guild.GetValue(key)))
                    await ctx.Channel.SendEmbedMessage($"Value at key \'{key}\' is empty", "", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"Value at key \'{key}\' is: ", ctx.Guild.GetValue(key), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_server_value")]
        [Description("Set server data value at given key")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetServerValue(CommandContext ctx, string key, string value)
        {
            try
            {
                ctx.Guild.SetValue(key, value);
                await ctx.Channel.SendEmbedMessage($"Set value at key \'{key}\' to: ", value, Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //User Values
        [Command("list_user_keys")]
        [Description("Get list of all user's data keys")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task ListUserKeys(CommandContext ctx, DiscordUser user)
        {
            try
            {
                string[] keys = Bot.GetSeverData(ctx.Guild.Id).GetUserKeys(user.Id);
                StringBuilder sb = new StringBuilder();
                foreach (string key in keys)
                    sb.AppendLine(key);

                await ctx.Channel.SendEmbedMessage($"{user.Username}' value keys :", sb.ToString(), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_user_value")]
        [Description("Get user data value at given key")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetServerValue(CommandContext ctx, DiscordUser user, string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ctx.Guild.GetUserValue(user.Id, key)))
                    await ctx.Channel.SendEmbedMessage($"Value at key \'{key}\' is empty", "", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"Value at key \'{key}\' is: ", ctx.Guild.GetUserValue(user.Id, key), Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_user_value")]
        [Description("Set user data value at given key")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetServerValue(CommandContext ctx, DiscordUser user, string key, string value)
        {
            try
            {
                ctx.Guild.SetUserValue(user.Id, key, value);
                await ctx.Channel.SendEmbedMessage($"{user.Username}' value at key \'{key}\' set to: ", value, Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Bot log channel
        [Command("bot_log_set_channel")]
        [Description("Set the bot log channel")]
        [RequireOwner]
        public async Task SetLogChannel(CommandContext ctx, DiscordChannel channel)
        {
            try
            {
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"\"{ctx.Command.Name}\" Command Error: Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                ulong oldChannelID;
                if (ulong.TryParse(Bot.GlobalData.GetValue("bot_log_channel"), out oldChannelID) && oldChannelID == channel.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Bot log channel already set to \"{channel.Name}\"", "", Bot.ColorWarning);
                    return;
                }

                Bot.GlobalData.SetValue("bot_log_server", channel.Guild.Id.ToString());
                Bot.GlobalData.SetValue("bot_log_channel", channel.Id.ToString());
                await ctx.Channel.SendEmbedMessage($"Bot log channel set to \"{channel.Name}\" in \"{channel.Guild.Name}\" server", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("bot_log_get_channel")]
        [Description("Get the currently set bot log channel")]
        [RequireOwner]
        public async Task GetLogChannel(CommandContext ctx)
        {
            try
            {
                ulong channelID = 0, serverID = 0;
                if (!ulong.TryParse(Bot.GlobalData.GetValue("bot_log_channel"), out channelID) || !ulong.TryParse(Bot.GlobalData.GetValue("bot_log_server"), out serverID) || (await ctx.Client.GetGuildAsync(serverID)) == null || (await ctx.Client.GetChannelAsync(channelID)) == null)
                    await ctx.Channel.SendEmbedMessage($"Bot log channel is not set", "", Bot.ColorMain);
                else
                    await ctx.Channel.SendEmbedMessage($"Bot log channel is set to \"{(await ctx.Client.GetChannelAsync(channelID)).Name}\" in server \"{(await ctx.Client.GetGuildAsync(serverID)).Name}\"", "", Bot.ColorMain);
            }
            catch (System.Exception)
            {
                await ctx.Channel.SendEmbedMessage($"Bot log channel is not set", "", Bot.ColorMain);
            }
        }

        //Nuke
        [Command("purge")]
        [Description("Deletes specified amount of messages in a given channel that are less than specified time (Max 14 days old)")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task Purge(CommandContext ctx, DiscordChannel channel, int messageCount = 100, int days = 13, int hours = 23, int minutes = 59)
        {
            try
            {
                if (channel == null)
                {
                    await ctx.Channel.SendEmbedMessage($"Channel doesn't exist", "", Bot.ColorError);
                    return;
                }

                await ctx.Channel.DeleteMessageAsync(ctx.Message);

                IReadOnlyList<DiscordMessage> messages = await channel.GetMessagesAsync(messageCount);
                IEnumerable<DiscordMessage> filteredMessages = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp) <= new TimeSpan(days, hours, minutes, 0));

                if (filteredMessages.Count() > 0)
                {
                    await channel.DeleteMessagesAsync(filteredMessages);
                    ctx.Channel.SendTimedEmbedMessage($"Successfully deleted {filteredMessages.Count()} messages in \'{channel.Name}\'", "", Bot.ColorMain, TimeSpan.FromSeconds(10)).GetAwaiter();
                }
                else
                    ctx.Channel.SendTimedEmbedMessage($"No messages can be deleted", "", Bot.ColorError, TimeSpan.FromSeconds(10)).GetAwaiter();
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("nuke_user_messages"), Aliases("num")]
        [Description("Deletes a given amount of user's messages in a given channel")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task NukeUserMessages(CommandContext ctx, DiscordChannel channel, ulong userID, int messageCount)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                if (messageCount < 1)
                {
                    await ctx.Channel.SendEmbedMessage($"Message count too small", "", Bot.ColorError);
                    return;
                }

                if (messageCount < 1)
                {
                    await ctx.Channel.SendEmbedMessage($"Message count too small", "", Bot.ColorError);
                    return;
                }

                int deletedCount = 0;

                var messages = (await channel.GetMessagesAsync(5000)).Where(x => x.Author.Id == userID).ToArray();
                foreach (var message in messages)
                {
                    await message.DeleteAsync();
                    deletedCount++;
                    if (deletedCount >= messageCount)
                        break;
                }

                ctx.Channel.SendTimedEmbedMessage($"Deleted {deletedCount} user's with id {userID} messages", "", Bot.ColorMain, TimeSpan.FromSeconds(10)).GetAwaiter();
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("nuke_user_messages")]
        [Description("Deletes a given amount of user's messages in a given channel")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task NukeUserMessages(CommandContext ctx, DiscordChannel channel, DiscordMember user, int messageCount)
        {
            await NukeUserMessages(ctx, channel, user.Id, messageCount);
        }
    }
}