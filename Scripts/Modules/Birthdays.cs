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
using System.Threading;

namespace SwippyBot
{
    public class Birthdays
    {
        List<Birthday> birthdays = new List<Birthday>();

        //Key: serverID  Value: ChannelRolePair
        Dictionary<DiscordGuild, ChannelRolePair> channelRolePairs = new Dictionary<DiscordGuild, ChannelRolePair>();

        CancellationTokenSource runTaskCancellationTokenSource;

        Bot bot;

        public Birthdays(Bot bot)
        {
            this.bot = bot;

            LoadBirthdays();

            Run().GetAwaiter();
        }

        async Task Run()
        {
            try
            {
                while (true)
                {
                    using (runTaskCancellationTokenSource = new CancellationTokenSource())
                    {
                        try
                        {
                            foreach (Birthday birthday in birthdays)
                            {
                                if (birthday.IsNow) //If the birthday is happening now and isn't active make sure that the user's birthday was announced and they have the birthday role
                                    await StartBirthday(birthday);
                                else //If the birthday isn't happening now and is active make sure that the user's birthday birthday role is revoked
                                    await EndBirthday(birthday);
                            }

                            await Task.Delay(TimeSpan.FromMinutes(15), runTaskCancellationTokenSource.Token);
                        }
                        catch (TaskCanceledException) { }
                    }
                    runTaskCancellationTokenSource = null;
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
                return;
            }
        }

        async Task StartBirthday(Birthday birthday)
        {
            if (birthday.Active)
                return;

            //Grant birthday role in all the servers the user is in
            foreach (DiscordGuild server in channelRolePairs.Keys.ToArray())
            {
                UpdateChannelRolePair(server);
                if (!channelRolePairs.ContainsKey(server))
                    continue;

                ChannelRolePair pair = channelRolePairs[server];
                DiscordMember member = server.GetMember(birthday.UserID);
                if (member == null)
                    continue;

                try
                {
                    if (!member.Roles.Contains(pair.Role))
                    {
                        await member.GrantRoleAsync(pair.Role);
                        await pair.Channel.SendEmbedMessage($"🎂 Happy birthday {member.Username}!!!!! ^-^ 🎂", $"🎉  {member.Mention}  🎉{Environment.NewLine}Born {birthday.Date.ToString()}{Environment.NewLine}🍰  Just turned {(DateTime.UtcNow.Year - birthday.Date.Year)} years old! Yays! >w<  🍰", Bot.ColorMain);
                    }
                }
                catch (System.Exception) { }
            }
            birthday.Active = true;
        }

        async Task EndBirthday(Birthday birthday)
        {
            if (!birthday.Active)
                return;

            //Revoke birthday role in all the servers the user is in
            foreach (DiscordGuild server in channelRolePairs.Keys.ToArray())
            {
                UpdateChannelRolePair(server);
                if (!channelRolePairs.ContainsKey(server))
                    continue;

                ChannelRolePair pair = channelRolePairs[server];
                DiscordMember member = server.GetMember(birthday.UserID);
                if (member == null)
                    continue;

                try
                {
                    await member.RevokeRoleAsync(pair.Role);
                }
                catch (System.Exception) { }
            }
            birthday.Active = false;
        }

        public bool AddBirthday(Birthday birthday)
        {
            //If the birthday already exists it shouldn't be added to the birthday list again
            foreach (Birthday existingBirthday in birthdays)
                if (existingBirthday == birthday)
                    return false;

            //Add the birthday to the birthday list and store it in the global configuration file
            birthdays.Add(birthday);
            StoreBirthdays();

            //Start the birthday if its happening now
            if (birthday.IsNow)
                StartBirthday(birthday).GetAwaiter().GetResult();

            return true;
        }

        public bool RemoveBirthday(ulong userID)
        {
            //Find the users birthday using their user ID
            for (int i = 0; i < birthdays.Count; i++)
            {
                if (birthdays[i].UserID == userID)
                {
                    Birthday birthday = birthdays[i];
                    birthdays.RemoveAt(i);

                    //If the birthday is happening right now it gets ended
                    if (birthday.Active)
                        EndBirthday(birthday).GetAwaiter().GetResult();

                    //The full birthday list gets written into the global data
                    StoreBirthdays();
                    return true;
                }
            }
            return false;
        }

        public Birthday NextBirthday(DiscordGuild server)
        {
            Birthday[] birthdayArray = SortedBirthdayArray(server);
            if (birthdayArray.Length == 0)
                return null;

            for (int i = 0; i < birthdayArray.Length; i++)
            {
                if (birthdayArray[i].StartDate > DateTimeOffset.UtcNow)
                    return birthdayArray[i];
            }
            return birthdayArray[0];
        }

        public Birthday[] BirthdayArray(DiscordGuild server)
        {
            return birthdays.Where(x => server.GetMember(x.UserID) != null).ToArray();
        }

        /// <summary>
        /// Gets and sorts an array of birthdays of given server's members
        /// </summary>
        /// <param name="server">Server whose member's birthdays will be listed</param>
        /// <returns>Sorted array of all the birthdays from earliest to latest</returns>
        public Birthday[] SortedBirthdayArray(DiscordGuild server)
        {
            Birthday[] birthdayArray = BirthdayArray(server);
            Birthday temp;

            //If the birthday array is longer then 1 it gets sorted in ascending order by the birthday's start date
            if (birthdayArray.Length > 1)
            {
                for (int i = 0; i < birthdayArray.Length; i++)
                {
                    for (int j = 0; j < birthdayArray.Length - 1; j++)
                    {
                        if (birthdayArray[j].StartDate > birthdayArray[j + 1].StartDate)
                        {
                            temp = birthdayArray[j];
                            birthdayArray[j] = birthdayArray[j + 1];
                            birthdayArray[j + 1] = temp;
                        }
                    }
                }
            }
            return birthdayArray;
        }

        public void StoreBirthdays()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Birthday birthday in birthdays)
                sb.Append($"{birthday.Date.Year}:{birthday.Date.Month}:{birthday.Date.Day}:{birthday.Date.TimeZone}:{birthday.UserID}|");

            Bot.GlobalData.SetValue("birthdays", sb.ToString());
        }

        public void LoadBirthdays()
        {
            birthdays.Clear();

            string[] birthdaysText = Bot.GlobalData.GetValue("birthdays").Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (string text in birthdaysText)
            {
                string[] birthdayEntryText = text.Split(":");
                if (birthdayEntryText.Length != 5)
                    continue;


                Birthday birthday = new Birthday(birthdayEntryText[4].ULong(), new BirthdayDate()
                {
                    Year = birthdayEntryText[0].Int(),
                    Month = birthdayEntryText[1].Int(),
                    Day = birthdayEntryText[2].Int(),
                    TimeZone = birthdayEntryText[3].Int()
                });

                birthdays.Add(birthday);
            }
        }

        public Birthday GetUserBirthday(ulong userID)
        {
            Birthday birthday = birthdays.Where(x => x.UserID == userID).FirstOrDefault();
            if (birthday == null)
                return null;
            return birthday;
        }

        public void UpdateChannelRolePair(DiscordGuild server)
        {
            channelRolePairs.Remove(server);

            DiscordChannel channel = server.GetValue("birthday_channel").DiscordChannel(server);
            DiscordRole role = server.GetValue("birthday_role").DiscordRole(server);

            if (channel != null && role != null && !channelRolePairs.ContainsKey(server))
                channelRolePairs.Add(server, new ChannelRolePair(channel, role));
        }

        //Update Birthday Role method
        public async Task<bool> SetBirthdayRole(DiscordGuild server, DiscordRole role)
        {
            //Sets the birthday role if none was set before
            if (!channelRolePairs.ContainsKey(server))
            {
                if (role == null)
                    return false;

                server.SetValue("birthday_role", role.Id);
                UpdateChannelRolePair(server);
                return true;
            }

            //Replace or remove an existing role
            DiscordRole oldRole = channelRolePairs[server].Role;
            server.SetValue("birthday_role", role == null ? "" : role.Id);
            UpdateChannelRolePair(server);

            //Removes the old birthday role from all active birthday users and if necessary grants them the new one
            foreach (Birthday birthday in birthdays)
            {
                if (!birthday.IsNow)
                    continue;

                DiscordMember member = server.GetMember(birthday.UserID);
                if (member == null)
                    continue;

                try
                {
                    await member.RevokeRoleAsync(oldRole);
                    if (role != null)
                        await member.GrantRoleAsync(role);
                }
                catch (System.Exception) { }
            }
            return true;
        }
    }

    public class ChannelRolePair
    {
        public DiscordChannel Channel { get; }
        public DiscordRole Role { get; }

        public ChannelRolePair(DiscordChannel channel, DiscordRole role)
        {
            Channel = channel;
            Role = role;
        }
    }

    public class Birthday
    {
        public BirthdayDate Date { get; }
        public ulong UserID { get; }
        public bool Active { get; set; } = false;

        public DateTimeOffset StartDate
        {
            get
            {
                //If the birthday has already happened this year the start date is returned for the following year
                if (DateTimeOffset.UtcNow >= EndDate)
                    return new DateTimeOffset(DateTimeOffset.UtcNow.Year + 1, Date.Month, Date.Day, 0, 0, 0, TimeSpan.Zero).AddHours(-Date.TimeZone);
                else
                    return new DateTimeOffset(DateTimeOffset.UtcNow.Year, Date.Month, Date.Day, 0, 0, 0, TimeSpan.Zero).AddHours(-Date.TimeZone);
            }
        }

        public string StartDateText
        {
            get => $"{StartDate.ToString("dd-MM-yyyy HH:mm")} UTC";
        }

        public DateTimeOffset EndDate
        {
            get => new DateTimeOffset(DateTimeOffset.UtcNow.Year, Date.Month, Date.Day, 0, 0, 0, TimeSpan.Zero).AddHours(-Date.TimeZone + 24);
        }

        public bool IsNow
        {
            get => DateTimeOffset.UtcNow >= StartDate && DateTimeOffset.UtcNow < EndDate;
        }

        public Birthday(ulong userID, BirthdayDate date)
        {
            UserID = userID;
            Date = date;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Birthday birthday = (Birthday)obj;
            return Date == birthday.Date && UserID == birthday.UserID && Active == birthday.Active;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Birthday b1, Birthday b2)
        {
            if (((object)b1) == null)
                return ((object)b2) == null;
            return b1.Equals(b2);
        }

        public static bool operator !=(Birthday b1, Birthday b2)
        {
            if (((object)b1) == null)
                return ((object)b2) != null;
            return !b1.Equals(b2);
        }
    }

    public class BirthdayDate
    {
        int year = 0; //4
        public int Year
        {
            get => year;
            set
            {
                if (year < 0)
                    year = 0;
                else if (day > 9999)
                    day = 9999;
                year = value;
            }
        }

        int month = 1; //2
        public int Month
        {
            get => month;
            set
            {
                if (month < 1)
                    month = 1;
                else if (month > 12)
                    month = 12;
                month = value;
            }
        }

        int day = 1; //2
        public int Day
        {
            get => day;
            set
            {
                if (day < 1)
                    day = 1;
                else if (day > 31)
                    day = 31;
                day = value;
            }
        }

        int timeZone = 0;
        public int TimeZone
        {
            get => timeZone;
            set
            {
                if (timeZone < -24)
                    timeZone = -24;
                else if (timeZone > 24)
                    timeZone = 24;
                timeZone = value;
            }
        }

        public override int GetHashCode()
        {
            return timeZone * 100000000 + day * 1000000 + month * 10000 + year;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(BirthdayDate))
                return false;
            return ((BirthdayDate)obj).GetHashCode() == GetHashCode();
        }

        public override string ToString()
        {
            return $"{Day}-{Month}-{Year} (Time Zone: {(TimeZone < 0 ? "" : "+")}{TimeZone})";
        }
    }
}