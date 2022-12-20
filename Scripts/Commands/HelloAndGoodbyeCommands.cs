using System.IO.Compression;
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

namespace SwippyBot;

public class HelloAndGoodbyeCommands : BaseCommandModule
{
    [Command("set_hello_channel")]
    [Description("Set the channel hello and goodbye channel")]
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

            ulong oldChannelID = ctx.Guild.GetValue("hello_channel").ULong();
            if (oldChannelID != 0 && oldChannelID == channel.Id)
            {
                await ctx.Channel.SendEmbedMessage($"Hello and goodbye channel already set to:", channel.Mention, Bot.ColorWarning);
                return;
            }

            Bot.WordChain.RemoveServerData(ctx.Guild);
            ctx.Guild.SetValue("hello_channel", channel.Id);
            Bot.WordChain.AddServerData(ctx.Guild);

            await ctx.Channel.SendEmbedMessage($"Hello and goodbye channel set to:", channel.Mention, Bot.ColorMain);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("unset_hello_channel")]
    [Description("Unset the hello and goodbye channel")]
    [RequireRoles(RoleCheckMode.Any, "Swippy")]
    public async Task UnsetChannel(CommandContext ctx)
    {
        try
        {
            ulong oldChannelID = ctx.Guild.GetValue("hello_channel").ULong();
            if (oldChannelID == 0)
            {
                await ctx.Channel.SendEmbedMessage($"Hello and goodbye channel not set already", "", Bot.ColorWarning);
                return;
            }

            Bot.WordChain.RemoveServerData(ctx.Guild);
            ctx.Guild.SetValue("word_chain_channel", "");

            await ctx.Channel.SendEmbedMessage($"Hello and goodbye channel unset", "", Bot.ColorMain);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("get_hello_channel")]
    [Description("Get the currently set hello and goodbye channel")]
    [RequireRoles(RoleCheckMode.Any, "Swippy")]
    public async Task GetChannel(CommandContext ctx)
    {
        try
        {
            DiscordChannel channel = ctx.Guild.GetValueChannel("hello_channel");
            if (channel == null)
                await ctx.Channel.SendEmbedMessage($"Hello and goodbye channel not set", "", Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"Hello and goodbye channel set to:", channel.Mention, Bot.ColorMain);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("user_join_leave_notification_subscribe")]
    [RequireRoles(RoleCheckMode.Any, "Swippy", "🐺 | Bwo", "🐇 | BunnyHop")]
    public async Task UserJoinLeaveNotificationSubscribe(CommandContext ctx)
    {
        try
        {
            if (!Bot.Staff.IsStaff(ctx.Member))
            {
                await ctx.Channel.SendEmbedMessage($"🐾  Only server staff can use this command", "", Bot.ColorError);
                return;
            }

            if (Bot.HelloAndGoodbye.SubscribeMember(ctx.Member))
                await ctx.Channel.SendEmbedMessage($"🐾  Successfully subscribed to user join and leave notifications", "", Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"🐾  You're already subscribed to user join and leave notifications", "", Bot.ColorWarning);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("user_join_leave_notification_unsubscribe")]
    [RequireRoles(RoleCheckMode.Any, "Swippy", "🐺 | Bwo", "🐇 | BunnyHop")]
    public async Task UserJoinLeaveNotificationUnsubscribe(CommandContext ctx)
    {
        try
        {
            if (!Bot.Staff.IsStaff(ctx.Member))
            {
                await ctx.Channel.SendEmbedMessage($"🐾  Only server staff can use this command", "", Bot.ColorError);
                return;
            }

            if (Bot.HelloAndGoodbye.UnsubscribeMember(ctx.Member))
                await ctx.Channel.SendEmbedMessage($"🐾  Successfully unsubscribed from user join and leave notifications", "", Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"🐾  You're already unsubscribed from user join and leave notifications", "", Bot.ColorWarning);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("user_join_leave_notification_subscribers")]
    [RequireRoles(RoleCheckMode.Any, "Swippy", "🐺 | Bwo", "🐇 | BunnyHop")]
    public async Task UserJoinLeaveNotificationSubscribers(CommandContext ctx)
    {
        try
        {
            if (!Bot.Staff.IsStaff(ctx.Member))
            {
                await ctx.Channel.SendEmbedMessage($"🐾  Only server staff can use this command", "", Bot.ColorError);
                return;
            }

            List<DiscordMember> subscribers = Bot.HelloAndGoodbye.GetSubscribedMembers(ctx.Guild);
            if (subscribers.Count == 0)
            {
                await ctx.Channel.SendEmbedMessage($"🐾  No user join and leave notification subscribers", "", Bot.ColorMain);
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (DiscordMember subscriber in subscribers)
                sb.AppendLine($"• {subscriber.Mention}");

            await ctx.Channel.SendEmbedMessage($"🐾  User join and leave notification subscribers:", sb.ToString(), Bot.ColorMain);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("set_new_joiner_info_message")]
    [RequireRoles(RoleCheckMode.Any, "Swippy")]
    public async Task SetNewJoinerInfoMessage(CommandContext ctx)
    {
        try
        {
            await ctx.Message.DeleteAsync();

            string title = ctx.Guild.GetValue("newJoinerInfoTitle");
            string description = ctx.Guild.GetValue("newJoinerInfoDescription");
            string sticker = ctx.Guild.GetValue("newJoinerInfoImage");

            if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(description))
            {
                ctx.Channel.SendTimedEmbedMessage("🐾  The new joiner info message is already set", "", Bot.ColorWarning, TimeSpan.FromSeconds(20)).GetAwaiter();
                return;
            }

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            //Title
            DiscordMessage instructionMessage = await ctx.Channel.SendEmbedMessage("🐾  Please enter the title of the new joiner info message", $"Type \".cancel\" to cancel this command{Environment.NewLine}The \"[ServerName]\" tag will be replaced with the current server's name", Bot.ColorMain);
            DiscordMessage message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && !x.Author.IsBot && (x.Content.ToLower() == ".cancel" || !x.Content.StartsWith(Bot.Prefix)), TimeSpan.FromMinutes(15))).Result;
            await instructionMessage.DeleteAsync();

            if (message == null || message.Content.ToLower() == ".cancel")
            {
                ctx.Channel.SendTimedEmbedMessage("🐾  Waiting for title timed out or was canceled", "", Bot.ColorError, TimeSpan.FromSeconds(20)).GetAwaiter();
                await message?.DeleteAsync();
                return;
            }

            title = message.Content;
            await message.DeleteAsync();

            //Description
            instructionMessage = await ctx.Channel.SendEmbedMessage("🐾  Please enter the description of the new joiner info message", $"Type \".cancel\" to cancel this command", Bot.ColorMain);
            message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && !x.Author.IsBot && (x.Content.ToLower() == ".cancel" || !x.Content.StartsWith(Bot.Prefix)), TimeSpan.FromMinutes(15))).Result;
            await instructionMessage.DeleteAsync();

            if (message == null || message.Content.ToLower() == ".cancel")
            {
                ctx.Channel.SendTimedEmbedMessage("🐾  Waiting for description timed out or was canceled", "", Bot.ColorError, TimeSpan.FromSeconds(20)).GetAwaiter();
                await message?.DeleteAsync();
                return;
            }

            description = message.Content;
            await message.DeleteAsync();

            //Image
            instructionMessage = await ctx.Channel.SendEmbedMessage("🐾  Please post a sticker to be used as an image for the new joiner info message", $"Type \".cancel\" to cancel this command or \".skip\" to skip this step", Bot.ColorMain);
            while (true)
            {
                message = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && !x.Author.IsBot && (x.Content.ToLower() == ".cancel" || x.Content.ToLower() == ".skip" || !x.Content.StartsWith(Bot.Prefix)), TimeSpan.FromMinutes(5))).Result;

                if (message == null || message.Content.ToLower() == ".cancel")
                {
                    ctx.Channel.SendTimedEmbedMessage("🐾  Waiting for sticker timed out or was canceled", "", Bot.ColorError, TimeSpan.FromSeconds(20)).GetAwaiter();
                    await message?.DeleteAsync();
                    await instructionMessage.DeleteAsync();
                    return;
                }

                if (message.Content.ToLower() == ".skip")
                {
                    await message.DeleteAsync();
                    break;
                }

                if (message.Stickers.Count == 0)
                {
                    await message.DeleteAsync();
                    ctx.Channel.SendTimedEmbedMessage("🐾  No sticker given. Please provide a sticker", "", Bot.ColorError, TimeSpan.FromSeconds(20)).GetAwaiter();
                    continue;
                }

                sticker = message.Stickers[0].StickerUrl;
                await message.DeleteAsync();
                break;
            }
            await instructionMessage.DeleteAsync();

            ctx.Guild.SetValue("newJoinerInfoTitle", title);
            ctx.Guild.SetValue("newJoinerInfoDescription", description);
            ctx.Guild.SetValue("newJoinerInfoImage", sticker);

            await ctx.Channel.SendEmbedMessage("🐾  New joiner info message created successfully!", "Preview of the welcome message is displayed below:", Bot.ColorMain);
            await ctx.Channel.SendMessageAsync(Bot.HelloAndGoodbye.NewJoinerInfoMessageEmbed(ctx.Guild));
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("unset_new_joiner_info_message")]
    [RequireRoles(RoleCheckMode.Any, "Swippy")]
    public async Task UnsetNewJoinerInfoMessage(CommandContext ctx)
    {
        try
        {
            await ctx.Message.DeleteAsync();

            string title = ctx.Guild.GetValue("newJoinerInfoTitle");
            string description = ctx.Guild.GetValue("newJoinerInfoDescription");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                ctx.Channel.SendTimedEmbedMessage("🐾  New joiner info message has already been unset", "", Bot.ColorWarning, TimeSpan.FromSeconds(20)).GetAwaiter();
                return;
            }

            ctx.Guild.SetValue("newJoinerInfoTitle", "");
            ctx.Guild.SetValue("newJoinerInfoDescription", "");
            ctx.Guild.SetValue("newJoinerInfoImage", "");

            await ctx.Channel.SendEmbedMessage("🐾  New joiner info message removed successfully", "", Bot.ColorMain);
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }

    [Command("preview_new_joiner_info_message")]
    [RequireRoles(RoleCheckMode.Any, "Swippy")]
    public async Task PreviewNewJoinerInfoMessage(CommandContext ctx)
    {
        try
        {
            string title = ctx.Guild.GetValue("newJoinerInfoTitle");
            string description = ctx.Guild.GetValue("newJoinerInfoDescription");
            string sticker = ctx.Guild.GetValue("newJoinerInfoImage");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
            {
                ctx.Channel.SendTimedEmbedMessage("🐾  No new joiner info message is set", "", Bot.ColorWarning, TimeSpan.FromSeconds(20)).GetAwaiter();
                return;
            }

            await ctx.Channel.SendMessageAsync(Bot.HelloAndGoodbye.NewJoinerInfoMessageEmbed(ctx.Guild));
        }
        catch (System.Exception ex)
        {
            Bot.ReportError(ex);
        }
    }
}