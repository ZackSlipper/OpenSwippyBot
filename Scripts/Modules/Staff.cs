using System.Net.Http.Headers;
using System.Diagnostics;
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
    public class Staff
    {
        Bot bot;

        public Staff(Bot bot)
        {
            this.bot = bot;
        }

        public bool IsStaff(DiscordMember member)
        {
            List<StaffRole> roles = GetStaffRoles(member.Guild);
            return member.IsOwner || member.Roles.Any(x => x.Permissions.HasFlag(Permissions.Administrator)) || member.Roles.Any(x => roles.Any(y => x.Id == y.Role.Id));
        }

        //Expensive function
        public async Task<List<StaffMember>> GetStaffMembers(DiscordGuild server)
        {
            List<StaffMember> members = new List<StaffMember>();
            List<StaffRole> roles = GetStaffRoles(server);

            var serverMembers = await server.GetAllMembersAsync();
            foreach (DiscordMember member in serverMembers)
            {
                foreach (StaffRole role in roles)
                {
                    if (member.Roles.Any(x => x.Id == role.Role.Id))
                    {
                        members.Add(new StaffMember(role, member));
                    }
                }
            }

            return members;
        }

        public bool SetStaffRole(DiscordGuild server, DiscordRole role, string tag)
        {
            if (tag.Contains('|') || tag.Contains(':') || string.IsNullOrWhiteSpace(tag))
                return false;

            List<StaffRole> roles = GetStaffRoles(server);
            if (roles.Any(x => x.Role.Id == role.Id))
                return false;

            roles.Add(new StaffRole(role, tag));
            StoreStaffRoles(roles, server);

            return true;
        }

        public bool UnsetStaffRole(DiscordGuild server, DiscordRole role)
        {
            List<StaffRole> roles = GetStaffRoles(server);
            for (int i = roles.Count-1; i >= 0; i--)
            {
                if (roles[i].Role.Id == role.Id)
                {
                    roles.RemoveAt(i);
                    StoreStaffRoles(roles, server);
                    return true;
                }
            }
            return false;
        }

        public List<StaffRole> GetStaffRoles(DiscordGuild server)
        {
            List<StaffRole> roles = new List<StaffRole>();

            string[] roleStrings = server.GetValue("staffRoles").Split('|', StringSplitOptions.RemoveEmptyEntries);
            foreach (string roleString in roleStrings)
            {
                string[] roleDataStrings = roleString.Split(':', StringSplitOptions.RemoveEmptyEntries);
                if (roleDataStrings.Length < 1 || roleDataStrings.Length > 2)
                    continue;

                DiscordRole role = roleDataStrings[0].DiscordRole(server);
                if (role == null)
                    continue;

                string tag = "";
                if (roleDataStrings.Length == 2)
                    tag = roleDataStrings[1];

                roles.Add(new StaffRole(role, tag));
            }
            return roles;
        }

        void StoreStaffRoles(List<StaffRole> roles, DiscordGuild server)
        {
            StringBuilder sb = new StringBuilder();
            foreach (StaffRole role in roles)
                sb.Append($"{role.Role.Id}:{role.Tag}|");

            server.SetValue("staffRoles", sb);
        }
    }

    public class StaffRole 
    {   
        public DiscordRole Role { get; }
        public string Tag { get; }

        public StaffRole(DiscordRole role, string tag)
        {
            Role = role;
            Tag = tag;
        }
    }

    public class StaffMember
    {
        public StaffRole Role { get; }
        public DiscordMember Member { get; }

        public StaffMember(StaffRole role, DiscordMember member)
        {
            Role = role;
            Member = member;
        }
    }
}