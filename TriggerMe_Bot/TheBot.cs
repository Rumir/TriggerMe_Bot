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
            try
            {
                // The bot should never respond to itself.
                if (message.Author.Id == _client.CurrentUser.Id)
                    return;

                await CommandTrigger(message);
            }
            catch (Exception e)
            {
                await message.Channel.SendMessageAsync("Something went wrong horribly. Please contact a developer: https://github.com/Rumir/TriggerMe_Bot/issues");
            }
            
        }

        private async Task CommandTrigger(SocketMessage message)
        {
            SocketGuildChannel chnl = message.Channel as SocketGuildChannel;
            string parameters;
            switch (message.Content.ToLower())
            {
                case "trigger me":
                    await message.Channel.SendMessageAsync("Hello, I am Trigger Me BOT, your friendly helper to not say certain words.\nIf you need help" +
                        ", you can just write \"Trigger Help!\" to trigger me and I will write a helpful help message in this channel");
                    break;

                case "trigger help":
                    await message.Channel.SendMessageAsync("***Commands***\n" +
                        "• Trigger Me - Shows a helpful introduction to me\n" +
                        "• Trigger Help - Shows this helpful help message\n" +
                        "• Trigger List - Messages you privately the full trigger list. You need to allow me to message you or I can't help you with this\n" + 
                        "• Trigger List2 - Shows the full trigger list in this channel\n" + 
                        "***Admin Commands (need to be at least moderator)***\n\n" +
                        "• Trigger Add <Word or Words> <Mode>! - Adds one word or a sequence of words to the Trigger List. The Mode parameter accepts \"normal\"" + 
                        ", \"characters\" where...\n" +
                        "•• normal - no special characters for letters\n" + //TODO
                        "•• leet - included special characters for letters like \"Tr!gg3r\" (1337 5p34k)\n" + //TODO
                        "• Trigger Info <Word or Words> - Shows infos about the set trigger\n" + //TODO
                        "• Trigger Remove <Word or Words> - Removes a trigger\n" + //TODO
                        "• Trigger AddMod <Discord Tag> - Adds a moderator **(OWNER ONLY)**\n" + //TODO
                        "• Trigger ListMod - Lists all moderators **(OWNER ONLY)**\n" + //TODO
                        "• Trigger RemoveMod <id> - Removes a moderator **(OWNER ONLY)**\n" + //TODO
                        "• Trigger Permissions - Shows all the needed permissions in a pretty list\n\n" +
                        "***Notice***\n" +
                        "• All commands are case-insensitive. So you can write everything capitalized, or the opposite, or just a mix of both.\n" +
                        "• I am still under construction. Please be patient with me when I crash. My programmer will fix the problems as fast as possible."); 
                    break;

                case "trigger list2":
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
                            output += "• " + localstring;
                        }
                        output = output.Remove(output.Length - 2);

                        if (output.Length >= 1984)
                            await message.Channel.SendMessageAsync("Output string too long. Please contact a developer");
                        else
                            await message.Channel.SendMessageAsync("***Triggers***\n" + output);
                    }
                    break;

                case "trigger list":
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
                            output += "• " + localstring;
                        }
                        output = output.Remove(output.Length - 2);

                        if (output.Length >= 1984)
                            await message.Author.SendMessageAsync("Output string too long. Please contact a developer");
                        else
                            await message.Author.SendMessageAsync("***Triggers***\n" + output);
                    }
                    break;

                case "trigger permissions":
                    await message.Channel.SendMessageAsync("***Permissions Needed***\n" +
                        "• View Channels\n" +
                        "• Send Messages\n" +
                        "• Embed Links\n" +
                        "• Attach Files\n" +
                        "• Manage Messages\n" +
                        "• Read Message History\n" +
                        "• Use External Emojis\n" +
                        "• Add Reactions\n");
                    break;
            }

            if (message.Content.ToLower().StartsWith("trigger add "))
            {
                if (!isMod(chnl, message))
                {
                    await message.Channel.SendMessageAsync("Sorry, but you are not a moderator");
                    return;
                }
                parameters = message.Content.ToLower().Remove(0, 12); //remove "trigger add"
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1); //remove unneccessary spaces

                string trigger_word = "";
                foreach (string item in parameters.Split(' '))
                    if (item.ToLower() != "normal" && item.ToLower() != "leet")
                        trigger_word += item.ToLower() + " ";
                trigger_word = trigger_word.Remove(trigger_word.Length - 1);

                ArrayList sqlresult = sqlClient.PutCommand($"SELECT trigger_word FROM triggers WHERE server_id = {chnl.Guild.Id} AND trigger_word = '{trigger_word}';");
                if (sqlresult.Count != 0)
                {
                    await message.Channel.SendMessageAsync("Trigger already existing");
                    return;
                }
                else
                {
                    string setting = parameters.Split(' ')[parameters.Split(' ').Length - 1].ToLower();
                    if (setting != "normal" && setting != "leet")
                    {
                        await message.Channel.SendMessageAsync("No setting was given. Using \"normal\" as setting");
                        setting = "normal";
                    }
                    else if (setting == "leet")
                    {
                        await message.Channel.SendMessageAsync("Setting \"leet\" not implemented yet. Using \"normal\"!"); //I'm lazy, okay?
                        setting = "normal";
                    }

                    sqlClient.PutCommand($"INSERT INTO triggers (server_id, trigger_word, trigger_setting, added_by) VALUES (" +
                        $"{chnl.Guild.Id}," +
                        $"'{trigger_word}'," +
                        $"'{setting}'," +
                        $"{message.Author.Id});");

                    await message.Channel.SendMessageAsync("Trigger added!");
                }

            }

            else if (message.Content.ToLower().StartsWith("trigger info "))
            {
                parameters = message.Content.ToLower().Remove(0, 13);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);
                await message.Channel.SendMessageAsync("Not implemented yet");
            }

            else if (message.Content.ToLower().StartsWith("trigger remove "))
            {
                parameters = message.Content.ToLower().Remove(0, 15);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);
                await message.Channel.SendMessageAsync("Not implemented yet");
            }

            else if (message.Content.ToLower().StartsWith("trigger addmod "))
            {
                parameters = message.Content.ToLower().Remove(0, 15);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);
                if (parameters.Length == 0)
                {
                    await message.Channel.SendMessageAsync("Usage: Trigger AddMod <Discord Tag>!");
                    return;
                }

                await message.Channel.SendMessageAsync("Not implemented yet");
            }

            else if (message.Content.ToLower().StartsWith("trigger listmod "))
            {
                parameters = message.Content.ToLower().Remove(0, 16);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);
                await message.Channel.SendMessageAsync("Not implemented yet");
            }

            else if (message.Content.ToLower().StartsWith("trigger removemod "))
            {
                parameters = message.Content.ToLower().Remove(0, 18);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);
                await message.Channel.SendMessageAsync("Not implemented yet");
            }

            /////////////////////

            else if (message.Content.ToLower().StartsWith("trigger add")
                || message.Content.ToLower().StartsWith("trigger info")
                || message.Content.ToLower().StartsWith("trigger remove")
                || message.Content.ToLower().StartsWith("trigger addmod")
                || message.Content.ToLower().StartsWith("trigger listmod")
                || message.Content.ToLower().StartsWith("trigger removemod"))
                ;

            
        }

        private bool isMod(SocketGuildChannel chnl, SocketMessage message)
        {
            ArrayList result = sqlClient.PutCommand($"SELECT client_id FROM moderators WHERE server_id = {chnl.Guild.Id} AND client_id = '{message.Author.Id}';");
            if (result.Count == 0)
                return false;
            return true;
        }
    }
}
