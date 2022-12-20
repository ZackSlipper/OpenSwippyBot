using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Enums;
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
using Genius;
using Genius.Models.Song;
using Genius.Models.Response;
using System.Net.Http;

namespace SwippyBot
{
    public class PlayerCommands : BaseCommandModule
    {
        [Command("join"), Aliases("j")]
        [Description("Join a voice channel")]
        public async Task Join(CommandContext ctx)
        {
            try
            {
                if (!(await Player.IsUserAllowed(ctx)))
                    return;

                PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

                if (values.activeChannel != null)
                {
                    await Leave(ctx);
                }

                if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  You're not in any voice channel", $"", Bot.ColorWarning);
                    return;
                }

                values.lavalink = ctx.Client.GetLavalink();
                if (!values.lavalink.ConnectedNodes.Any())
                {
                    await values.messageChannel.SendEmbedMessage("🌋  Lavalink not connected", $"", Bot.ColorError);
                    return;
                }

                values.activeChannel = ctx.Member.VoiceState.Channel;
                values.messageChannel = ctx.Channel;
                values.loop = false;

                await Player.Connect(values);
                await ctx.Channel.SendEmbedMessage($"📻  Joined: {values.activeChannel.Name}", $"Player message channel set to: {values.messageChannel.Name}", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("leave"), Aliases("l")]
        [Description("Leave a voice channel")]
        public async Task Leave(CommandContext ctx)
        {
            try
            {
                if (!(await Player.CanRunCommand(ctx)))
                    return;

                await Player.Leave(Bot.GetValues(ctx.Guild.Id).Player, false);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("p"), Aliases("play_search")]
        //[Description("Search for and play audio track")]
        public async Task Play(CommandContext ctx, Uri url)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;
            values.failCount = 0;

            if (values.activeChannel == null)
                await Join(ctx);

            if (values.activeChannel == null) //Fix for double "not allowed to use player" messages
                return;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            LavalinkLoadResult trackResult = await values.node.Rest.GetTracksAsync(url);
            if (trackResult.LoadResultType == LavalinkLoadResultType.LoadFailed || trackResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.Channel.SendEmbedMessage($"😿  Track search failed with result: {trackResult.LoadResultType}", $"", Bot.ColorError);
                return;
            }

            foreach (LavalinkTrack track in trackResult.Tracks)
                values.TrackQueue.Enqueue(track);

            if (trackResult.Tracks.Count() > 1)
                await ctx.Channel.SendEmbedMessage($"🎶  Enqueued playlist of {trackResult.Tracks.Count()} tracks named: {trackResult.PlaylistInfo.Name}", $"", Bot.ColorMain);
            else if (values.isPlaying || values.TrackQueue.Count > 1)
                await ctx.Channel.SendEmbedMessage($"🎶  Enqueued: {trackResult.Tracks.First().Title}", $"", Bot.ColorMain);

            if (!values.isPlaying)
            {
                LavalinkTrack track = values.TrackQueue.Dequeue();
                values.isPlaying = true;
                await values.connection.PlayAsync(track);
                await ctx.Channel.SendEmbedMessage($"🎵  Now playing: {track.Title}", $"", Bot.ColorMain);
                await ctx.Channel.SendMessageAsync(track.Uri.ToString());
            }
        }

        [Command("p"), Aliases("play")]
        //[Description("Search for and play audio")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;
            values.failCount = 0;

            if (string.IsNullOrWhiteSpace(search))
            {
                await Play(ctx);
                return;
            }

            if (values.activeChannel == null)
                await Join(ctx);

            if (values.activeChannel == null) //Fix for double "not allowed to use player" messages6
                return;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            LavalinkLoadResult trackResult = await values.node.Rest.GetTracksAsync(search, LavalinkSearchType.Youtube);
            if (trackResult.LoadResultType == LavalinkLoadResultType.LoadFailed || trackResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.Channel.SendEmbedMessage($"😿  Track search failed with result: {trackResult.LoadResultType}", $"", Bot.ColorError);
                return;
            }

            values.TrackQueue.Enqueue(trackResult.Tracks.First());
            if (values.isPlaying || values.TrackQueue.Count > 1)
                await ctx.Channel.SendEmbedMessage($"🎶  Enqueued: {trackResult.Tracks.First().Title}", $"", Bot.ColorMain);

            if (!values.isPlaying)
            {
                LavalinkTrack track = values.TrackQueue.Dequeue();
                values.isPlaying = true;
                await values.connection.PlayAsync(track);
                await ctx.Channel.SendEmbedMessage($"🎵  Now playing: {track.Title}", $"", Bot.ColorMain);
                await ctx.Channel.SendMessageAsync(track.Uri.ToString());
            }
        }

        [Command("r"), Aliases("replay")]
        [Description("Replay last track")]
        public async Task Replay(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;
            values.failCount = 0;

            if (values.activeChannel == null)
                await Join(ctx);

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.previousTrackUri == null)
            {
                await ctx.Channel.SendEmbedMessage($"⚠️  No previous track", $"", Bot.ColorWarning);
                return;
            }

            LavalinkLoadResult trackResult = await values.node.Rest.GetTracksAsync(values.previousTrackUri);
            if (trackResult.LoadResultType == LavalinkLoadResultType.LoadFailed || trackResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.Channel.SendEmbedMessage($"😿  Previous track search failed with result: {trackResult.LoadResultType}", $"", Bot.ColorError);
                return;
            }

            List<LavalinkTrack> tracks = values.TrackQueue.ToList();
            if (values.isPlaying)
                tracks.Insert(0, values.connection.CurrentState.CurrentTrack);
            tracks.Insert(0, trackResult.Tracks.First());
            values.TrackQueue.Clear();
            foreach (LavalinkTrack track in tracks)
                values.TrackQueue.Enqueue(track);

            await ctx.Channel.SendEmbedMessage($"🔁  Replaying: {trackResult.Tracks.First().Title}", $"", Bot.ColorMain);
            if (values.isPlaying)
                await values.connection.StopAsync();
            else
            {
                LavalinkTrack track = values.TrackQueue.Dequeue();
                values.isPlaying = true;
                await values.connection.PlayAsync(track);
                await ctx.Channel.SendEmbedMessage($"🎵  Now playing: {track.Title}", $"", Bot.ColorMain);
                await ctx.Channel.SendMessageAsync(track.Uri.ToString());
            }
        }

        [Command("p"), Aliases("unpause")]
        [Description("Unpause or play a track from the specified YouTube url or search query")]
        public async Task Play(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;
            values.failCount = 0;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                return;
            }

            await values.connection.ResumeAsync();
            await ctx.Channel.SendEmbedMessage($"▶️  Unpaused: {values.connection.CurrentState.CurrentTrack.Title}", $"", Bot.ColorMain);
        }

        [Command("pause"), Aliases("=")]
        [Description("Pause the currently playing audio track")]
        public async Task Pause(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                return;
            }

            await values.connection.PauseAsync();
            await ctx.Channel.SendEmbedMessage($"⏸️  Paused: {values.connection.CurrentState.CurrentTrack.Title}", $"", Bot.ColorMain);
        }

        [Command("skipto"), Aliases("st")]
        [Description("Skip the currently playing audio track and progress to the track number from the playback queue")]
        public async Task SkipTo(CommandContext ctx, uint trackNumber)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                return;
            }

            if (trackNumber > values.TrackQueue.Count)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  Invalid track number", $"", Bot.ColorError);
                return;
            }

            for (int i = 0; i < trackNumber - 1; i++)
                values.TrackQueue.Dequeue();

            await ctx.Channel.SendEmbedMessage($"⏩  Skipped to track: {trackNumber}", $"", Bot.ColorMain);
            await values.connection.StopAsync();
        }

        [Command("skip"), Aliases("s")]
        [Description("Skip the currently playing audio track")]
        public async Task Skip(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                return;
            }

            await values.connection.StopAsync();
            await ctx.Channel.SendEmbedMessage($"⏭️  Skipped: {values.connection.CurrentState.CurrentTrack.Title}", $"", Bot.ColorMain);
        }

        [Command("last"), Aliases("ls")]
        [Description("Skip to the last audio track in the playback queue")]
        public async Task Last(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.TrackQueue.Count == 0)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks in queue", $"", Bot.ColorError);
                return;
            }

            LavalinkTrack track = values.TrackQueue.Last();
            values.TrackQueue.Clear();

            await values.connection.StopAsync();
            await values.connection.PlayAsync(track);

            await ctx.Channel.SendEmbedMessage($"⏩  Skipped to last track in playback queue", $"", Bot.ColorMain);
            await ctx.Channel.SendEmbedMessage($"🎵  Now playing: {track.Title}", $"", Bot.ColorMain);
            await ctx.Channel.SendMessageAsync(track.Uri.ToString());
        }

        [Command("time"), Aliases("t")]
        [Description("Get current playback time and track duration")]
        public async Task Time(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (values.activeChannel == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  SwippyBot not in any voice channel", $"", Bot.ColorError);
                return;
            }

            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel != values.activeChannel)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  You're not in the player's currently active voice channel", $"", Bot.ColorError);
                return;
            }

            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                return;
            }

            await ctx.Channel.SendEmbedMessage($"🕔  {values.connection.CurrentState.PlaybackPosition.ToString().Split('.')[0]} / {values.connection.CurrentState.CurrentTrack.Length}", $"", Bot.ColorMain);
        }

        [Command("seek"), Aliases("se")]
        [Description("Seek some tim in the currently playing track")]
        public async Task Seek(CommandContext ctx, uint minutes, uint seconds)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                return;
            }

            if (seconds > 59)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  Invalid time", $"Seconds can not be higher than 59", Bot.ColorError);
                return;
            }

            TimeSpan position = TimeSpan.FromSeconds(minutes * 60 + seconds);
            if (position > values.connection.CurrentState.CurrentTrack.Length)
            {
                await ctx.Channel.SendEmbedMessage("⌛  Given time is out of range", "", Bot.ColorError);
                return;
            }

            await values.connection.SeekAsync(position);
            await ctx.Channel.SendEmbedMessage($"🕠  Playing current track from {position.ToString()}", $"", Bot.ColorMain);
        }

        [Command("clear"), Aliases("c")]
        [Description("Clears the playback queue")]
        public async Task ClearTrackQueue(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            values.TrackQueue.Clear();
            await ctx.Channel.SendEmbedMessage($"🧹  Track queue cleared", $"", Bot.ColorMain);
        }

        [Command("shuffle"), Aliases("sh")]
        [Description("Shuffle the playback queue")]
        public async Task ShuffleTrackQueue(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            LavalinkTrack[] tracks = values.TrackQueue.ToArray();
            LavalinkTrack tempTrack;
            Random rnd = new Random();

            for (int i = 0; i < tracks.Length; i++)
            {
                int newPosition = rnd.Next(0, tracks.Length);

                tempTrack = tracks[i];
                tracks[i] = tracks[newPosition];
                tracks[newPosition] = tempTrack;
            }

            values.TrackQueue.Clear();
            foreach (LavalinkTrack track in tracks)
                values.TrackQueue.Enqueue(track);

            await ctx.Channel.SendEmbedMessage($"🔀  Playback queue shuffled", $"", Bot.ColorMain);
        }

        [Command("loop"), Aliases("lp")]
        [Description("Loop the current song")]
        public async Task Loop(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            values.loop = !values.loop;
            if (values.loop)
                await ctx.Channel.SendEmbedMessage($"🔁  Looping the current song", $"", Bot.ColorMain);
            else
                await ctx.Channel.SendEmbedMessage($"▶️  No longer looping the current song", $"", Bot.ColorMain);
        }

        [Command("drop_track"), Aliases("d")]
        [Description("Drop a track from from the playback queue")]
        public async Task Drop(CommandContext ctx, int trackNumber)
        {
            try
            {
                PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

                if (!(await Player.CanRunCommand(ctx)))
                    return;

                if (values.connection.CurrentState.CurrentTrack == null)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                    return;
                }

                if (trackNumber <= 0 || trackNumber > values.TrackQueue.Count)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  Invalid track number", $"", Bot.ColorError);
                    return;
                }

                List<LavalinkTrack> tracks = values.TrackQueue.ToList();
                LavalinkTrack droppedTrack = tracks[trackNumber - 1];

                tracks.RemoveAt(trackNumber - 1);
                values.TrackQueue.Clear();

                foreach (LavalinkTrack track in tracks)
                    values.TrackQueue.Enqueue(track);

                await ctx.Channel.SendEmbedMessage($"🔥  Dropped track: **{trackNumber}. {droppedTrack.Title}**", $"", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("move_track"), Aliases("m")]
        [Description("Move the given track to a given position in the playback queue")]
        public async Task Move(CommandContext ctx, int trackNumber, int numberInQueue)
        {
            try
            {
                PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

                if (!(await Player.CanRunCommand(ctx)))
                    return;

                if (values.connection.CurrentState.CurrentTrack == null)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                    return;
                }

                if (trackNumber <= 0 || trackNumber > values.TrackQueue.Count)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  Invalid track number", $"", Bot.ColorError);
                    return;
                }

                if (trackNumber <= 0 || trackNumber > values.TrackQueue.Count)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  Invalid target number in queue", $"", Bot.ColorError);
                    return;
                }

                List<LavalinkTrack> tracks = values.TrackQueue.ToList();
                LavalinkTrack movedTrack = tracks[trackNumber - 1];

                tracks.RemoveAt((int)trackNumber - 1);
                tracks.Insert(numberInQueue - 1, movedTrack);

                values.TrackQueue.Clear();

                foreach (LavalinkTrack track in tracks)
                    values.TrackQueue.Enqueue(track);

                await ctx.Channel.SendEmbedMessage($"🔼  Moved track: **{trackNumber}. {movedTrack.Title}** to **{numberInQueue}**", $"", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("raise_track"), Aliases("raise", "rs")]
        [Description("Raise the given track to be next in the playback queue")]
        public async Task Raise(CommandContext ctx, int trackNumber)
        {
            await RaiseTrack(ctx, trackNumber);
        }

        public async Task<bool> RaiseTrack(CommandContext ctx, int trackNumber)
        {
            try
            {
                PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

                if (!(await Player.CanRunCommand(ctx)))
                    return false;

                if (values.connection.CurrentState.CurrentTrack == null)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  There are no tracks loaded", $"", Bot.ColorError);
                    return false;
                }

                if (trackNumber <= 0 || trackNumber > values.TrackQueue.Count)
                {
                    await ctx.Channel.SendEmbedMessage("⚠️  Invalid track number", $"", Bot.ColorError);
                    return false;
                }

                List<LavalinkTrack> tracks = values.TrackQueue.ToList();
                LavalinkTrack raisedTrack = tracks[trackNumber - 1];

                tracks.RemoveAt((int)trackNumber - 1);
                tracks.Insert(0, raisedTrack);

                values.TrackQueue.Clear();

                foreach (LavalinkTrack track in tracks)
                    values.TrackQueue.Enqueue(track);

                await ctx.Channel.SendEmbedMessage($"⏫  Raised track: **{trackNumber}. {raisedTrack.Title}**", $"", Bot.ColorMain);
                return true;
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
                return false;
            }
        }

        [Command("raise_track_and_skip"), Aliases("rss")]
        [Description("Raise the given track to be next in the playback queue")]
        public async Task RaiseAndSkip(CommandContext ctx, int trackNumber)
        {
            try
            {
                if (await RaiseTrack(ctx, trackNumber))
                    await Skip(ctx);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("stop"), Aliases("sp")]
        [Description("Stops playback and clears the queue")]
        public async Task Stop(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

            if (!(await Player.CanRunCommand(ctx)))
                return;

            await ClearTrackQueue(ctx);
            if (values.connection != null && values.connection.CurrentState.CurrentTrack != null)
                await values.connection.StopAsync();
            values.loop = false;
        }

        [Command("list"), Aliases("queue", "q")]
        [Description("Lists all the tracks in the playback queue")]
        public async Task ListTrackQueue(CommandContext ctx)
        {
            try
            {
                PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;

                if (values.TrackQueue.Count == 0)
                {
                    await ctx.Message.SendReplyEmbedMessage($"🦗  Playback queue: Empty", "", Bot.ColorMain);
                    return;
                }

                LavalinkTrack[] tracks = values.TrackQueue.ToArray();
                List<string> pageContent = new List<string>();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < tracks.Length; i++)
                {
                    //Definition info
                    sb.AppendLine($"**{i + 1}.** ({tracks[i].Length}) **{tracks[i].Title}**");
                    sb.AppendLine($"　　　{tracks[i].Uri}");

                    //Content pagination
                    if (sb.Length >= 2000)
                    {
                        pageContent.Add(sb.ToString());
                        sb.Clear();
                    }
                }

                //Add the remaining items to their own page if they weren't added in the loop
                if (sb.Length > 0)
                    pageContent.Add(sb.ToString());

                //Build pages
                if (pageContent.Count > 0)
                {
                    List<Page> pages = new List<Page>();
                    for (int i = 0; i < pageContent.Count; i++)
                    {
                        pages.Add(new Page("", new DiscordEmbedBuilder()
                        {
                            Title = $"🎶  Playback queue:  (page {i + 1}/{pageContent.Count} )",
                            Description = pageContent[i],
                            Color = Bot.ColorMain
                        }));
                    }
                    await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, null, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteMessage, TimeSpan.FromMinutes(5));
                    return;
                }

                await ctx.Message.SendReplyEmbedMessage($"🦗  Playback queue: Empty", "", Bot.ColorMain);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("ly"), Aliases("lyrics")]
        [Description("Search and show lyrics ofs the currently playing song")]
        public async Task Lyrics(CommandContext ctx)
        {
            PlayerValues values = Bot.GetValues(ctx.Guild.Id).Player;
            if (values.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendEmbedMessage("⚠️  No song playing to show lyrics for", "", Bot.ColorError);
                return;
            }
            await LyricSearch(ctx, values.connection.CurrentState.CurrentTrack.Title);
        }

        [Command("ly"), Aliases("lyrics_search")]
        [Description("Search and show lyrics of a given song name")]
        public async Task LyricSearch(CommandContext ctx, [RemainingText] string searchQuery)
        {
            DiscordMessage message = await ctx.Channel.SendEmbedMessage("🎼  Requesting lyrics...", "", Bot.ColorMain);

            try
            {
                Song songInfo = await SearchForSongInfo(searchQuery);
                if (songInfo == null)
                {
                    await message.DeleteAsync();
                    await ctx.Channel.SendEmbedMessage("😿  No lyrics found", "", Bot.ColorMain);
                    return;
                }

                Tuple<string, string> artistAndTitle = GetSongArtistAndTitle(songInfo);
                string lyrics = await GetSongLyrics(artistAndTitle.Item1, artistAndTitle.Item2);

                //await ctx.Channel.SendMessageAsync($"https://www.azlyrics.com/lyrics/{artistAndTitle.Item1}/{artistAndTitle.Item2}.html");

                if (string.IsNullOrEmpty(lyrics))
                {
                    await message.DeleteAsync();
                    await ctx.Channel.SendEmbedMessage("😿  No lyrics found", "", Bot.ColorMain);
                    return;
                }

                string title = $"{songInfo.FullTitle}";
                if (lyrics.Length > 1750)
                {
                    string[] sections = lyrics.Split($"{Environment.NewLine}{Environment.NewLine}");
                    bool splitInLines = sections.Any(x => x.Length > 1750);
                    if (splitInLines) //Split and print by lines
                    {
                        string[] lines = lyrics.Split($"{Environment.NewLine}", StringSplitOptions.None);
                        StringBuilder sb = new StringBuilder();

                        for (int i = 0; i < lines.Length; i++)
                        {
                            if (sb.Length + lines[i].Length > 1750)
                            {
                                await ctx.Channel.SendEmbedMessage(title, sb.ToString(), Bot.ColorMain);
                                sb.Clear();
                                title = " ";
                            }

                            sb.AppendLine(string.IsNullOrWhiteSpace(lines[i]) ? " " : lines[i]);
                        }

                        if (!string.IsNullOrWhiteSpace(sb.ToString()))
                            await ctx.Channel.SendEmbedMessage(title, sb.ToString(), Bot.ColorMain);
                    }
                    else //Split and print by sections (paragraphs)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < sections.Length; i++)
                        {
                            if (sb.Length + sections[i].Length > 1750)
                            {
                                await ctx.Channel.SendEmbedMessage(title, sb.ToString(), Bot.ColorMain);
                                title = " ";
                                sb.Clear();
                            }

                            sb.AppendLine(sections[i]);
                            sb.AppendLine(" ");
                        }

                        if (!string.IsNullOrWhiteSpace(sb.ToString()))
                            await ctx.Channel.SendEmbedMessage(title, sb.ToString(), Bot.ColorMain);
                    }
                }
                else //Print all the lyrics in one message
                    await ctx.Channel.SendEmbedMessage($"🎼  {title}", lyrics, Bot.ColorMain);

            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }

            await message.DeleteAsync();
        }

        async Task<Song> SearchForSongInfo(string searchQuery)
        {
            //Format query
            searchQuery = searchQuery.ToLower();

            StringBuilder query = new StringBuilder();
            string[] queryParts = searchQuery.Split("-", StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < queryParts.Length; i++)
            {
                int openBracketIndex = queryParts[i].IndexOf("(");
                if (openBracketIndex >= 0)
                    queryParts[i] = queryParts[i].Remove(openBracketIndex);
                query.Append(queryParts[i]);
                query.Append(" ");
            }

            //Search
            SearchResponse response = await Bot.GeniusClient.SearchClient.Search(query.ToString());
            if (response.Response.Hits.Count == 0)
                return null;

            return response.Response.Hits[0].Result;
        }

        Tuple<string, string> GetSongArtistAndTitle(Song song)
        {
            string text = song.PrimaryArtist.Name.ToLower();
            int openBracketIndex = text.IndexOf("(");
            if (openBracketIndex >= 0)
                text = text.Remove(openBracketIndex);

            StringBuilder songArtist = new StringBuilder(text);
            //Remove every symbol that isn't a letter
            for (int i = songArtist.Length - 1; i >= 0; i--)
            {
                if (!char.IsLetter(songArtist[i]) && !char.IsDigit(songArtist[i]))
                    songArtist.Remove(i, 1);
            }

            string artist = songArtist.ToString();
            if (artist.StartsWith("the"))
                artist = artist.Remove(0, 3);


            text = song.Title.Replace(" ", "").ToLower();
            openBracketIndex = text.IndexOf("(");
            if (openBracketIndex >= 0)
                text = text.Remove(openBracketIndex);

            StringBuilder songTitle = new StringBuilder(text);
            //Remove every symbol that isn't a letter
            for (int i = songTitle.Length - 1; i >= 0; i--)
            {
                if (!char.IsLetter(songTitle[i]) && !char.IsDigit(songTitle[i]))
                    songTitle.Remove(i, 1);
            }

            return new Tuple<string, string>(artist, songTitle.ToString());
        }

        async Task<string> GetSongLyrics(string artist, string title)
        {
            using (HttpResponseMessage httpResponse = await Bot.HttpClient.GetAsync($"https://www.azlyrics.com/lyrics/{artist}/{title}.html"))
            {
                string lyrics = (await httpResponse.Content.ReadAsStringAsync());

                string startIndicatorString = "<!-- Usage of azlyrics.com content by any third-party lyrics provider is prohibited by our licensing agreement. Sorry about that. -->";
                int startIndex = lyrics.IndexOf(startIndicatorString);
                if (startIndex <= 0)
                    return "";
                startIndex += startIndicatorString.Length;

                int endIndex = lyrics.IndexOf("</div>", startIndex);
                if (endIndex <= 0)
                    return "";

                return lyrics.Substring(startIndex, endIndex - startIndex).Replace("<br>", "");
            }
        }

        [Command("player_leave_all"), Aliases("la")]
        [Description("Leave all voice channels in all servers")]
        [RequireOwner]
        public async Task LeaveAll(CommandContext ctx)
        {
            try
            {
                int leftPlayers = 0;
                foreach (ServerValues values in Bot.GetAllValues())
                {
                    if (values.Player.activeChannel != null)
                    {
                        await Player.Leave(values.Player, false, true);
                        leftPlayers++;
                    }
                }

                if (leftPlayers > 0)
                    await ctx.Channel.SendEmbedMessage($"🔧  Left voice channels in {leftPlayers} servers", $"", Bot.ColorSystem);
                else
                    await ctx.Channel.SendEmbedMessage("🔧  No servers with active players", $"", Bot.ColorWarning);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }


        //Player Ban Role
        [Command("set_player_ban_role")]
        [Description("Sets the role restricting the user from using the player")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task SetPlayerBanRole(CommandContext ctx, DiscordRole role)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("player_ban_role").ULong();
                if (currentRoleID == role.Id)
                {
                    await ctx.Channel.SendEmbedMessage($"Player ban role already set to: ", $"{role.Mention}", Bot.ColorSystem);
                    return;
                }

                ctx.Guild.SetValue("player_ban_role", role.Id);
                await ctx.Channel.SendEmbedMessage($"Player ban role set to: ", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("unset_player_ban_role")]
        [Description("Unsets the role restricting the user from using the player")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task UnsetPlayerBanRole(CommandContext ctx)
        {
            try
            {
                ulong currentRoleID = ctx.Guild.GetValue("player_ban_role").ULong();
                if (currentRoleID == 0)
                {
                    await ctx.Channel.SendEmbedMessage($"Player ban role already not set", $"", Bot.ColorWarning);
                    return;
                }

                ctx.Guild.SetValue("player_ban_role", "");
                await ctx.Channel.SendEmbedMessage($"Player ban role unset", $"", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        [Command("get_player_ban_role")]
        [Description("Gets the role restricting the user from using the player")]
        [RequireRoles(RoleCheckMode.Any, "Swippy")]
        public async Task GetPlayerBanRole(CommandContext ctx)
        {
            try
            {
                DiscordRole role = ctx.Guild.GetValue("player_ban_role").DiscordRole(ctx.Guild);
                if (role == null)
                {
                    await ctx.Channel.SendEmbedMessage($"Player ban role not set", $"", Bot.ColorSystem);
                    return;
                }

                await ctx.Channel.SendEmbedMessage($"Player ban role:", $"{role.Mention}", Bot.ColorSystem);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        /*[Command("player_help")]
        [Description("Show a list of all player commands")]
        public async Task Help(CommandContext ctx)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("• **h**,  **player_help**");
            sb.AppendLine("• **j**,  **join**");
            sb.AppendLine("• **l**,  **leave**");
            sb.AppendLine("• **p**,  **play [url]**");
            sb.AppendLine("• **p**,  **play_search [search string]**");
            sb.AppendLine("• **p**,  **unpause**");
            sb.AppendLine("• **=**,  **pause**");
            sb.AppendLine("• **st**, **skipto [track number]**");
            sb.AppendLine("• **s**,  **skip**");
            sb.AppendLine("• **ls**, **last**");
            sb.AppendLine("• **t**,  **time**");
            sb.AppendLine("• **se**, **seek [minutes] [seconds]**");
            sb.AppendLine("• **c**,  **clear**");
            sb.AppendLine("• **sh**, **shuffle**");
            sb.AppendLine("• **lp**, **loop**");
            sb.AppendLine("• **d**,  **drop [track number]**");
            sb.AppendLine("• **sp**, **stop**");
            sb.AppendLine("• **q**,  **queue**, **list**");
            sb.AppendLine("• **ly**, **lyrics**");
            sb.AppendLine("• **ly**, **lyrics_search [search string]**");

            await ctx.Channel.SendEmbedMessage($"Player commands:", sb.ToString(), Bot.ColorMain);
        }*/
    }
}