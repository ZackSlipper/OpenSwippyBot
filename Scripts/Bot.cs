using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using System.Linq;
using Genius;
using System.Net.Http;
using Newtonsoft.Json;

namespace SwippyBot
{
    public class Bot
    {
        static Bot current;
        BotConfig botConfig;

        Dictionary<string, List<Command>> moduleCommands;
        public static Dictionary<string, List<Command>> ModuleCommands { get => current.moduleCommands; }

        CommandsNextExtension commands;
        public static CommandsNextExtension Commands { get => current.commands; }

        public DiscordClient Client { get; private set; }
        public LavalinkExtension Lavalink { get; private set; }

        InteractivityExtension interactivity;
        public static InteractivityExtension Interactivity { get => current.interactivity; }

        GlobalData globalData;
        public static GlobalData GlobalData { get => current.globalData; }

        Random random;
        public static Random Random { get => current.random; }

        WordChain wordChain;
        public static WordChain WordChain { get => current.wordChain; }

        Counter counter;
        public static Counter Counter { get => current.counter; }

        AuditLog auditLog;
        public static AuditLog AuditLog { get => current.auditLog; }

        RoleReaction roleReaction;
        public static RoleReaction RoleReaction { get => current.roleReaction; }

        NewJoinerRoles newJoinerRoles;
        public static NewJoinerRoles NewJoinerRoles { get => current.newJoinerRoles; }

        Birthdays birthdays;
        public static Birthdays Birthdays { get => current.birthdays; }

        InviteController inviteController;
        public static InviteController InviteController { get => current.inviteController; }

        UserSlowMode userSlowMode;
        public static UserSlowMode UserSlowMode { get => current.userSlowMode; }

        ContentMonitor contentMonitor;
        public static ContentMonitor ContentMonitor { get => current.contentMonitor; }

        ContentFilter contentFilter;
        public static ContentFilter ContentFilter { get => current.contentFilter; }

        ThreadEnforcement threadEnforcement;
        public static ThreadEnforcement ThreadEnforcement { get => current.threadEnforcement; }

        WarningSystem warningSystem;
        public static WarningSystem WarningSystem { get => current.warningSystem; }

        Staff staff;
        public static Staff Staff { get => current.staff; }

        MassPingProtection massPingProtection;
        public static MassPingProtection MassPingProtection { get => current.massPingProtection; }

        HelloAndGoodbye helloAndGoodbye;
        public static HelloAndGoodbye HelloAndGoodbye { get => current.helloAndGoodbye; }

        Player player;

        Dictionary<ulong, ServerData> serverData = new Dictionary<ulong, ServerData>();
        Dictionary<ulong, ServerValues> serverValues = new Dictionary<ulong, ServerValues>();

        Dictionary<LavalinkGuildConnection, ulong> lavalinkConnectionServerID = new Dictionary<LavalinkGuildConnection, ulong>();
        public static Dictionary<LavalinkGuildConnection, ulong> LavalinkConnectionServerID { get => current.lavalinkConnectionServerID; }

        public static string Prefix { get => current.botConfig.Prefix; }

        ulong systemGuildID;
        DiscordChannel systemChannel;

        //Colors
        public static DiscordColor ColorMain { get; } = new DiscordColor(95, 56, 204);
        public static DiscordColor ColorSecondary { get; } = new DiscordColor(117, 84, 184);
        public static DiscordColor ColorSystem { get; } = new DiscordColor(108, 221, 210); //new DiscordColor(79, 47, 171); <---- old color
        public static DiscordColor ColorWarning { get; } = DiscordColor.Yellow;
        public static DiscordColor ColorError { get; } = DiscordColor.Red;

        //Other
        public static ulong ID { get => 936760943499706428; } //SwippyBot's user ID

        public static Dictionary<string, string> ModuleNameMap { get; } = new Dictionary<string, string>()
        {
            {"BotCommands", "Bot"},
            {"CounterCommands", "Counter"},
            {"UserCommands", "User"},
            {"WordChainCommands", "Word Chain"},
            {"PlayerCommands", "Player"},
            {"AuditLogCommands", "Audit Log"},
            {"RoleReactionCommands", "Role Reaction"},
            {"RoleManagementCommands", "Role Management"},
            {"BirthdayCommands", "Birthday"},
            {"InviteControllerCommands", "Invite Controller"},
            {"UserSlowModeCommands", "User Slow Mode"},
            {"HelloAndGoodbyeCommands", "Hello And Goodbye"},
            {"ContentFilterCommands", "Content Filter"},
            {"ThreadEnforcementCommands", "Thread Enforcement"},
            {"WarningSystemCommands", "Warning System"},
            {"StaffCommands", "Staff"},
            {"MassPingProtectionCommands", "Mass Ping Protection"},
        };

        HttpClient httpClient;
        public static HttpClient HttpClient { get => current.httpClient; }

        GeniusClient geniusClient;
        public static GeniusClient GeniusClient { get => current.geniusClient; }

        public async Task RunAsync()
        {
            if (current != null)
                return;
            current = this;

            //Client Config
            botConfig = LoadJsonObject<BotConfig>("./config.json");

            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = botConfig.DiscordToken,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            };

            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.GuildAvailable += OnGuildAvailble;
            Client.GuildCreated += GuildCreated;

            //Interactivity
            interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            });

            //Commands
            CommandsNextConfiguration commandConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { botConfig.Prefix },
                EnableMentionPrefix = true,
                EnableDms = false,
                DmHelp = false,
                IgnoreExtraArguments = false,
            };

            commands = Client.UseCommandsNext(commandConfig);

            //HTTP Client
            httpClient = new HttpClient();

            //Genius Client
            geniusClient = new GeniusClient(botConfig.GeniusToken);

            //Unregister Help
            Commands.UnregisterCommands(new Command[] { Commands.RegisteredCommands.Values.Where(x => x.Name == "help").First() });

            //Commands.RegisterCommands<TestCommands>();
            Commands.RegisterCommands<BotCommands>();
            Commands.RegisterCommands<CounterCommands>();
            Commands.RegisterCommands<UserCommands>();
            Commands.RegisterCommands<WordChainCommands>();
            Commands.RegisterCommands<PlayerCommands>();
            Commands.RegisterCommands<AuditLogCommands>();
            Commands.RegisterCommands<RoleReactionCommands>();
            Commands.RegisterCommands<RoleManagementCommands>();
            Commands.RegisterCommands<BirthdayCommands>();
            Commands.RegisterCommands<InviteControllerCommands>();
            Commands.RegisterCommands<UserSlowModeCommands>();
            Commands.RegisterCommands<HelloAndGoodbyeCommands>();
            Commands.RegisterCommands<ContentFilterCommands>();
            Commands.RegisterCommands<ThreadEnforcementCommands>();
            Commands.RegisterCommands<WarningSystemCommands>();
            Commands.RegisterCommands<StaffCommands>();
            Commands.RegisterCommands<MassPingProtectionCommands>();

            //Build command list
            moduleCommands = new Dictionary<string, List<Command>>();
            foreach (Command command in Commands.RegisteredCommands.Values)
            {
                if (!moduleCommands.ContainsKey(command.Module.ModuleType.Name))
                    moduleCommands.Add(command.Module.ModuleType.Name, new List<Command>());

                if (!moduleCommands[command.Module.ModuleType.Name].Contains(command))
                    moduleCommands[command.Module.ModuleType.Name].Add(command);
            }

            //Lavalink
            ConnectionEndpoint endpoint = new ConnectionEndpoint()
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration()
            {
                Password = "swippybwotishbwab",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            Lavalink = Client.UseLavalink();

            //Connect
            await Client.ConnectAsync();
            await Lavalink.ConnectAsync(lavalinkConfig);

            //Global data
            globalData = new GlobalData();

            //Get bot channel
            ulong.TryParse(globalData.GetValue("bot_log_server"), out systemGuildID);

            if (systemGuildID == 0)
                ReportError(new Exception("Bot log channel isn't set"));

            //Bot jobs
            random = new Random();
            wordChain = new WordChain(this);
            counter = new Counter(this);
            player = new Player(this);
            auditLog = new AuditLog(this);
            roleReaction = new RoleReaction(this);
            newJoinerRoles = new NewJoinerRoles(this);
            birthdays = new Birthdays(this);
            inviteController = new InviteController(this);
            userSlowMode = new UserSlowMode(this);
            helloAndGoodbye = new HelloAndGoodbye(this);
            contentMonitor = new ContentMonitor(this);
            contentFilter = new ContentFilter(this);
            threadEnforcement = new ThreadEnforcement(this);
            warningSystem = new WarningSystem(this);
            staff = new Staff(this);
            massPingProtection = new MassPingProtection(this);

            await Task.Delay(-1);
        }

        Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        async Task OnGuildAvailble(DiscordClient client, GuildCreateEventArgs e)
        {
            await GuildJoinedOrLoaded(e);
        }

        async Task GuildCreated(DiscordClient client, GuildCreateEventArgs e)
        {
            await GuildJoinedOrLoaded(e);
        }

        async Task GuildJoinedOrLoaded(GuildCreateEventArgs e)
        {
            if (e.Guild.Id == systemGuildID)
            {
                ulong channelID;
                ulong.TryParse(globalData.GetValue("bot_log_channel"), out channelID);

                DiscordGuild guild = await Client.GetGuildAsync(e.Guild.Id);
                if (guild != null)
                {
                    systemChannel = guild.GetChannel(channelID);
                    await SystemMessage("Swippy bot is up and running ^-^", "");
                }
                else
                    ReportError(new Exception("Bot log channel isn't set"));
            }

            if (!serverData.ContainsKey(e.Guild.Id))
                serverData.Add(e.Guild.Id, new ServerData(e.Guild.Id));

            if (!serverValues.ContainsKey(e.Guild.Id))
                serverValues.Add(e.Guild.Id, new ServerValues(e.Guild));

            counter.AddServerData(e.Guild);
            wordChain.AddServerData(e.Guild);
            auditLog.UpdateServerChannels(e.Guild);
            auditLog.LoadUserAvatars(e.Guild);
            roleReaction.LoadServerMessages(e.Guild);
            newJoinerRoles.LoadServerRoles(e.Guild);
            birthdays.UpdateChannelRolePair(e.Guild);
            contentFilter.LoadIgnoredChannels(e.Guild);
        }

        public static bool HasAllRoles(CommandContext ctx, params string[] serverValues)
        {
            foreach (string serverValue in serverValues)
            {
                if (!ctx.Member.Roles.Any(x => x.Id == serverValue.ULong()))
                    return false;
            }
            return true;
        }

        public static ServerData GetSeverData(ulong guildId)
        {
            return current.serverData[guildId];
        }

        public static ServerValues GetValues(ulong guildId)
        {
            return current.serverValues[guildId];
        }

        public static ServerValues[] GetAllValues()
        {
            return current.serverValues.Values.ToArray();
        }

        public static void ReportError(Exception ex)
        {
            if (Bot.current.systemChannel == null)
                Console.WriteLine($"Bot Error: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}{ex.InnerException}");
            else
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"Bot Error: {ex.Message}",
                    Description = $"{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}{ex.InnerException}",
                    Color = ColorError
                };

                Bot.current.systemChannel.SendMessageAsync(embedBuilder).GetAwaiter().GetResult();
            }
        }

        public static async Task<DiscordMessage> SystemMessage(string title, string description)
        {
            if (Bot.current.systemChannel == null)
                Console.WriteLine($"{title}{Environment.NewLine}{description}");
            else
            {
                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder
                {
                    Title = $"{title}",
                    Description = description,
                    Color = ColorSystem
                };
                return await Bot.current.systemChannel.SendMessageAsync(embedBuilder);
            }
            return null;
        }

        public static T LoadJsonObject<T>(string filePath)
        {
            try
            {
                using (StreamReader stream = new StreamReader(filePath))
                {
                    string jsonText = stream.ReadToEnd();
                    return JsonConvert.DeserializeObject<T>(jsonText);
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }

            return default(T);
        }
    }
}