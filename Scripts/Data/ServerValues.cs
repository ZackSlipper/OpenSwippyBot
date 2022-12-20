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
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace SwippyBot
{
    public class ServerValues
    {
        public PlayerValues Player { get; } = new PlayerValues();
        public CounterValues Counter { get; } = new CounterValues();
        public RoleReactionValues RoleReaction { get; } = new RoleReactionValues();
        public NewJoinerRoleValues NewJoinerRoles { get; } = new NewJoinerRoleValues();
        public ContentFilterValues ContentFilter { get; } = new ContentFilterValues();

        public ServerValues(DiscordGuild server)
        {
            Counter.currentNumber = server.GetValue("counter_number").ULong();
        }
    }

    public class PlayerValues
    {
        public LavalinkExtension lavalink;
        public LavalinkNodeConnection node;
        public LavalinkGuildConnection connection;

        public Uri previousTrackUri;
        Queue<LavalinkTrack> trackQueue = new Queue<LavalinkTrack>();
        public Queue<LavalinkTrack> TrackQueue { get { return trackQueue; } }

        public DiscordChannel activeChannel;
        public DiscordChannel messageChannel;
        public bool isPlaying = false;
        public bool loop = false;

        public int failCount = 0;
    }

    public class CounterValues
    {
        public ulong currentNumber = 0;
        public ulong lastMessageID = 0;
    }

    public class RoleReactionValues
    {
        //Key: MessageID
        public Dictionary<ulong, RoleReactionMessage> Messages { get; } = new Dictionary<ulong, RoleReactionMessage>();
    }

    public class NewJoinerRoleValues
    {
        public List<DiscordRole> Roles { get; } = new List<DiscordRole>();
    }

    public class ContentFilterValues
    {
        public List<ulong> IgnoredChannels { get; } = new List<ulong>();
    }
}