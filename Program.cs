namespace SwippyBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // https://discord.com/api/oauth2/authorize?client_id=936760943499706428&scope=bot&permissions=8
            Bot bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}
