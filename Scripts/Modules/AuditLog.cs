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
    public class AuditLog
    {
        Bot bot;

        Dictionary<ulong, DiscordChannel> adminLogChannels = new Dictionary<ulong, DiscordChannel>();
        Dictionary<ulong, DiscordChannel> messageLogChannels = new Dictionary<ulong, DiscordChannel>();
        Dictionary<ulong, DiscordChannel> userLogChannels = new Dictionary<ulong, DiscordChannel>();
        Dictionary<ulong, DiscordChannel> vcLogChannels = new Dictionary<ulong, DiscordChannel>();

        Dictionary<ulong, UserAvatars> userAvatars = new Dictionary<ulong, UserAvatars>();

        public AuditLog(Bot bot)
        {
            this.bot = bot;

            //Admin log
            bot.Client.ChannelCreated += ChannelCreated;
            bot.Client.ChannelDeleted += ChannelDeleted;
            bot.Client.ChannelPinsUpdated += ChannelPinsUpdated;
            bot.Client.ChannelUpdated += ChannelUpdated;

            bot.Client.GuildUpdated += GuildUpdated;
            bot.Client.GuildEmojisUpdated += GuildEmojisUpdated;
            bot.Client.GuildStickersUpdated += GuildStickersUpdated;

            bot.Client.GuildRoleCreated += GuildRoleCreated;
            bot.Client.GuildRoleDeleted += GuildRoleDeleted;
            bot.Client.GuildRoleUpdated += GuildRoleUpdated;

            //bot.Client.InviteCreated += InviteCreated;
            //bot.Client.InviteDeleted += InviteDeleted; //Managed by InviteController

            //Message log
            //bot.Client.MessageCreated += MessageCreated;
            bot.Client.MessageDeleted += MessageDeleted;
            bot.Client.MessageReactionAdded += MessageReactionAdded;
            bot.Client.MessageReactionRemoved += MessageReactionRemoved;
            bot.Client.MessageReactionRemovedEmoji += MessageReactionRemovedEmoji;
            bot.Client.MessageReactionsCleared += MessageReactionsCleared;
            bot.Client.MessagesBulkDeleted += MessagesBulkDeleted;
            bot.Client.MessageUpdated += MessageUpdated;

            //User log
            bot.Client.GuildBanAdded += GuildBanAdded;
            bot.Client.GuildBanRemoved += GuildBanRemoved;

            bot.Client.GuildMemberAdded += GuildMemberAdded;
            bot.Client.GuildMemberRemoved += GuildMemberRemoved;
            bot.Client.GuildMemberUpdated += GuildMemberUpdated;

            //bot.Client.UserSettingsUpdated += UserSettingsUpdated;
            //bot.Client.UserUpdated += UserUpdated;
            bot.Client.VoiceStateUpdated += VoiceStateUpdated;

            //Presence log
            //bot.Client.PresenceUpdated += PresenceUpdated;
        }

        public void LoadUserAvatars(DiscordGuild guild)
        {
            var members = guild.GetAllMembersAsync().GetAwaiter().GetResult();
            foreach (DiscordMember member in members)
                SetMemberAvatars(member, guild);
        }

        void SetMemberAvatars(DiscordMember member, DiscordGuild guild)
        {
            UserAvatars avatars;
            if (!userAvatars.ContainsKey(member.Id))
                userAvatars.Add(member.Id, new UserAvatars());
            avatars = userAvatars[member.Id];

            avatars.mainAvatar = member.AvatarUrl;
            avatars.guildAvatars[guild.Id] = member.GuildAvatarUrl;
        }

        public void UpdateServerChannels(DiscordGuild server)
        {
            DiscordChannel channel;

            channel = server.GetValueChannel("audit_log_admin_channel");
            if (channel != null)
                adminLogChannels[server.Id] = channel;

            channel = server.GetValueChannel("audit_log_message_channel");
            if (channel != null)
                messageLogChannels[server.Id] = channel;

            channel = server.GetValueChannel("audit_log_user_channel");
            if (channel != null)
                userLogChannels[server.Id] = channel;

            channel = server.GetValueChannel("audit_log_vc_channel");
            if (channel != null)
                vcLogChannels[server.Id] = channel;
        }

        async Task ChannelCreated(DiscordClient client, ChannelCreateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Channel.Mention);
            sb.AppendLine($"Name: {e.Channel.Name}");
            sb.AppendLine($"Category: {e.Channel.IsCategory}");
            sb.AppendLine($"ID: {e.Channel.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "🦜 Channel Created",
                Description = sb.ToString(),
                Color = Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Name: {e.Channel.Name}");
            sb.AppendLine($"Category: {e.Channel.IsCategory}");
            sb.AppendLine($"ID: {e.Channel.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "🦜 Channel Deleted",
                Description = sb.ToString(),
                Color = Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task ChannelPinsUpdated(DiscordClient client, ChannelPinsUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "📌 Channel Pins Updated",
                Description = $"{e.Channel.Mention}",
                Color = Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task ChannelUpdated(DiscordClient client, ChannelUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            //Channel Topic
            if (e.ChannelBefore.Topic != e.ChannelAfter.Topic)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"🦜 Channel Topic {(string.IsNullOrWhiteSpace(e.ChannelBefore.Topic) ? "Set" : (string.IsNullOrWhiteSpace(e.ChannelAfter.Topic) ? "Removed" : "Updated"))}",
                    Description = $"{e.ChannelAfter.Mention}{Environment.NewLine}ID: {e.ChannelBefore.Id}",
                    Color = Bot.ColorSystem
                };

                if (string.IsNullOrWhiteSpace(e.ChannelBefore.Topic))
                    embedBuilder.Description += $"{Environment.NewLine} Topic: {e.ChannelAfter.Topic}";
                else if (string.IsNullOrWhiteSpace(e.ChannelAfter.Topic))
                    embedBuilder.Description += $"{Environment.NewLine} Topic: {e.ChannelBefore.Topic}";
                else
                {
                    embedBuilder.AddField("Before", e.ChannelBefore.Topic, true);
                    embedBuilder.AddField("After", e.ChannelAfter.Topic, true);
                }
                await channel.SendMessageAsync(embedBuilder);
            }

            //Channel Name
            if (e.ChannelBefore.Name != e.ChannelAfter.Name)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"🦜 Channel Name Updated",
                    Description = $"{e.ChannelAfter.Mention}{Environment.NewLine}ID: {e.ChannelBefore.Id}",
                    Color = Bot.ColorSystem
                };

                embedBuilder.AddField("Before", e.ChannelBefore.Name, true);
                embedBuilder.AddField("After", e.ChannelAfter.Name, true);
                await channel.SendMessageAsync(embedBuilder);
            }

            //Channel NSFW
            if (e.ChannelBefore.IsNSFW != e.ChannelAfter.IsNSFW)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"🦜 Channel Is NSFW Updated",
                    Description = $"{e.ChannelAfter.Mention}{Environment.NewLine}ID: {e.ChannelBefore.Id}",
                    Color = Bot.ColorSystem
                };

                embedBuilder.AddField("Before", e.ChannelBefore.IsNSFW.ToString(), true);
                embedBuilder.AddField("After", e.ChannelAfter.IsNSFW.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }

            //Channel Private
            if (e.ChannelBefore.IsPrivate != e.ChannelAfter.IsPrivate)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"🦜 Channel Is Private Updated",
                    Description = $"{e.ChannelAfter.Mention}{Environment.NewLine}ID: {e.ChannelBefore.Id}",
                    Color = Bot.ColorSystem
                };

                embedBuilder.AddField("Before", e.ChannelBefore.IsPrivate.ToString(), true);
                embedBuilder.AddField("After", e.ChannelAfter.IsPrivate.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }
        }

        async Task GuildBanAdded(DiscordClient client, GuildBanAddEventArgs e)
        {
            DiscordChannel channel;
            if (userLogChannels.ContainsKey(e.Guild.Id))
                channel = userLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Member.Mention);
            sb.AppendLine($"Username: {e.Member.Username}");
            sb.AppendLine($"User ID: {e.Member.Id}");
            try
            {
                sb.AppendLine($"Reason: {(await e.Guild.GetBanAsync(e.Member.Id)).Reason}");
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "❌ User Banned",
                Description = sb.ToString(),
                Color = Bot.ColorSystem,
                ImageUrl = e.Member.AvatarUrl
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task GuildBanRemoved(DiscordClient client, GuildBanRemoveEventArgs e)
        {
            DiscordChannel channel;
            if (userLogChannels.ContainsKey(e.Guild.Id))
                channel = userLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Member.Mention);
            sb.AppendLine($"ID: {e.Member.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "✔️ User Unbanned",
                Description = sb.ToString(),
                Color = Bot.ColorSystem,
                ImageUrl = e.Member.AvatarUrl
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task GuildUpdated(DiscordClient client, GuildUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.GuildAfter.Id))
                channel = adminLogChannels[e.GuildAfter.Id];
            else
                return;

            if (e.GuildBefore.Banner != e.GuildAfter.Banner)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🖼️ Server Banner Updated",
                    ImageUrl = e.GuildAfter.BannerUrl,
                    Color = Bot.ColorSystem,
                };
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.IconHash != e.GuildAfter.IconHash)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🖼️ Server Icon Updated",
                    ImageUrl = e.GuildAfter.IconUrl,
                    Color = Bot.ColorSystem,
                };
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.Description != e.GuildAfter.Description)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "📝 Server Description Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.Description);
                embedBuilder.AddField("After", e.GuildAfter.Description);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.DiscoverySplashHash != e.GuildAfter.DiscoverySplashHash)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🖼️ Server Discovery Splash Updated",
                    ImageUrl = e.GuildAfter.DiscoverySplashUrl,
                    Color = Bot.ColorSystem
                };
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.IsLarge != e.GuildAfter.IsLarge)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🌟 Server Is Large Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.IsLarge.ToString(), true);
                embedBuilder.AddField("After", e.GuildAfter.IsLarge.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.IsNSFW != e.GuildAfter.IsNSFW)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🔥 Server Is NSFW Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.IsLarge.ToString(), true);
                embedBuilder.AddField("After", e.GuildAfter.IsLarge.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.MfaLevel != e.GuildAfter.MfaLevel)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🛡️ Server Multi Factor Authentication Level Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.MfaLevel.ToString(), true);
                embedBuilder.AddField("After", e.GuildAfter.MfaLevel.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.Name != e.GuildAfter.Name)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "📝 Server Name Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.Name.ToString(), true);
                embedBuilder.AddField("After", e.GuildAfter.Name.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.Owner.Id != e.GuildAfter.Owner.Id)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "⭐ Server Owner Updated",
                    ImageUrl = e.GuildAfter.Owner.AvatarUrl,
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", $"{e.GuildBefore.Owner.Mention}{Environment.NewLine}ID:{e.GuildBefore.Owner.Id}", true);
                embedBuilder.AddField("After", $"{e.GuildAfter.Owner.Mention}{Environment.NewLine}ID:{e.GuildAfter.Owner.Id}", true);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.PremiumTier != e.GuildAfter.PremiumTier)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "⭐ Server Premium Tier (Boost) Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.PremiumTier.ToString(), true);
                embedBuilder.AddField("After", e.GuildAfter.PremiumTier.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.SplashHash != e.GuildAfter.SplashHash)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🖼️ Server Splash Updated",
                    ImageUrl = e.GuildAfter.SplashUrl,
                    Color = Bot.ColorSystem,
                };
                await channel.SendMessageAsync(embedBuilder);
            }

            if (e.GuildBefore.VerificationLevel != e.GuildAfter.VerificationLevel)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🛡️ Server Verification Level Updated",
                    Color = Bot.ColorSystem
                };
                embedBuilder.AddField("Before", e.GuildBefore.VerificationLevel.ToString(), true);
                embedBuilder.AddField("After", e.GuildAfter.VerificationLevel.ToString(), true);
                await channel.SendMessageAsync(embedBuilder);
            }
        }

        async Task GuildEmojisUpdated(DiscordClient client, GuildEmojisUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            if (e.EmojisAfter.Count > e.EmojisBefore.Count)
            {
                foreach (var emoji in e.EmojisAfter.Values.Where(x => !e.EmojisBefore.Values.Any(x2 => x.Id == x2.Id)))
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "🐱 Emoji Added",
                        ImageUrl = emoji.Url,
                        Color = Bot.ColorSystem
                    };
                    await channel.SendMessageAsync(embedBuilder);
                }
            }
            else if (e.EmojisAfter.Count < e.EmojisBefore.Count)
            {
                foreach (var emoji in e.EmojisBefore.Values.Where(x => !e.EmojisAfter.Values.Any(x2 => x.Id == x2.Id)))
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "😿 Emoji Removed",
                        ImageUrl = emoji.Url,
                        Color = Bot.ColorSystem
                    };
                    await channel.SendMessageAsync(embedBuilder);
                }
            }
            else
            {
                foreach (var emojiBefore in e.EmojisBefore.Values)
                {
                    foreach (var emojiAfter in e.EmojisAfter.Values)
                    {
                        if (emojiBefore.Id == emojiAfter.Id && emojiBefore.Name != emojiAfter.Name)
                        {
                            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                            {
                                Title = "📝 Emoji Name Changed",
                                ImageUrl = emojiAfter.Url,
                                Color = Bot.ColorSystem
                            };
                            embedBuilder.AddField("Before", emojiBefore.Name, true);
                            embedBuilder.AddField("After", emojiAfter.Name, true);
                            await channel.SendMessageAsync(embedBuilder);
                            break;
                        }
                    }
                }

                foreach (var emoji in e.EmojisBefore.Values.Where(x => !e.EmojisAfter.Values.Any(x2 => x == x2)))
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "📝 Emoji Updated",
                        ImageUrl = emoji.Url,
                        Color = Bot.ColorSystem
                    };
                    await channel.SendMessageAsync(embedBuilder);
                }
            }
        }

        async Task GuildStickersUpdated(DiscordClient client, GuildStickersUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            if (e.StickersAfter.Count > e.StickersBefore.Count)
            {
                foreach (var sticker in e.StickersAfter.Values.Where(x => !e.StickersBefore.Values.Any(x2 => x.Id == x2.Id)))
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = $"🖼️ Sticker Added: {sticker.Name}",
                        ImageUrl = sticker.StickerUrl,
                        Color = Bot.ColorSystem
                    };
                    await channel.SendMessageAsync(embedBuilder);
                }
            }
            else if (e.StickersAfter.Count < e.StickersBefore.Count)
            {
                foreach (var sticker in e.StickersBefore.Values.Where(x => !e.StickersAfter.Values.Any(x2 => x.Id == x2.Id)))
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = $"❌ Sticker Removed: {sticker.Name}",
                        ImageUrl = sticker.StickerUrl,
                        Color = Bot.ColorSystem
                    };
                    await channel.SendMessageAsync(embedBuilder);
                }
            }
        }

        async Task GuildMemberAdded(DiscordClient client, GuildMemberAddEventArgs e)
        {
            DiscordChannel channel;
            if (userLogChannels.ContainsKey(e.Guild.Id))
                channel = userLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Member.Mention);
            sb.AppendLine($"ID: {e.Member.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "👋 User Joined",
                Description = sb.ToString(),
                Color = Bot.ColorSystem,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
            };

            SetMemberAvatars(e.Member, e.Guild);
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task GuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
        {
            DiscordChannel channel;
            if (userLogChannels.ContainsKey(e.Guild.Id))
                channel = userLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Member.Mention);
            sb.AppendLine($"ID: {e.Member.Id}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "😿 User Left",
                Description = sb.ToString(),
                Color = Bot.ColorSystem,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task GuildMemberUpdated(DiscordClient client, GuildMemberUpdateEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                DiscordChannel channel;
                if (userLogChannels.ContainsKey(e.Guild.Id))
                    channel = userLogChannels[e.Guild.Id];
                else
                    return;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(e.Member.Mention);
                sb.AppendLine($"User ID: {e.Member.Id}");

                if (e.NicknameAfter != e.NicknameBefore)
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = ":page_facing_up: User Nickname Changed",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem,
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Member.AvatarUrl, Height = 48, Width = 48 }
                    };

                    embedBuilder.AddField("Before", string.IsNullOrWhiteSpace(e.NicknameBefore) ? "[None]" : e.NicknameBefore, true);
                    embedBuilder.AddField("After", string.IsNullOrWhiteSpace(e.NicknameAfter) ? "[None]" : e.NicknameAfter, true);
                    await channel.SendMessageAsync(embedBuilder);
                }

                //Roles
                bool containsRole;

                //Removed roles
                List<DiscordRole> removedRoles = new List<DiscordRole>();
                foreach (var role in e.RolesBefore)
                {
                    containsRole = false;
                    foreach (var afterRole in e.RolesAfter)
                    {
                        if (role.Id == afterRole.Id)
                        {
                            containsRole = true;
                            break;
                        }
                    }
                    if (!containsRole)
                        removedRoles.Add(role);
                }

                if (removedRoles.Count > 0)
                {
                    if (removedRoles.Count > 1)
                    {
                        sb.AppendLine($"");
                        sb.AppendLine($"Roles:");
                        foreach (var role in removedRoles)
                            sb.AppendLine(role.Mention);
                    }
                    else
                        sb.AppendLine($"Roles: {removedRoles[0].Mention}");

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = $":red_square: User Role{(removedRoles.Count == 1 ? "" : "s")} Removed",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem,
                    };

                    await channel.SendMessageAsync(embedBuilder);
                }

                //Added roles
                List<DiscordRole> addedRoles = new List<DiscordRole>();
                foreach (var role in e.RolesAfter)
                {
                    containsRole = false;
                    foreach (var beforeRole in e.RolesBefore)
                    {
                        if (role.Id == beforeRole.Id)
                        {
                            containsRole = true;
                            break;
                        }
                    }
                    if (!containsRole)
                        addedRoles.Add(role);
                }

                if (addedRoles.Count > 0)
                {
                    if (addedRoles.Count > 1)
                    {
                        sb.AppendLine($"");
                        sb.AppendLine($"Roles:");
                        foreach (var role in addedRoles)
                            sb.AppendLine(role.Mention);
                    }
                    else
                        sb.AppendLine($"Roles: {addedRoles[0].Mention}");

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = $":green_square: User Role{(addedRoles.Count == 1 ? "" : "s")} Added",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem,
                    };

                    await channel.SendMessageAsync(embedBuilder);
                }

                //Profile picture
                if (userAvatars.ContainsKey(e.Member.Id) && e.Member.AvatarUrl != userAvatars[e.Member.Id].mainAvatar)
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = ":fox: User Profile Picture Changed",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem,
                        ImageUrl = e.Member.AvatarUrl
                    };

                    userAvatars[e.Member.Id].mainAvatar = e.Member.AvatarUrl;
                    await channel.SendMessageAsync(embedBuilder);
                }

                if (userAvatars.ContainsKey(e.Member.Id) && userAvatars[e.Member.Id].guildAvatars.ContainsKey(e.Guild.Id) && e.Member.GuildAvatarUrl != userAvatars[e.Member.Id].guildAvatars[e.Guild.Id])
                {
                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = ":wolf: User Server Profile Picture Changed",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem,
                        ImageUrl = e.Member.GuildAvatarUrl
                    };

                    userAvatars[e.Member.Id].guildAvatars[e.Guild.Id] = e.Member.GuildAvatarUrl;
                    await channel.SendMessageAsync(embedBuilder);
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task GuildRoleCreated(DiscordClient client, GuildRoleCreateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Role.Mention);
            sb.AppendLine($"Name: {e.Role.Name}");
            sb.AppendLine($"ID: {e.Role.Id}");
            sb.AppendLine($"Color: {e.Role.Color}");
            sb.AppendLine($"Shown above others: {e.Role.IsHoisted}");
            sb.AppendLine($"Mentionable: {e.Role.IsMentionable}");
            sb.AppendLine($"Managed by integration: {e.Role.IsManaged}");
            sb.AppendLine($"Permissions: {e.Role.Permissions.ToPermissionString()}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "🔵 Role Created",
                Description = sb.ToString(),
                Color = Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task GuildRoleDeleted(DiscordClient client, GuildRoleDeleteEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(e.Role.Mention);
            sb.AppendLine($"Name: {e.Role.Name}");
            sb.AppendLine($"ID: {e.Role.Id}");
            sb.AppendLine($"Color: {e.Role.Color}");
            sb.AppendLine($"Shown above others: {e.Role.IsHoisted}");
            sb.AppendLine($"Mentionable: {e.Role.IsMentionable}");
            sb.AppendLine($"Managed by integration: {e.Role.IsManaged}");
            sb.AppendLine($"Permissions: {e.Role.Permissions.ToPermissionString()}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = "🔴 Role Deleted",
                Description = sb.ToString(),
                Color = Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task GuildRoleUpdated(DiscordClient client, GuildRoleUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (adminLogChannels.ContainsKey(e.Guild.Id))
                channel = adminLogChannels[e.Guild.Id];
            else
                return;

            DiscordMessageBuilder message = new DiscordMessageBuilder();

            if (e.RoleBefore.Color.Value != e.RoleAfter.Color.Value)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🟣 Role Color Changed",
                    Description = e.RoleAfter.Mention,
                    Color = Bot.ColorSystem,
                };
                embedBuilder.AddField("Before", $"R {e.RoleBefore.Color.R}   G {e.RoleBefore.Color.G}   B {e.RoleBefore.Color.B}", true);
                embedBuilder.AddField("After", $"R {e.RoleAfter.Color.R}   G {e.RoleAfter.Color.G}   B {e.RoleAfter.Color.B}", true);
                message.AddEmbed(embedBuilder);
            }

            if (e.RoleBefore.IsHoisted != e.RoleAfter.IsHoisted)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🟡 Role Shown Above Others Changed",
                    Description = e.RoleAfter.Mention,
                    Color = Bot.ColorSystem,
                };
                embedBuilder.AddField("Before", e.RoleBefore.IsHoisted.ToString(), true);
                embedBuilder.AddField("After", e.RoleAfter.IsHoisted.ToString(), true);
                message.AddEmbed(embedBuilder);
            }

            if (e.RoleBefore.IsMentionable != e.RoleAfter.IsMentionable)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "⚪ Role Mentionable Changed",
                    Description = e.RoleAfter.Mention,
                    Color = Bot.ColorSystem,
                };
                embedBuilder.AddField("Before", e.RoleBefore.IsMentionable.ToString(), true);
                embedBuilder.AddField("After", e.RoleAfter.IsMentionable.ToString(), true);
                message.AddEmbed(embedBuilder);
            }

            if (e.RoleBefore.Name != e.RoleAfter.Name)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🟢 Role Name Changed",
                    Description = e.RoleAfter.Mention,
                    Color = Bot.ColorSystem,
                };
                embedBuilder.AddField("Before", e.RoleBefore.Name, true);
                embedBuilder.AddField("After", e.RoleAfter.Name, true);
                message.AddEmbed(embedBuilder);
            }

            /*if (e.RoleBefore.Position != e.RoleAfter.Position)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "Role Position Changed",
                    Description = e.RoleAfter.Mention,
                    Color = Bot.ColorSystem,
                };
                embedBuilder.AddField("Before", e.RoleBefore.Position.ToString(), true);
                embedBuilder.AddField("After", e.RoleAfter.Position.ToString(), true);
                message.AddEmbed(embedBuilder);
            }*/

            if (e.RoleBefore.Permissions != e.RoleAfter.Permissions)
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = "🟠 Role Permissions Changed",
                    Description = e.RoleAfter.Mention,
                    Color = Bot.ColorSystem,
                };
                embedBuilder.AddField("Before", e.RoleBefore.Permissions.ToPermissionString(), true);
                embedBuilder.AddField("After", e.RoleAfter.Permissions.ToPermissionString(), true);
                message.AddEmbed(embedBuilder);
            }

            if (message.Embeds.Count > 0)
                await channel.SendMessageAsync(message);
        }

        /*async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
        {
            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;
 0
            //Do sth.
            //This function is unnecessary in an audit log
        }*/

        async Task MessageDeleted(DiscordClient client, MessageDeleteEventArgs e)
        {
            try
            {
                if (e.Message == null)
                    return;

                DiscordChannel channel;
                if (messageLogChannels.ContainsKey(e.Guild.Id))
                    channel = messageLogChannels[e.Guild.Id];
                else
                    return;

                DiscordMessageBuilder message = new DiscordMessageBuilder();

                StringBuilder sb = new StringBuilder();

                if (e.Message.Author != null)
                {
                    sb.AppendLine("**Author:**");
                    sb.AppendLine(e.Message.Author.Mention);
                    sb.AppendLine($"ID: {e.Message.Author.Id}");
                    sb.AppendLine("");
                }

                if (e.Message.Channel != null)
                {
                    sb.AppendLine("**Channel:**");
                    sb.AppendLine(e.Message.Channel.Mention);
                    sb.AppendLine($"ID: {e.Message.Channel.Id}");
                }

                string preview = "";

                //Content
                if (!string.IsNullOrWhiteSpace(e.Message.Content))
                {
                    sb.AppendLine("");
                    sb.AppendLine("**Message content:**");
                    sb.AppendLine(e.Message.Content);
                }

                //Attachments
                if (e.Message.Attachments.Count > 0)
                {
                    sb.AppendLine("");

                    if (e.Message.Attachments.Count == 1)
                    {
                        sb.AppendLine("**Message attachment:**");
                        sb.AppendLine($"{e.Message.Attachments[0].Url}");

                        if (!string.IsNullOrWhiteSpace(e.Message.Attachments[0].MediaType))
                        {
                            string mime = e.Message.Attachments[0].MediaType.Split('/')[0];
                            if (mime == "image")
                                preview = e.Message.Attachments[0].Url;
                        }
                    }
                    else
                    {
                        sb.AppendLine("**Message attachments:**");
                        for (int i = 0; i < e.Message.Attachments.Count; i++)
                            sb.AppendLine($"{i + 1}. {e.Message.Attachments[i].Url}");
                    }
                }

                //Stickers
                if (e.Message.Stickers.Count > 0)
                {
                    sb.AppendLine("");
                    sb.AppendLine("**Sticker:**");
                    sb.AppendLine($"{e.Message.Stickers[0].Name} ({e.Message.Stickers[0].Id})");
                    sb.AppendLine(e.Message.Stickers[0].StickerUrl);
                    preview = e.Message.Stickers[0].StickerUrl;
                }

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = ":x: Message Deleted",
                    Description = sb.ToString(),
                    Color = Bot.ColorSystem,
                    ImageUrl = preview
                };

                message.AddEmbed(embedBuilder);

                if (e.Message.Embeds.Count > 0)
                {
                    embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = "Deleted Embeds: ",
                        Color = Bot.ColorSystem
                    };
                    message.AddEmbed(embedBuilder);
                    message.AddEmbeds(e.Message.Embeds);
                }

                await channel.SendMessageAsync(message);
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task MessageReactionAdded(DiscordClient client, MessageReactionAddEventArgs e)
        {
            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**User:**");
            sb.AppendLine(e.User.Mention);
            sb.AppendLine($"ID: {e.User.Id}");

            sb.AppendLine("");
            sb.AppendLine("**Channel:**");
            sb.AppendLine(e.Channel.Mention);
            sb.AppendLine($"ID: {e.Channel.Id}");

            sb.AppendLine($"");
            sb.AppendLine($"**Mesage: **");
            sb.AppendLine(e.Message.JumpLink.ToString());
            sb.AppendLine($"Emoji: {e.Emoji.Name}");

            string emojiUrl = "";

            //Fix for Unicode emojis not having URLs
            try
            {
                emojiUrl = e.Emoji.Url;
            }
            catch (System.Exception) {}

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = ":grey_exclamation: User Reacted To Message",
                Description = sb.ToString(),
                Thumbnail = string.IsNullOrEmpty(emojiUrl) ? null : new DiscordEmbedBuilder.EmbedThumbnail() { Url = emojiUrl, Height = 32, Width = 32 },
                Color = Bot.ColorSystem
            };

            await channel.SendMessageAsync(embedBuilder);
        }

        async Task MessageReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs e)
        {
            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**User:**");
            sb.AppendLine(e.User.Mention);
            sb.AppendLine($"ID: {e.User.Id}");
            sb.AppendLine("");
            sb.AppendLine("**Channel:**");
            sb.AppendLine(e.Channel.Mention);
            sb.AppendLine($"ID: {e.Channel.Id}");
            sb.AppendLine($"");
            sb.AppendLine($"**Mesage: **");
            sb.AppendLine(e.Message.JumpLink.ToString());
            sb.AppendLine($"Emoji: {e.Emoji.Name}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = ":grey_exclamation: User Removed Message Reaction",
                Description = sb.ToString(),
                Thumbnail = DiscordEmoji.IsValidUnicode(e.Emoji.Name) ? null : new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Emoji.Url, Height = 32, Width = 32 },
                Color = Bot.ColorSystem
            };

            await channel.SendMessageAsync(embedBuilder);
        }

        async Task MessageReactionRemovedEmoji(DiscordClient client, MessageReactionRemoveEmojiEventArgs e)
        {
            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**Channel:**");
            sb.AppendLine(e.Channel.Mention);
            sb.AppendLine($"ID: {e.Channel.Id}");
            sb.AppendLine($"");
            sb.AppendLine($"**Mesage: **");
            sb.AppendLine(e.Message.JumpLink.ToString());
            sb.AppendLine($"Emoji: {e.Emoji.Name}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = ":exclamation: Message Reaction Emoji Removed",
                Description = sb.ToString(),
                Thumbnail = DiscordEmoji.IsValidUnicode(e.Emoji.Name) ? null : new DiscordEmbedBuilder.EmbedThumbnail() { Url = e.Emoji.Url, Height = 32, Width = 32 },
                Color = Bot.ColorSystem
            };

            await channel.SendMessageAsync(embedBuilder);
        }

        async Task MessageReactionsCleared(DiscordClient client, MessageReactionsClearEventArgs e)
        {
            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**Channel:**");
            sb.AppendLine(e.Channel.Mention);
            sb.AppendLine($"ID: {e.Channel.Id}");
            sb.AppendLine($"");
            sb.AppendLine($"**Mesage: **");
            sb.AppendLine(e.Message.JumpLink.ToString());

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = ":exclamation: Message Reactions Cleared",
                Description = sb.ToString(),
                Color = Bot.ColorSystem
            };

            await channel.SendMessageAsync(embedBuilder);
        }

        async Task MessagesBulkDeleted(DiscordClient client, MessageBulkDeleteEventArgs e)
        {
            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Channel: {e.Channel.Mention}");
            sb.AppendLine($"Message Count: {e.Messages.Count}");

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = ":x: Messages Bulk Deleted",
                Description = sb.ToString(),
                Color = Bot.ColorSystem
            };
            await channel.SendMessageAsync(embedBuilder);
        }

        async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
        {
            if (e.MessageBefore.Content == e.Message.Content && e.Message.Embeds.Except(e.MessageBefore.Embeds).Count() == 0 || e.Message.Author.IsBot)
                return;

            DiscordChannel channel;
            if (messageLogChannels.ContainsKey(e.Guild.Id))
                channel = messageLogChannels[e.Guild.Id];
            else
                return;

            DiscordMessageBuilder message = new DiscordMessageBuilder();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("**Author:**");
            sb.AppendLine(e.Message.Author.Mention);
            sb.AppendLine($"ID: {e.Message.Author.Id}");

            sb.AppendLine("");
            sb.AppendLine("**Message:**");
            sb.AppendLine(e.Message.JumpLink.ToString());

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
            {
                Title = ":memo: Message Edited",
                Description = sb.ToString(),
                Color = Bot.ColorSystem,
            };

            if (!string.IsNullOrWhiteSpace(e.Message.Content) && string.Compare(e.MessageBefore.Content, e.Message.Content) != 0)
            {
                embedBuilder.AddField("Before", e.MessageBefore.Content, true);
                embedBuilder.AddField("After", e.Message.Content, true);
            }
            message.AddEmbed(embedBuilder);

            if (e.Message.Embeds.Count > 0)
            {
                if (e.MessageBefore.Embeds.Count > 0)
                {
                    DiscordEmbedBuilder embedBuilder_before = new DiscordEmbedBuilder
                    {
                        Title = "Before:",
                        Color = Bot.ColorSystem
                    };
                    message.AddEmbed(embedBuilder_before);
                    message.AddEmbeds(e.MessageBefore.Embeds);

                    DiscordEmbedBuilder embedBuilder_after = new DiscordEmbedBuilder
                    {
                        Title = "After:",
                        Color = Bot.ColorSystem
                    };
                    message.AddEmbed(embedBuilder_after);
                    message.AddEmbeds(e.Message.Embeds);
                }
                else
                {
                    DiscordEmbedBuilder embedBuilder_after = new DiscordEmbedBuilder
                    {
                        Title = "Added Embeds:",
                        Color = Bot.ColorSystem
                    };
                    message.AddEmbed(embedBuilder_after);
                    message.AddEmbeds(e.Message.Embeds);
                }
            }

            await channel.SendMessageAsync(message);
        }

        async Task VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            DiscordChannel channel;
            if (vcLogChannels.ContainsKey(e.Guild.Id))
                channel = vcLogChannels[e.Guild.Id];
            else
                return;

            DiscordMessageBuilder message = new DiscordMessageBuilder();

            //Joined Voice Channel
            if (e.Before == null && e.After != null && e.After.Channel != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(e.User.Mention);
                sb.AppendLine($"User ID: {e.User.Id}");
                sb.AppendLine("");
                sb.AppendLine(e.After.Channel.Mention);
                sb.AppendLine($"Channel ID: {e.After.Channel.Id}");

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = ":loud_sound: User Joined Voice Channel",
                    Description = sb.ToString(),
                    Color = Bot.ColorSystem
                };
                message.AddEmbed(embedBuilder);
            }

            if (e.Before != null && e.After != null)
            {
                //Voice Channel Changed
                if (e.Before.Channel != e.After.Channel)
                {
                    if (e.Before.Channel != null && e.After.Channel == null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");
                        sb.AppendLine("");
                        sb.AppendLine(e.Before.Channel.Mention);
                        sb.AppendLine($"Channel ID: {e.Before.Channel.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: User Left Voice Channel",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else if (e.Before.Channel != null && e.After.Channel != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        StringBuilder sb_from = new StringBuilder();
                        sb_from.AppendLine(e.Before.Channel.Mention);
                        sb_from.AppendLine($"Channel ID: {e.Before.Channel.Id}");

                        StringBuilder sb_to = new StringBuilder();
                        sb_to.AppendLine(e.After.Channel.Mention);
                        sb_to.AppendLine($"Channel ID: {e.After.Channel.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: User Moved Voice Channel",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        embedBuilder.AddField("From", sb_from.ToString(), true);
                        embedBuilder.AddField("To", sb_to.ToString(), true);
                        message.AddEmbed(embedBuilder);
                    }


                }

                //Deafened
                if (e.Before.IsSelfDeafened != e.After.IsSelfDeafened)
                {
                    if (e.Before.IsSelfDeafened)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :x: User Self Undeafened",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :x: User Self Deafened",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                }

                //Muted
                if (e.Before.IsSelfMuted != e.After.IsSelfMuted)
                {
                    if (e.Before.IsSelfMuted)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":mute: User Self Unmuted",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":mute: User Self Muted",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                }

                //Muted
                if (e.Before.IsServerMuted != e.After.IsServerMuted)
                {
                    if (e.Before.IsServerMuted)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":mute: User Server Unmuted",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":mute: User Server Muted",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                }

                //Deafened
                if (e.Before.IsServerDeafened != e.After.IsServerDeafened)
                {
                    if (e.Before.IsServerDeafened)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :x: User Server Undeafened",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :x: User Server Deafened",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                }

                //Stream
                if (e.Before.IsSelfStream != e.After.IsSelfStream)
                {
                    if (e.Before.IsSelfStream)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :desktop: User Stopped Streaming",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :desktop: User Started Streaming",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                }

                //Video
                if (e.Before.IsSelfVideo != e.After.IsSelfVideo)
                {
                    if (e.Before.IsSelfVideo)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :video_camera: User Disabled Camera",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(e.User.Mention);
                        sb.AppendLine($"User ID: {e.User.Id}");

                        DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                        {
                            Title = ":loud_sound: :video_camera: User Enabled Camera",
                            Description = sb.ToString(),
                            Color = Bot.ColorSystem
                        };
                        message.AddEmbed(embedBuilder);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(message.Content) || message.Files.Count > 0 || message.Sticker != null || message.Embeds.Count > 0)
                await channel.SendMessageAsync(message);
        }

        /*async Task PresenceUpdated(DiscordClient client, PresenceUpdateEventArgs e)
        {
            if (e.PresenceBefore != null && e.PresenceAfter != null)
            {
                DiscordMessageBuilder message = new DiscordMessageBuilder();

                if (e.PresenceBefore.Status != e.PresenceAfter.Status)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(e.User.Mention);
                    sb.AppendLine($"ID: {e.User.Id}");
                    sb.AppendLine($"UTC Timestamp: {System.DateTime.UtcNow.ToString()}");

                    DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                    {
                        Title = ":blue_circle: User Status Changed",
                        Description = sb.ToString(),
                        Color = Bot.ColorSystem,
                    };
                    embedBuilder.AddField("Before", GetUserStatusString(e.PresenceBefore.Status), true);
                    embedBuilder.AddField("After", GetUserStatusString(e.PresenceAfter.Status), true);
                    message.AddEmbed(embedBuilder);
                }

                if (message.Embeds.Count > 0)
                    foreach (var channel in presenceLogChannels.Values)
                        if (channel != null && channel.Guild.Members.ContainsKey(e.User.Id))
                            await channel.SendMessageAsync(message);
            }
        }

        string GetUserStatusString(UserStatus status)
        {
            StringBuilder sb = new StringBuilder();
            switch (status)
            {
                case UserStatus.Offline:
                    sb.Append(":black_circle: ");
                    break;
                case UserStatus.Invisible:
                    sb.Append(":white_circle: ");
                    break;
                case UserStatus.DoNotDisturb:
                    sb.Append(":red_circle: ");
                    break;
                case UserStatus.Idle:
                    sb.Append(":yellow_circle: ");
                    break;
                case UserStatus.Online:
                    sb.Append(":green_circle: ");
                    break;
            }
            sb.Append(status.ToString());
            return sb.ToString();
        }*/

        public DiscordChannel GetAdminLogChannel(ulong guildID)
        {
            if (adminLogChannels.ContainsKey(guildID))
                return adminLogChannels[guildID];
            return null;
        }

        public DiscordChannel GetMessageLogChannel(ulong guildID)
        {
            if (messageLogChannels.ContainsKey(guildID))
                return messageLogChannels[guildID];
            return null;
        }
    }

    class UserAvatars
    {
        public string mainAvatar;
        public Dictionary<ulong, string> guildAvatars = new Dictionary<ulong, string>();
    }
}