using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace TriggerMe_Bot
{
    class TheBot
    {
        //The few first lines were based on an example at
        //https://github.com/RogueException/Discord.Net/tree/dev/samples/01_basic_ping_bot
        //Props to you, guys!

        DiscordSocketClient _client;

        public TheBot()
        {
            
        }

        public async Task MainAsync()
        {
            

            _client = new DiscordSocketClient();

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;

            await _client.LoginAsync(TokenType.Bot, Properties.Resources.token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _client.CurrentUser.Id)
                return;

            await CommandTrigger(message);
        }

        private async Task CommandTrigger(SocketMessage message)
        {
            switch (message.Content.ToLower())
            {
                case "trigger me!":
                    await message.Channel.SendMessageAsync("Hello, I am Trigger Me BOT, your friendly helper to not say certain words.\nIf you need help" +
                        ", you can just write \"Trigger Help!\" to trigger me and write a helpful help message in this channel");
                    break;
                case "trigger help!":
                    await message.Channel.SendMessageAsync("**Commands**\n" +
                        "• Trigger Me! - Shows a helpful introduction to me\n" +
                        "• Trigger Help! - Shows this helpful help message\n" +
                        "• Trigger List! - Messages you privately the full trigger list. You need to allow me to message you or I can't help you with this\n" +
                        "• Trigger List2! - Shows the full trigger list in this channel\n" +
                        "• Trigger Permissions! - Shows ");
            }
            
        }
    }
}
