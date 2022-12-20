using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
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
    public class UserCommands : BaseCommandModule
    {
        [Command("stats")]
        public async Task Stats(CommandContext ctx, DiscordMember user)
        {
            try
            {
                await ctx.Channel.DeleteMessageAsync(ctx.Message);

                if (user == null)
                    return;

                ulong numbersCounted = ctx.Guild.GetUserValue(user.Id, "counter_numbers_counted").ULong();
                int warningCount = Bot.WarningSystem.GetUserWarnings(user).Count;
                int count = ctx.Guild.GetValue("warningCountToTimeout").Int();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(user.Mention);
                sb.AppendLine("");
                sb.AppendLine($"**Numbers counted:** {numbersCounted}");
                sb.AppendLine($"**Warnings:** {warningCount}/{count}");

                sb.AppendLine("");
                sb.AppendLine($"**Boops:**");
                sb.AppendLine($"　• **Total given:** {ctx.Guild.GetUserValue(user.Id, "boops_given").ULong()}");
                sb.AppendLine($"　• **Total received:** {ctx.Guild.GetUserValue(user.Id, "boops_received").ULong()}");

                //User received and given boops
                string boopsGivenKey = "boops_given_";
                string boopsReceivedKey = "boops_received_";

                List<Tuple<DiscordMember, ulong>> boopsGiven = new List<Tuple<DiscordMember, ulong>>();
                List<Tuple<DiscordMember, ulong>> boopsReceived = new List<Tuple<DiscordMember, ulong>>();

                //Get boop given/received data
                foreach (string key in Bot.GetSeverData(ctx.Guild.Id).GetUserKeys(user.Id))
                {
                    if (key.StartsWith(boopsGivenKey))
                    {
                        DiscordMember boopUser = null;
                        try
                        {
                            boopUser = await ctx.Guild.GetMemberAsync(key.Remove(0, boopsGivenKey.Length).ULong());
                        }
                        catch (System.Exception) { }

                        boopsGiven.Add(new Tuple<DiscordMember, ulong>(boopUser, ctx.Guild.GetUserValue(user.Id, key).ULong()));
                    }
                    else if (key.StartsWith(boopsReceivedKey))
                    {
                        DiscordMember boopUser = null;
                        try
                        {
                            boopUser = await ctx.Guild.GetMemberAsync(key.Remove(0, boopsReceivedKey.Length).ULong());
                        }
                        catch (System.Exception) { }

                        boopsReceived.Add(new Tuple<DiscordMember, ulong>(boopUser, ctx.Guild.GetUserValue(user.Id, key).ULong()));
                    }
                }

                //Sort the data and print it out
                Tuple<DiscordMember, ulong> temp;
                if (boopsGiven.Count > 0)
                {
                    for (int i = 0; i < boopsGiven.Count; i++)
                    {
                        for (int j = 1; j < boopsGiven.Count; j++)
                        {
                            if (boopsGiven[j - 1].Item2 < boopsGiven[j].Item2)
                            {
                                temp = boopsGiven[j - 1];
                                boopsGiven[j - 1] = boopsGiven[j];
                                boopsGiven[j] = temp;
                            }
                        }
                    }

                    sb.AppendLine($"");
                    sb.AppendLine($"　• **Given to: **");
                    foreach (Tuple<DiscordMember, ulong> item in boopsGiven)
                        if (item.Item1 != null)
                            sb.AppendLine($"　　- {item.Item1.Mention}　{item.Item2}");
                }

                if (boopsReceived.Count > 0)
                {
                    for (int i = 0; i < boopsReceived.Count; i++)
                    {
                        for (int j = 1; j < boopsReceived.Count; j++)
                        {
                            if (boopsReceived[j - 1].Item2 < boopsReceived[j].Item2)
                            {
                                temp = boopsReceived[j - 1];
                                boopsReceived[j - 1] = boopsReceived[j];
                                boopsReceived[j] = temp;
                            }
                        }
                    }

                    sb.AppendLine($"");
                    sb.AppendLine($"　• **Received from: **");
                    foreach (Tuple<DiscordMember, ulong> item in boopsReceived)
                        if (item.Item1 != null)
                            sb.AppendLine($"　　- {item.Item1.Mention}　{item.Item2}");



                    sb.AppendLine($"");
                    sb.AppendLine($"**Hugs:**");
                    sb.AppendLine($"　• **Total given:** {ctx.Guild.GetUserValue(user.Id, "hugs_given").ULong()}");
                    sb.AppendLine($"　• **Total received:** {ctx.Guild.GetUserValue(user.Id, "hugs_received").ULong()}");

                    //User received and given boops
                    string hugsGivenKey = "hugs_given_";
                    string hugsReceivedKey = "hugs_received_";

                    List<Tuple<DiscordMember, ulong>> hugsGiven = new List<Tuple<DiscordMember, ulong>>();
                    List<Tuple<DiscordMember, ulong>> hugsReceived = new List<Tuple<DiscordMember, ulong>>();

                    //Get hug given/received data
                    foreach (string key in Bot.GetSeverData(ctx.Guild.Id).GetUserKeys(user.Id))
                    {
                        if (key.StartsWith(hugsGivenKey))
                        {
                            DiscordMember hugUser = null;
                            try
                            {
                                hugUser = await ctx.Guild.GetMemberAsync(key.Remove(0, hugsGivenKey.Length).ULong());
                            }
                            catch (System.Exception) { }

                            hugsGiven.Add(new Tuple<DiscordMember, ulong>(hugUser, ctx.Guild.GetUserValue(user.Id, key).ULong()));
                        }
                        else if (key.StartsWith(hugsReceivedKey))
                        {
                            DiscordMember boopUser = null;
                            try
                            {
                                boopUser = await ctx.Guild.GetMemberAsync(key.Remove(0, hugsReceivedKey.Length).ULong());
                            }
                            catch (System.Exception) { }

                            hugsReceived.Add(new Tuple<DiscordMember, ulong>(boopUser, ctx.Guild.GetUserValue(user.Id, key).ULong()));
                        }
                    }

                    //Sort the data and print it out
                    if (hugsGiven.Count > 0)
                    {
                        for (int i = 0; i < hugsGiven.Count; i++)
                        {
                            for (int j = 1; j < hugsGiven.Count; j++)
                            {
                                if (hugsGiven[j - 1].Item2 < hugsGiven[j].Item2)
                                {
                                    temp = hugsGiven[j - 1];
                                    hugsGiven[j - 1] = hugsGiven[j];
                                    hugsGiven[j] = temp;
                                }
                            }
                        }

                        sb.AppendLine($"");
                        sb.AppendLine($"　• **Given to: **");
                        foreach (Tuple<DiscordMember, ulong> item in hugsGiven)
                            if (item.Item1 != null)
                                sb.AppendLine($"　　- {item.Item1.Mention}　{item.Item2}");
                    }

                    if (hugsReceived.Count > 0)
                    {
                        for (int i = 0; i < hugsReceived.Count; i++)
                        {
                            for (int j = 1; j < hugsReceived.Count; j++)
                            {
                                if (hugsReceived[j - 1].Item2 < hugsReceived[j].Item2)
                                {
                                    temp = hugsReceived[j - 1];
                                    hugsReceived[j - 1] = hugsReceived[j];
                                    hugsReceived[j] = temp;
                                }
                            }
                        }

                        sb.AppendLine($"");
                        sb.AppendLine($"　• **Received from: **");
                        foreach (Tuple<DiscordMember, ulong> item in hugsReceived)
                            if (item.Item1 != null)
                                sb.AppendLine($"　　- {item.Item1.Mention}　{item.Item2}");
                    }
                    ctx.Channel.SendTimedEmbedMessage($"📏  {user.Username}' stats:", sb.ToString(), Bot.ColorMain, TimeSpan.FromMinutes(2)).GetAwaiter();
                }
                else
                    await ctx.Channel.SendEmbedMessage($"Invalid user", "", Bot.ColorError);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("stats")]
        [Description("Get user statistics")]
        public async Task Stats(CommandContext ctx)
        {
            await Stats(ctx, ctx.Member);
        }
    }
}