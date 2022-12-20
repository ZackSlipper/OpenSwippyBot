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
    public class HelloAndGoodbye
    {
        Bot bot;
        Random rnd;

        string[] joinMessages = { "Slipped Into The Server 💜", "Popped Up 🍿", "Materialized 💫", "Came Into Existence 🌟", "Is Here! Yay! ^-^ 🎉", "Dropped In 📦", "Rocketed Into The Server 🚀" };

        public HelloAndGoodbye(Bot bot)
        {
            this.bot = bot;
            rnd = new Random();

            bot.Client.GuildMemberAdded += GuildMemberAdded;
            bot.Client.GuildMemberRemoved += GuildMemberRemoved;
        }

        //Join
        async Task GuildMemberAdded(DiscordClient client, GuildMemberAddEventArgs e)
        {
            await JoinNotifyChannel(e);
            await JoinNotifyStaff(e);
            try
            {
                DiscordEmbedBuilder newJoinerInfoMessage = NewJoinerInfoMessageEmbed(e.Guild);
                if (newJoinerInfoMessage != null)
                await e.Member.SendMessageAsync(newJoinerInfoMessage);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task JoinNotifyChannel(GuildMemberAddEventArgs e)
        {
            try
            {
                DiscordChannel channel = e.Guild.GetValueChannel("hello_channel");
                if (channel == null)
                    return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Welcome to");
                sb.AppendLine(e.Guild.Name);
                sb.AppendLine(e.Member.Mention);

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"🐾 {e.Member.Username} {joinMessages[rnd.Next(0, joinMessages.Length)]}",
                    Description = sb.ToString(),
                    Color = Bot.ColorMain,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
                };

                await channel.SendMessageAsync(embedBuilder);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task JoinNotifyStaff(GuildMemberAddEventArgs e)
        {
            List<DiscordMember> members = GetSubscribedMembers(e.Guild);
            if (members.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Member.Mention);
            sb.AppendLine($"ID: {e.Member.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"🌟 A new user joined \"{e.Guild.Name}\" server",
                Description = sb.ToString(),
                Color = Bot.ColorMain,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
            };

            foreach (DiscordMember member in members)
            {
                try
                {
                    await member.SendMessageAsync(embedBuilder);
                }
                catch (System.Exception) { }
            }
        }

        //Leave
        async Task GuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            await LeaveNotifyChannel(e);
        }

        async Task LeaveNotifyChannel(GuildMemberRemoveEventArgs e)
        {
            try
            {
                DiscordChannel channel = e.Guild.GetValueChannel("hello_channel");
                if (channel == null)
                    return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Bye bye ;-;");
                sb.AppendLine(e.Member.Mention);

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"😿 {e.Member.Username} Left",
                    Description = sb.ToString(),
                    Color = Bot.ColorMain,
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
                };
                await channel.SendMessageAsync(embedBuilder);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task LeaveNotifyStaff(GuildMemberRemoveEventArgs e)
        {
            List<DiscordMember> members = GetSubscribedMembers(e.Guild);
            if (members.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Member.Mention);
            sb.AppendLine($"ID: {e.Member.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"🧳 A new user left \"{e.Guild.Name}\" server",
                Description = sb.ToString(),
                Color = Bot.ColorMain,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
            };

            foreach (DiscordMember member in members)
            {
                try
                {
                    await member.SendMessageAsync(embedBuilder);
                }
                catch (System.Exception) { }
            }
        }

        //Subscriptions
        public bool SubscribeMember(DiscordMember member)
        {
            List<DiscordMember> members = GetSubscribedMembers(member.Guild);
            if (members.Any(x => x.Id == member.Id))
                return false;

            members.Add(member);
            StoreSubscribedMembers(members, member.Guild);

            return true;
        }

        public bool UnsubscribeMember(DiscordMember member)
        {
            List<DiscordMember> members = GetSubscribedMembers(member.Guild);
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].Id == member.Id)
                {
                    members.RemoveAt(i);
                    StoreSubscribedMembers(members, member.Guild);
                    return true;
                }
            }
            return false;
        }

        public List<DiscordMember> GetSubscribedMembers(DiscordGuild server)
        {
            string[] memberIDs = server.GetValue("userJoinLeaveSubscribedMembers").Split('|', StringSplitOptions.RemoveEmptyEntries);
            List<DiscordMember> members = new List<DiscordMember>();

            foreach (string memberID in memberIDs)
            {
                DiscordMember member = server.GetMember(memberID.ULong());
                if (member != null && Bot.Staff.IsStaff(member))
                    members.Add(member);
            }

            return members;
        }

        void StoreSubscribedMembers(List<DiscordMember> members, DiscordGuild server)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DiscordMember member in members)
                sb.Append($"{member.Id}|");

            server.SetValue("userJoinLeaveSubscribedMembers", sb);
        }

        //New Joiner Info
        public DiscordEmbedBuilder NewJoinerInfoMessageEmbed(DiscordGuild server)
        {
            string title = server.GetValue("newJoinerInfoTitle");
            string description = server.GetValue("newJoinerInfoDescription");
            string sticker = server.GetValue("newJoinerInfoImage");

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description))
                return null;

            title = title.Replace("[ServerName]", server.Name);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = Bot.ColorMain,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = sticker, Height = 64, Width = 64 }
            };

            return embedBuilder;
        }
    }
}