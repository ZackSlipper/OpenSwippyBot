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
    public class Counter
    {
        Bot bot;
        HashSet<ulong> channels = new HashSet<ulong>();

        Dictionary<ulong, string> specialNumbers = new Dictionary<ulong, string>()
        {
            {69, "https://c.tenor.com/H6sjheSkU1wAAAAC/noice-nice.gif"},
            {420, "https://c.tenor.com/8tWPtR7ia-kAAAAC/snoop-dogg.gif"},
            {1337, "https://c.tenor.com/l5YJN2v0P6gAAAAC/1337-bazhar.gif"},
            {314159, "https://c.tenor.com/42mPaFRXrQYAAAAd/pi-day-314.gif"}
        };

        public Counter(Bot bot)
        {
            this.bot = bot;

            bot.Client.MessageCreated += MessageCreated;
            bot.Client.MessageDeleted += MessageDeleted;
            bot.Client.MessageUpdated += MessageUpdated;
        }

        async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            try
            {
                if (e.Author.IsBot || !channels.Contains(e.Channel.Id))
                    return;

                ulong noCountingRoleID = e.Guild.GetValue("no_counting_role").ULong();
                if ((await e.Guild.GetMemberAsync(e.Author.Id)).Roles.Any(x => x.Id == noCountingRoleID))
                {
                    await e.Message.DeleteAsync();
                    e.Channel.SendTimedEmbedMessage($"⛔  {e.Author.Username} not allowed to count", "", Bot.ColorWarning, TimeSpan.FromSeconds(10)).GetAwaiter();
                    return;
                }

                if (!string.IsNullOrWhiteSpace(e.Message.Content) && e.Message.Content.Substring(0, 1) != Bot.Prefix)
                {
                    DiscordGuild server = e.Guild;
                    ServerValues values = Bot.GetValues(server.Id);

                    if (e.Message.Id == values.Counter.lastMessageID)
                        return;

                    string[] lines = e.Message.Content.Split(Environment.NewLine);
                    if (lines.Length > 10)
                    {
                        await e.Message.DeleteAsync();
                        e.Channel.SendTimedEmbedMessage($"⚠️  Too many lines. Max allowed number of lines is 10", "", Bot.ColorWarning, TimeSpan.FromSeconds(10)).GetAwaiter();
                        return;
                    }

                    List<int> invalidLines = new List<int>();
                    bool counted = false;

                    for (int i = 0; i < lines.Length; i++)
                    {
                        ulong number;
                        if (ulong.TryParse(lines[i], out number))
                        {
                            if (number != values.Counter.currentNumber) //Miscount
                            {
                                StringBuilder sb = new StringBuilder();
                                int lives = server.GetValue("counting_lives").Int() - 1;
                                int maxLives = server.GetValue("counting_max_lives").Int();
                                DiscordColor color = Bot.ColorWarning;
                                ulong oldTargetNum = values.Counter.currentNumber;

                                if (lives <= 0)
                                {
                                    SetCurrentNumber(server, 0);
                                    color = Bot.ColorError;

                                    sb.AppendLine("💔 Out of lives :(");
                                    sb.AppendLine("");
                                    sb.AppendLine($"Restarting count from 0 with {maxLives} lives");
                                }
                                else
                                {
                                    server.SetValue("counting_lives", lives);
                                    SetCurrentNumber(server, server.GetValue("counter_number").ULong());

                                    for (int j = 0; j < lives; j++)
                                        sb.Append("❤️");
                                    sb.Append(Environment.NewLine);

                                    sb.AppendLine($"{lives} of {maxLives} lives remaining");
                                    sb.AppendLine("");
                                    sb.AppendLine($"Next number should be {values.Counter.currentNumber}");
                                }

                                if (lines.Length > 1)
                                    await e.Message.Channel.SendEmbedMessage($"🧮  {e.Author.Username} miscounted at line {i + 1}... The number was supposed to be {oldTargetNum}", sb.ToString(), color);
                                else
                                    await e.Message.Channel.SendEmbedMessage($"🧮  {e.Author.Username} miscounted... The number was supposed to be {oldTargetNum}", sb.ToString(), color);

                                values.Counter.lastMessageID = e.Message.Id;

                                return;
                            }

                            values.Counter.currentNumber++;
                            counted = true;

                            //User counted number count
                            ulong numbersCounted = server.GetUserValue(e.Author.Id, "counter_numbers_counted").ULong() + 1;
                            server.SetUserValue(e.Author.Id, "counter_numbers_counted", numbersCounted);

                            if (specialNumbers.ContainsKey(number))
                            {
                                await e.Channel.SendMessageAsync(new DiscordMessageBuilder()
                                {
                                    Embed = new DiscordEmbedBuilder()
                                    {
                                        Title = number.ToString(),
                                        ImageUrl = specialNumbers[number],
                                        Color = Bot.ColorMain
                                    }
                                });
                            }
                        }
                        else
                            invalidLines.Add(i);
                    }

                    if (counted)
                    {
                        server.SetValue("counter_number", values.Counter.currentNumber);
                        values.Counter.lastMessageID = e.Message.Id;

                        if (invalidLines.Count > 0)
                        {
                            StringBuilder sb = new StringBuilder();
                            foreach (int lineNumber in invalidLines)
                                sb.AppendLine($"{lineNumber + 1}: {lines[lineNumber]}");

                            e.Channel.SendTimedEmbedMessage($"🧮  The following line{(invalidLines.Count > 1 ? "s" : "")} could not be parsed in to numbers:", sb.ToString(), Bot.ColorWarning, TimeSpan.FromSeconds(45)).GetAwaiter();
                        }

                        if (lines.Length > 1)
                            await e.Channel.SendEmbedMessage($"🧮  {e.Author.Username} counted up to {values.Counter.currentNumber - 1}", $"Next number is {values.Counter.currentNumber}", Bot.ColorMain);
                    }
                    else
                        e.Channel.SendTimedEmbedMessage($"🧮  What {e.Author.Username} said is not a number or a set of numbers...", $"Hint: the next number should be {values.Counter.currentNumber}", Bot.ColorWarning, TimeSpan.FromSeconds(10)).GetAwaiter();
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task MessageDeleted(DiscordClient client, MessageDeleteEventArgs e)
        {
            try
            {
                ServerValues values = Bot.GetValues(e.Guild.Id);
                if (values.Counter.lastMessageID == e.Message.Id)
                    await e.Channel.SendEmbedMessage($"{values.Counter.currentNumber - 1}", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            try
            {
                if (e == null || e.Guild == null || e.Message == null)
                    return;

                ServerValues values = Bot.GetValues(e.Guild.Id);
                if (values.Counter.lastMessageID == e.Message.Id)
                {
                    e.Channel.SendTimedEmbedMessage($"🧮  The last number message can not be edited", "", Bot.ColorMain, TimeSpan.FromSeconds(15)).GetAwaiter();
                    await e.Message.DeleteAsync();
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        public void SetCurrentNumber(DiscordGuild server, ulong number)
        {
            ServerValues values = Bot.GetValues(server.Id);
            values.Counter.currentNumber = number;
            server.SetValue("counter_number", values.Counter.currentNumber);

            if (number == 0)
                server.SetValue("counting_lives", server.GetValue("counting_max_lives"));
        }

        public void AddServerData(DiscordGuild server)
        {
            ulong channelID = server.GetValue("counting_channel").ULong();

            if (!channels.Contains(channelID))
                channels.Add(channelID);


            if (server.GetValue("counting_max_lives").Int() == 0)
                server.SetValue("counting_max_lives", "3");

            if (server.GetValue("counting_lives").Int() == 0)
                server.SetValue("counting_lives", "3");
        }

        public void RemoveServerData(DiscordGuild server)
        {
            ulong channelID = server.GetValue("counting_channel").ULong();

            if (channels.Contains(channelID))
                channels.Remove(channelID);
        }
    }
}