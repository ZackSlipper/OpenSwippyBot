using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Emzi0767.Utilities;

namespace SwippyBot
{
    public static class Extensions
    {
        public static string Name(this DiscordUser user)
        {
            try
            {
                DiscordMember member = (DiscordMember)user;
                return string.IsNullOrWhiteSpace(member.Nickname) ? member.Username : member.Nickname;
            }
            catch (System.Exception)
            {
                return user.Username;
            }
        }

        public static DiscordChannel DiscordChannel(this string text, DiscordGuild server)
        {
            try
            {
                return server.GetChannel(text.ULong());
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static DiscordChannel DiscordChannel(this ulong id, DiscordGuild server)
        {
            try
            {
                return server.GetChannel(id);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static DiscordRole DiscordRole(this string text, DiscordGuild server)
        {
            try
            {
                return server.GetRole(text.ULong());
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static DiscordMember GetMember(this DiscordGuild server, ulong userID)
        {
            try
            {
                return server.GetMemberAsync(userID).GetAwaiter().GetResult();
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public static bool Bool(this string text)
        {
            text = text.ToLower();
            if (text == "true" || text == "1")
                return true;
            return false;
        }

        public static ulong ULong(this string text)
        {
            ulong output;
            if (ulong.TryParse(text, out output))
                return output;
            return 0;
        }

        public static long Long(this string text)
        {
            long output;
            if (long.TryParse(text, out output))
                return output;
            return 0;
        }

        public static int Int(this string text)
        {
            int output;
            if (int.TryParse(text, out output))
                return output;
            return 0;
        }

        public static float Float(this string text)
        {
            float output;
            if (float.TryParse(text, out output))
                return output;
            return 0;
        }

        public static float Probability(this string text)
        {
            float output;
            if (float.TryParse(text, out output))
                return output / 100;
            return 0;
        }

        public static double Double(this string text)
        {
            double output;
            if (double.TryParse(text, out output))
                return output;
            return 0;
        }

        public static DiscordChannel GetValueChannel(this DiscordGuild server, string key)
        {
            return Bot.GetSeverData(server.Id).GetServerValue(key).DiscordChannel(server);
        }

        //Server values
        public static string GetValue(this DiscordGuild server, string key)
        {
            ServerData data = Bot.GetSeverData(server.Id);
            if (data == null)
                return "";
            return data.GetServerValue(key);
        }

        public static void SetValue(this DiscordGuild server, string key, object value)
        {
            Bot.GetSeverData(server.Id).SetServerValue(key, value.ToString());
        }

        //User values
        public static string GetUserValue(this DiscordGuild server, ulong userID, string key)
        {
            return Bot.GetSeverData(server.Id).GetUserValue(userID, key);
        }

        public static void SetUserValue(this DiscordGuild server, ulong userID, string key, object value)
        {
            Bot.GetSeverData(server.Id).SetUserValue(userID, key, value.ToString());
        }

        public static string GetValue(this DiscordMember member, string key)
        {
            return member.Guild.GetUserValue(member.Id, key);
        }

        public static void SetValue(this DiscordMember member, string key, object value)
        {
            member.Guild.SetUserValue(member.Id, key, value.ToString());
        }

        //Messages
        public static async Task<DiscordMessage> SendReplyEmbedMessage(this DiscordMessage replyMessage, string title, string description, DiscordColor color)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = color
            };
            return await replyMessage.RespondAsync(embedBuilder);
        }

        public static async Task ReplyTimedMessage(this DiscordMessage replyMessage, TimeSpan duration, DiscordMessageBuilder message)
        {
            DiscordMessage msg = null;
            try
            {
                if (duration.TotalSeconds <= 0)
                {
                    msg = await Error(replyMessage.Channel, new Exception($"Invalid duration of: {duration.TotalSeconds}s"));
                    duration = TimeSpan.FromSeconds(5);

                    await Task.Delay(duration);
                    await replyMessage.Channel.DeleteMessageAsync(msg);
                }

                msg = await replyMessage.RespondAsync(message).ConfigureAwait(false);

                await Task.Delay(duration);
                await replyMessage.Channel.DeleteMessageAsync(msg);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                DiscordChannel logChannel = Bot.AuditLog.GetMessageLogChannel(replyMessage.Channel.Guild.Id);
                if (logChannel != null && msg != null)
                    await SendEmbedMessage(logChannel, "Timed message deleted before time out", $"Message id: {msg.Id}", Bot.ColorWarning);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        public static async Task SendReplyTimedEmbedMessage(this DiscordMessage replyMessage, string title, string description, DiscordColor color, TimeSpan duration)
        {
            DiscordMessageBuilder message = new DiscordMessageBuilder();
            message.AddEmbed(new DiscordEmbedBuilder()
            {
                Title = title,
                Description = description,
                Color = color
            });
            await replyMessage.ReplyTimedMessage(duration, message);
        }

        public static async Task<DiscordMessage> SendEmbedMessage(this DiscordChannel channel, string title, string description, DiscordColor color)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = color
            };

            return await channel.SendMessageAsync(embedBuilder);
        }

        public static async Task<DiscordMessage> SendEmbedDM(this DiscordMember member, string title, string description, DiscordColor color)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = title,
                Description = description,
                Color = color
            };

            return await member.SendMessageAsync(embedBuilder);
        }

        public static async Task<DiscordMessage> Error(this DiscordChannel channel, Exception ex)
        {
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = $"Command Error: {ex.Message}",
                Description = ex.StackTrace,
                Color = Bot.ColorError
            };
            return await channel.SendMessageAsync(embedBuilder);
        }

        public static async Task TimedMessage(this DiscordChannel channel, TimeSpan duration, DiscordMessageBuilder message)
        {
            DiscordMessage msg = null;
            try
            {
                if (duration.TotalSeconds <= 0)
                {
                    msg = await Error(channel, new Exception($"Invalid duration of: {duration.TotalSeconds}s"));
                    duration = TimeSpan.FromSeconds(5);

                    await Task.Delay(duration);
                    await channel.DeleteMessageAsync(msg);
                }

                msg = await channel.SendMessageAsync(message).ConfigureAwait(false);

                await Task.Delay(duration);
                await channel.DeleteMessageAsync(msg);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                DiscordChannel logChannel = Bot.AuditLog.GetMessageLogChannel(channel.Guild.Id);
                if (logChannel != null && msg != null)
                    await SendEmbedMessage(logChannel, "Timed message deleted before time out", $"Message id: {msg.Id}", Bot.ColorWarning);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        public static async Task SendTimedEmbedMessage(this DiscordChannel channel, string title, string description, DiscordColor color, TimeSpan duration)
        {
            DiscordMessageBuilder message = new DiscordMessageBuilder();
            message.AddEmbed(new DiscordEmbedBuilder()
            {
                Title = title,
                Description = description,
                Color = color
            });
            await channel.TimedMessage(duration, message);
        }

        public static async Task MessageWithButtons(this CommandContext ctx, DiscordEmbedBuilder embed, List<DiscordButtonComponent> buttons, TimeSpan lifetime, AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> action)
        {
            try
            {
                DiscordMessageBuilder msgBuilder = new DiscordMessageBuilder { Embed = embed };
                msgBuilder.AddComponents(buttons.ToArray());

                DiscordMessage msg = await ctx.Channel.SendMessageAsync(msgBuilder);

                buttons.ForEach(x => x.Disable());

                AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs> interaction = null;
                interaction = new AsyncEventHandler<DiscordClient, ComponentInteractionCreateEventArgs>(async (client, e) =>
                {
                    if (msg.Id != e.Message.Id)
                        return;

                    await action(client, e);
                });

                ctx.Client.ComponentInteractionCreated += interaction;

                await ctx.Client.GetInteractivity().WaitForButtonAsync(msg, buttons, lifetime);

                ctx.Client.ComponentInteractionCreated -= interaction;
                await msg.ModifyAsync(msgBuilder);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        public static async Task TimedDelete(this DiscordMessage message, TimeSpan duration)
        {
            try
            {
                if (duration.TotalSeconds <= 0)
                {
                    await message.DeleteAsync();
                    return;
                }

                await Task.Delay(duration);
                await message.DeleteAsync();
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                DiscordChannel logChannel = Bot.AuditLog.GetMessageLogChannel(message.Channel.Guild.Id);
                if (logChannel != null && message != null)
                    await SendEmbedMessage(logChannel, "Timed message deleted before time out", $"Message id: {message.Id}", Bot.ColorWarning);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Returns false/no if times out or errors out
        public static async Task<bool> SendYesNoResponseRequest(this DiscordChannel channel, DiscordUser user, string title, string descriptionYes, string descriptionNo, DiscordColor color)
        {
            try
            {
                DiscordEmoji[] reactions = new DiscordEmoji[] { DiscordEmoji.FromUnicode("✅"), DiscordEmoji.FromUnicode("❎") };
                DiscordMessage responseMessage = await channel.SendEmbedMessage(title, $"✅ - {descriptionYes}{Environment.NewLine}❎ - {descriptionNo}", color);

                foreach (DiscordEmoji reaction in reactions)
                    await responseMessage.CreateReactionAsync(reaction);

                InteractivityResult<MessageReactionAddEventArgs> result = await Bot.Interactivity.WaitForReactionAsync((x) => reactions.Any((z) => z == x.Emoji), responseMessage, user);
                await responseMessage.DeleteAsync();

                if (result.TimedOut)
                    return false;

                return result.Result.Emoji == reactions[0];
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
                return false;
            }
        }
    }
}