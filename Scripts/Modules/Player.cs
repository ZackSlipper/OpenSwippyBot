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
    public class Player
    {
        Bot bot;

        public Player(Bot bot)
        {
            this.bot = bot;

            Run().GetAwaiter();
        }

        async Task Run()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(2));

                foreach (ServerValues values in Bot.GetAllValues()) //Leave channel if inactive
                {
                    if (values.Player.activeChannel != null && values.Player.activeChannel.Users.Count() == 1)
                        await Leave(values.Player, true);
                }
            }
        }

        public static async Task Connect(PlayerValues values)
        {
            values.node = values.lavalink.ConnectedNodes.Values.First();
            await values.node.ConnectAsync(values.activeChannel);

            values.connection = values.node.GetGuildConnection(values.activeChannel.Guild);
            if (!Bot.LavalinkConnectionServerID.ContainsKey(values.connection))
                Bot.LavalinkConnectionServerID.Add(values.connection, values.activeChannel.Guild.Id);

            values.connection.PlaybackFinished += PlaybackFinished;
            values.connection.TrackException += TrackException;
            values.connection.TrackStuck += TrackStuck;
            values.connection.DiscordWebSocketClosed += DiscordWebSocketClosed;
        }

        public static async Task Disconnect(PlayerValues values)
        {
            if (values.connection != null)
            {
                values.connection.PlaybackFinished -= PlaybackFinished;
                values.connection.TrackException -= TrackException;
                values.connection.TrackStuck -= TrackStuck;
                values.connection.DiscordWebSocketClosed -= DiscordWebSocketClosed;

                if (values.connection.IsConnected)
                {
                    await values.connection.StopAsync();
                    await values.connection.DisconnectAsync();
                }
            }

            if (values.connection != null && Bot.LavalinkConnectionServerID.ContainsKey(values.connection))
                Bot.LavalinkConnectionServerID.Remove(values.connection);

            values.isPlaying = false;
            values.connection = null;
            values.node = null;
        }

        public static async Task Leave(PlayerValues values, bool inactivity, bool maintenance = false)
        {
            if (values.isPlaying)
                values.previousTrackUri = values.connection.CurrentState.CurrentTrack.Uri;

            values.TrackQueue.Clear();
            values.loop = false;
            values.failCount = 0;

            await Player.Disconnect(values);

            if (maintenance)
                await values.messageChannel.SendEmbedMessage($"🔧  Left: {values.activeChannel.Name}", "Due to maintenance (The bot is being worked on and might be restarted several times in the following hours)", Bot.ColorMain);
            else
                await values.messageChannel.SendEmbedMessage($"{(inactivity ? "🦗" : "🐾")}  Left: {values.activeChannel.Name}", inactivity ? "Due to inactivity" : "", Bot.ColorMain);

            values.activeChannel = null;
            values.messageChannel = null;
            values.lavalink = null;
        }

        //Events
        static async Task PlaybackFinished(LavalinkGuildConnection connection, TrackFinishEventArgs e)
        {
            PlayerValues values = Bot.GetValues(connection.Guild.Id).Player;
            try
            {
                //await Prebuilt.SendEmbedMessage(messageChannel, $"Playback finished ({isPlaying}): {e.Track.Title} {e.Track.Length} {e.Track.Position} {e.Reason}", "", Bot.ColorError);

                if (values.isPlaying && e.Reason != TrackEndReason.Replaced)
                {
                    if (values.loop)
                    {
                        await connection.PlayAsync(e.Track);
                        return;
                    }

                    values.previousTrackUri = e.Track.Uri;
                    if (values.TrackQueue.Count > 0)
                    {
                        LavalinkTrack track = values.TrackQueue.Dequeue();
                        await connection.PlayAsync(track);
                        values.messageChannel.SendEmbedMessage($"🎵  Now playing: {track.Title}", $"", Bot.ColorMain).GetAwaiter().GetResult();
                        values.messageChannel.SendMessageAsync(track.Uri.ToString()).GetAwaiter().GetResult();
                    }
                    else
                    {
                        values.isPlaying = false;
                        values.messageChannel.SendEmbedMessage($"⏹️  Playback finished", $"Queue empty", Bot.ColorMain).GetAwaiter().GetResult();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        /*static async Task PlayerUpdated(LavalinkGuildConnection connection, PlayerUpdateEventArgs e)
        {
            PlayerValues values = Bot.GetValues(connection.Guild.Id).Player;
            await Prebuilt.SendEmbedMessage(values.messageChannel, $"Update: {e.Position}", "", Bot.ColorError);
        }*/

        static async Task TrackException(LavalinkGuildConnection connection, TrackExceptionEventArgs e)
        {
            PlayerValues values = Bot.GetValues(connection.Guild.Id).Player;
            await values.messageChannel.SendEmbedMessage("⚠️  Track Exception: ", $"{e.Error}{Environment.NewLine}{Environment.NewLine}Reconnecting and resuming playback...", Bot.ColorError);
            await Leave(values, false);

            /*LavalinkTrack track = connection.CurrentState.CurrentTrack;
            TimeSpan position = connection.CurrentState.PlaybackPosition;

            await Disconnect(values);
            await Connect(values);
            if (track != null)
                await ResumePlayback(values, track, position);*/
        }

        static async Task TrackStuck(LavalinkGuildConnection connection, TrackStuckEventArgs e)
        {
            PlayerValues values = Bot.GetValues(connection.Guild.Id).Player;
            await values.messageChannel.SendEmbedMessage("⚠️  Track Stuck", "Reconnecting and resuming playback...", Bot.ColorError);

            LavalinkTrack track = connection.CurrentState.CurrentTrack;
            TimeSpan position = connection.CurrentState.PlaybackPosition;

            await Disconnect(values);
            await Connect(values);
            if (track != null)
                await ResumePlayback(values, track, position);
        }

        static async Task DiscordWebSocketClosed(LavalinkGuildConnection connection, WebSocketCloseEventArgs e)
        {
            PlayerValues values = Bot.GetValues(Bot.LavalinkConnectionServerID[connection]).Player;

            try
            {
                values.failCount++;
                if (values.failCount > 2)
                {
                    await values.messageChannel.SendEmbedMessage("⚠️  Web Socket Closed: ", $"{e.Reason}{Environment.NewLine}Failed too many times. Giving up", Bot.ColorError);
                    await Leave(values, false);
                    return;
                }

                if (values.messageChannel != null)
                    await values.messageChannel.SendEmbedMessage("⚠️  Web Socket Closed: ", $"{e.Reason}{Environment.NewLine}Reconnecting and resuming playback...", Bot.ColorError);

                LavalinkTrack track = connection.CurrentState.CurrentTrack;
                TimeSpan position = connection.CurrentState.PlaybackPosition;

                await Disconnect(values);
                await Connect(values);
                if (track != null)
                    await ResumePlayback(values, track, position);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        //Other
        static async Task ResumePlayback(PlayerValues values, LavalinkTrack track, TimeSpan position)
        {
            try
            {
                await values.connection.PlayAsync(track);
                await values.connection.SeekAsync(position);
                values.isPlaying = true;
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        public static async Task<bool> CanRunCommand(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await IsUserAllowed(ctx)))
                return false;

            if (values.activeChannel == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  SwippyBot not in any voice channel", $"", Bot.ColorError);
                return false;
            }

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel != values.activeChannel)
            {
                await ctx.Channel.SendEmbedMessage("🔈  You're not in the player's currently active voice channel", $"", Bot.ColorError);
                return false;
            }
            return true;
        }

        public static async Task<bool> IsUserAllowed(CommandContext ctx)
        {
            ulong playerBanRoleID = ctx.Guild.GetValue("player_ban_role").ULong();
            if (ctx.Member.Roles.Any(x => x.Id == playerBanRoleID))
            {
                await ctx.Channel.SendEmbedMessage($"⛔  {ctx.User.Username} not allowed to use the player", $"", Bot.ColorWarning);
                return false;
            }
            return true;
        }
    }
}