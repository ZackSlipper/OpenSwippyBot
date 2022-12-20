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
    public class BirthdayCommands : BaseCommandModule
    {
        [Command("birthdays_set_role")]
        [Description("Sets the role give to users that are celebrating their birthday")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetRole(CommandContext ctx, DiscordRole role)
        {
            DiscordRole currentRole = ctx.Guild.GetValue("birthday_role").DiscordRole(ctx.Guild);
            if (currentRole != null && currentRole.Id == role.Id)
                await ctx.Channel.SendEmbedMessage("🎂  Birthday role already set to:", role.Mention, Bot.ColorSystem);
            else
            {
                await Bot.Birthdays.SetBirthdayRole(ctx.Guild, role);
                await ctx.Channel.SendEmbedMessage("🎂  Birthday role set to:", role.Mention, Bot.ColorSystem);
            }
        }

        [Command("birthdays_unset_role")]
        [Description("Unsets the role give to users that are celebrating their birthday")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetRole(CommandContext ctx)
        {
            DiscordRole currentRole = ctx.Guild.GetValue("birthday_role").DiscordRole(ctx.Guild);
            if (currentRole == null)
                await ctx.Channel.SendEmbedMessage("🎂  Birthday role unset already", "", Bot.ColorSystem);
            else
            {
                await Bot.Birthdays.SetBirthdayRole(ctx.Guild, null);
                await ctx.Channel.SendEmbedMessage("🎂  Birthday role unset", "", Bot.ColorSystem);
            }
        }

        [Command("birthdays_get_role")]
        [Description("Gets the role give to users that are celebrating their birthday")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetRole(CommandContext ctx)
        {
            DiscordRole currentRole = ctx.Guild.GetValue("birthday_role").DiscordRole(ctx.Guild);
            if (currentRole == null)
                await ctx.Channel.SendEmbedMessage("🎂  No birthday role set", "", Bot.ColorSystem);
            else
                await ctx.Channel.SendEmbedMessage("🎂  Birthday role:", currentRole.Mention, Bot.ColorSystem);
        }

        [Command("birthdays_set_channel")]
        [Description("Sets the channel where users birthdays are announced")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetChannel(CommandContext ctx, DiscordChannel channel)
        {
            DiscordChannel currentChannel = ctx.Guild.GetValue("birthday_channel").DiscordChannel(ctx.Guild);
            if (currentChannel != null && currentChannel.Id == channel.Id)
                await ctx.Channel.SendEmbedMessage("🎂  Birthday channel already set to:", channel.Mention, Bot.ColorSystem);
            else
            {
                ctx.Guild.SetValue("birthday_channel", channel.Id);
                Bot.Birthdays.UpdateChannelRolePair(ctx.Guild);
                await ctx.Channel.SendEmbedMessage("🎂  Birthday channel set to:", channel.Mention, Bot.ColorSystem);
            }
        }

        [Command("birthdays_unset_channel")]
        [Description("Unsets the channel where users birthdays are announced")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetChannel(CommandContext ctx)
        {
            DiscordChannel currentChannel = ctx.Guild.GetValue("birthday_channel").DiscordChannel(ctx.Guild);
            if (currentChannel == null)
                await ctx.Channel.SendEmbedMessage("🎂  Birthday channel unset already", "", Bot.ColorSystem);
            else
            {
                ctx.Guild.SetValue("birthday_channel", "");
                Bot.Birthdays.UpdateChannelRolePair(ctx.Guild);
                await ctx.Channel.SendEmbedMessage("🎂  Birthday channel unset", "", Bot.ColorSystem);
            }
        }

        [Command("birthdays_get_channel")]
        [Description("Gets the channel where users birthdays are announced")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetChannel(CommandContext ctx)
        {
            DiscordChannel currentChannel = ctx.Guild.GetValue("birthday_channel").DiscordChannel(ctx.Guild);
            if (currentChannel == null)
                await ctx.Channel.SendEmbedMessage("🎂  No birthday channel set", "", Bot.ColorSystem);
            else
                await ctx.Channel.SendEmbedMessage("🎂  Birthday channel:", currentChannel.Mention, Bot.ColorSystem);
        }

        [Command("birthday"), Aliases("get_birthday", "birthday_date", "bday")]
        [Description("Get a given user's birthday date")]
        public async Task BirthdayDate(CommandContext ctx, DiscordUser user)
        {
            Birthday birthday = Bot.Birthdays.GetUserBirthday(user.Id);
            if (birthday == null)
                await ctx.Channel.SendEmbedMessage("🎂  Birthday not set for:", user.Mention, Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"🎂  {user.Username}'s birthday:", $"**Born:** {birthday.Date}{Environment.NewLine}**Current or upcoming birthday date:** {birthday.StartDateText}", Bot.ColorMain);
        }

        [Command("set_birthday")]
        [Description("Set own birthday")]
        public async Task SetBirthday(CommandContext ctx, int day, int month, int year, int timeZone)
        {
            try
            {
                Birthday birthday = Bot.Birthdays.GetUserBirthday(ctx.User.Id);
                if (birthday != null)
                    await ctx.Channel.SendEmbedMessage("🎂  Your birthday's already set to: ", birthday.Date.ToString(), Bot.ColorMain);
                else
                {
                    if (day < 1 || day > 31)
                    {
                        await ctx.Channel.SendEmbedMessage($"🎂  Invalid day value: {day.ToString()}", "", Bot.ColorError);
                        return;
                    }

                    if (month < 1 || month > 12)
                    {
                        await ctx.Channel.SendEmbedMessage($"🎂  Invalid month value: {month.ToString()}", "", Bot.ColorError);
                        return;
                    }

                    if (year < 1900 || year > DateTime.UtcNow.Year)
                    {
                        await ctx.Channel.SendEmbedMessage($"🎂  Invalid year value: {year.ToString()}", $"Year format: YYYY{Environment.NewLine}Eg. 2015", Bot.ColorError);
                        return;
                    }

                    if (timeZone < -16 || timeZone > 16)
                    {
                        await ctx.Channel.SendEmbedMessage($"🎂  Invalid time zone value: {timeZone.ToString()}", "Time zone range: -16 to 16", Bot.ColorError);
                        return;
                    }

                    birthday = new Birthday(ctx.User.Id, new BirthdayDate()
                    {
                        Year = year,
                        Month = month,
                        Day = day,
                        TimeZone = timeZone,
                    });

                    if (Bot.Birthdays.AddBirthday(birthday))
                        await ctx.Channel.SendEmbedMessage($"🎂  {ctx.User.Username}'s birthday set to:", birthday.Date.ToString(), Bot.ColorMain);
                    else
                        await ctx.Channel.SendEmbedMessage($"🎂  Failed to set {ctx.User.Username}'s birthday", "", Bot.ColorError);
                }

            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("set_birthday")]
        [Description("Set own birthday")]
        public async Task SetBirthday(CommandContext ctx, int day, int month, int year, string timeZoneAbbreviation)
        {
            int? timeZone = AbbreviationTimeZones.Get(timeZoneAbbreviation);
            if (timeZone.HasValue)
                await SetBirthday(ctx, day, month, year, timeZone.Value);
            else
                await ctx.Channel.SendEmbedMessage($"🎂  Invalid time zone abbreviation: {timeZoneAbbreviation}", "", Bot.ColorError);
        }

        [Command("set_birthday")]
        [Description("Throws error that no time zone was provided when setting own birthday")]
        public async Task SetBirthday(CommandContext ctx, int day, int month, int year)
        {
            await ctx.Channel.SendEmbedMessage($"🎂  No time zone provided", $"Command format: .set_birthday [Day] [Month] [Year] [TimeZone]{Environment.NewLine}Time zone can be a number between -16 and 16 or a time zone abbreviation", Bot.ColorError);
        }

        [Command("remove_birthday")]
        [Description("Remove own birthday")]
        public async Task RemoveBirthday(CommandContext ctx)
        {
            if (Bot.Birthdays.RemoveBirthday(ctx.User.Id))
                await ctx.Channel.SendEmbedMessage($"🎂  {ctx.User.Username}'s birthday removed", "", Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"🎂  {ctx.User.Username}'s birthday already isn't set", "", Bot.ColorMain);
        }

        [Command("next_birthday")]
        [Description("Get the next birthday that will happen")]
        public async Task NextBirthday(CommandContext ctx)
        {
            Birthday birthday = Bot.Birthdays.NextBirthday(ctx.Guild);
            if (birthday == null)
                await ctx.Channel.SendEmbedMessage($"🎂  No future birthdays yet", "", Bot.ColorMain);
            else if (!birthday.Active)
                await ctx.Channel.SendEmbedMessage($"🎂  Next birthday: ", $"{ctx.Guild.GetMember(birthday.UserID).Mention}{Environment.NewLine}**Born:** {birthday.Date}{Environment.NewLine}**Next birthday date:** {birthday.StartDateText}", Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"🎂  Next birthday: ", $"{ctx.Guild.GetMember(birthday.UserID).Mention}{Environment.NewLine}**Born:** {birthday.Date}{Environment.NewLine}**Next birthday date:** Same time next year :D", Bot.ColorMain);
        }

        [Command("list_birthdays")]
        [Description("Get a list of all birthdays on the current server")]
        public async Task ListBirthdays(CommandContext ctx)
        {
            Birthday[] birthdays = Bot.Birthdays.SortedBirthdayArray(ctx.Guild);
            if (birthdays.Length == 0)
                await ctx.Channel.SendEmbedMessage($"🎂  No birthdays yet", "", Bot.ColorMain);
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (Birthday birthday in birthdays)
                    sb.AppendLine($"• {ctx.Guild.GetMember(birthday.UserID).Mention}: {birthday.StartDateText}");
                await ctx.Channel.SendEmbedMessage($"🎂  Birthdays:", sb.ToString(), Bot.ColorMain);
            }
        }
    }
}