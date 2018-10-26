using System;
using System.Collections;
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
        SqlClient sqlClient;
        public TheBot()
        {
            sqlClient = new SqlClient();
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
            SocketGuildChannel chnl = message.Channel as SocketGuildChannel;
            switch (message.Content.ToLower())
            {
                case "trigger me!":
                    await message.Channel.SendMessageAsync("Hello, I am Trigger Me BOT, your friendly helper to not say certain words.\nIf you need help" +
                        ", you can just write \"Trigger Help!\" to trigger me and I will write a helpful help message in this channel");
                    break;

                case "trigger help!":
                    await message.Channel.SendMessageAsync("***Commands***\n" +
                        "• Trigger Me! - Shows a helpful introduction to me\n" +
                        "• Trigger Help! - Shows this helpful help message\n" +
                        "• Trigger List! - Messages you privately the full trigger list. You need to allow me to message you or I can't help you with this\n" + 
                        "• Trigger List2! - Shows the full trigger list in this channel\n" + 
                        "• Trigger Permissions! - Shows all the needed permissions in a pretty list\n\n" + //TODO
                        "***Admin Commands (need to be at least moderator)***\n" +
                        "• Trigger Add <Word or Words> <Mode>! - Adds one word or a sequence of words to the Trigger List. The Mode parameter accepts \"normal\"" + //TODO
                        ", \"characters\" where...\n" +
                        "•• normal - no special characters for letters\n" + //TODO
                        "•• characters - included special characters for letters like \"Tr!gg3r\" (1337 5p34k)\n" + //TODO
                        "• Trigger Info <Word or Words>! - Shows infos about the set trigger\n" + //TODO
                        "• Trigger Remove <Word or Words>! - Removes a trigger\n" + //TODO
                        "• Trigger AddMod <Discord Tag>! - Adds a moderator **(OWNER ONLY)**\n" + //TODO
                        "• Trigger ListMod! - Lists all moderators **(OWNER ONLY)**\n" + //TODO
                        "• Trigger RemoveMod <id>! - Removes a moderator **(OWNER ONLY)**\n\n" + //TODO
                        "***Notice***\n" +
                        "• All commands are case-insensitive. So you can write everything capitalized, or the opposite, or just a mix of both.\n" +
                        "• I am still under construction. Please be patient with me when I crash. My programmer will fix the problems as fast as possible."); 
                    break;

                case "trigger list2!":
                    ArrayList list = sqlClient.PutCommand($"SELECT trigger_word FROM triggers WHERE server_id = {chnl.Guild.Id};");
                    if (list.Count == 0)
                        await message.Channel.SendMessageAsync("There are no triggers right now..");
                    else
                    {
                        string output = "";
                        list.Sort();
                        foreach (string item in list)
                        {
                            string localstring = item;
                            localstring = localstring.Remove(localstring.Length - 1);
                            localstring = localstring.Insert(localstring.Length, ",\n");
                            output += localstring;
                        }
                        output = output.Remove(output.Length - 2);

                        if (output.Length >= 1984)
                            await message.Channel.SendMessageAsync("Output string too long. Please contact a developer");
                        else
                            await message.Channel.SendMessageAsync("***Triggers***\n\n" + output);
                    }
                    break;

                case "trigger list!":
                    ArrayList listt = sqlClient.PutCommand($"SELECT trigger_word FROM triggers WHERE server_id = {chnl.Guild.Id};");
                    if (listt.Count == 0)
                        await message.Channel.SendMessageAsync("There are no triggers right now..");
                    else
                    {
                        string output = "";
                        listt.Sort();
                        foreach (string item in listt)
                        {
                            string localstring = item;
                            localstring = localstring.Remove(localstring.Length - 1);
                            localstring = localstring.Insert(localstring.Length, ",\n");
                            output += localstring;
                        }
                        output = output.Remove(output.Length - 2);

                        if (output.Length >= 1984)
                            await message.Author.SendMessageAsync("Output string too long. Please contact a developer");
                        else
                            await message.Author.SendMessageAsync("***Triggers***\n\n" + output);
                    }
                    break;

                case "trigger permissions!":
                    await message.Channel.SendMessageAsync("Not implemented yet");
                    break;
            }
            if(message.Content.ToLower().Contains("trigger add")
                || message.Content.ToLower().Contains("trigger info")
                || message.Content.ToLower().Contains("trigger remove")
                || message.Content.ToLower().Contains("trigger addmod")
                || message.Content.ToLower().Contains("trigger listmod")
                || message.Content.ToLower().Contains("trigger removemod"))
            {
                await message.Channel.SendMessageAsync("Not implemented yet");
            }
            
        }
    }
}
