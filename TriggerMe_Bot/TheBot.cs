using System;
using System.Collections;
using System.Collections.Generic;
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
        ArrayList triggerWords,moderators;
        bool init;
        public TheBot()
        {
            sqlClient = new SqlClient();
            triggerWords = new ArrayList();
            moderators = new ArrayList();
            init = false;
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
            if (!init)
            {
                await InitialiseTriggers(message);
                await InitMods(message);
            }
            try
            {
                // The bot should never respond to itself.
                if (message.Author.Id == _client.CurrentUser.Id)
                    return;

                await CommandTrigger(message);
            }
            catch (Exception e)
            {
                await message.Channel.SendMessageAsync("Something went wrong horribly. Please contact a developer: https://github.com/Rumir/TriggerMe_Bot/issues \n" +
                    "Error Message: " + e.Message);
            }
            
        }

        private Task InitialiseTriggers(SocketMessage pMessage)
        {
            init = true;
            SocketGuildChannel chnl = pMessage.Channel as SocketGuildChannel;
            triggerWords = sqlClient.PutCommand($"SELECT trigger_word,trigger_setting FROM triggers WHERE server_id = {chnl.Guild.Id};");
            triggerWords.Sort();
            return Task.CompletedTask;
        }

        private Task InitMods(SocketMessage pMessage)
        {
            SocketGuildChannel chnl = pMessage.Channel as SocketGuildChannel;
            moderators = sqlClient.PutCommand($"SELECT client_id FROM moderators WHERE server_id = {chnl.Guild.Id}");
            for (int i = 0; i < moderators.Count; i++)
                moderators[i] = moderators[i].ToString().Split(';')[0];
            return Task.CompletedTask;
        }

        private async Task CommandTrigger(SocketMessage message)
        {
            SocketGuildChannel chnl = message.Channel as SocketGuildChannel;
            string parameters;
            switch (message.Content.ToLower())
            {
                /**
                 * 
                 **/
                case "trigger me":
                    await message.Channel.SendMessageAsync("Hello, I am Trigger Me BOT, your friendly helper to not say certain words.\nIf you need help" +
                        ", you can just write \"Trigger Help\" to trigger me and I will write a helpful help message in this channel");
                    break;
                /**
                 * 
                 **/
                case "trigger help":
                    await message.Channel.SendMessageAsync("***Commands***\n" +
                        "• Trigger Me - Shows a helpful introduction to me\n" +
                        "• Trigger Help - Shows this helpful help message\n" +
                        "• Trigger List - Messages you privately the full trigger list. You need to allow me to message you or I can't help you with this\n" + 
                        "• Trigger List2 - Shows the full trigger list in this channel\n\n" + 
                        "***Admin Commands (need to be at least moderator)***\n" +
                        "• Trigger Add <Word or Words> <Mode> - Adds one word or a sequence of words to the Trigger List. The Mode parameter accepts \"normal\"" + 
                        ", \"characters\" where...\n" +
                        "•• normal - no special characters for letters. You can also leave this parameter empty for normal\n" + 
                        //"•• leet - included special characters for letters like \"Tr!gg3r\" (1337 5p34k)\n" + //TODO
                        "• Trigger Info <Word or Words> - Shows infos about the set trigger\n" + 
                        "• Trigger Remove <Word or Words> - Removes a trigger\n" + 
                        "• Trigger AddMod <Discord Tag> - Adds a moderator **(OWNER ONLY)**\n" +
                        "• Trigger ListMods - Lists all moderators **(OWNER ONLY)**\n" +
                        "• Trigger RemoveMod <Discord Tag> - Removes a moderator **(OWNER ONLY)**\n" +
                        "• Trigger Permissions - Shows all the needed permissions in a pretty list\n\n" +
                        "***Notice***\n" +
                        "• All commands are case-insensitive. So you can write everything capitalized, or the opposite, or just a mix of both.\n" +
                        "• Discord Tags are to be written as \"User#1234\" without the @ for example.\n" +
                        "• \"Trigger Add jam glass\" will add the trigger \"jam glass\".\n" +
                        "• I am still under construction. Please be patient with me when I crash. My programmer will fix the problems as fast as possible."); 
                    break;
                /**
                 * 
                 **/
                case "trigger list2":
                    if (triggerWords.Count == 0)
                        await message.Channel.SendMessageAsync("There are no triggers right now..");
                    else
                    {
                        string output = "";
                        foreach (string item in triggerWords)
                        {
                            string localstring = item.Split(';')[0];
                            output += "• " + localstring + "\n";
                        }

                        if (output.Length >= 1984)
                            await message.Channel.SendMessageAsync("Output string too long. Please contact a developer: https://github.com/Rumir/TriggerMe_Bot/issues");
                        else
                            await message.Channel.SendMessageAsync("***Triggers***\n" + output);
                    }
                    break;
                /**
                 * 
                 **/
                case "trigger list":
                    ArrayList listt = sqlClient.PutCommand($"SELECT trigger_word FROM triggers WHERE server_id = {chnl.Guild.Id};");
                    if (listt.Count == 0)
                        await message.Channel.SendMessageAsync("There are no triggers right now..");
                    else
                    {
                        string output = "";
                        foreach (string item in triggerWords)
                        {
                            string localstring = item.Split(';')[0];
                            output += "• " + localstring + "\n";
                        }

                        if (output.Length >= 1984)
                            await message.Author.SendMessageAsync("Output string too long. Please contact a developer: https://github.com/Rumir/TriggerMe_Bot/issues");
                        else
                            await message.Author.SendMessageAsync("***Triggers***\n" + output);
                    }
                    break;
                /**
                 * 
                 **/
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
            /**
            * 
            **/
            if (message.Content.ToLower().StartsWith("trigger add "))
            {
                if (!IsMod(message) && chnl.Guild.OwnerId != message.Author.Id)
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
                
                foreach(string item in triggerWords)
                {
                    if (item.Contains(trigger_word))
                    {
                        await message.Channel.SendMessageAsync("Trigger already existing");
                        return;
                    }
                }
                
                string setting = parameters.Split(' ')[parameters.Split(' ').Length - 1].ToLower();
                if (setting != "normal" && setting != "leet")
                {
                    await message.Channel.SendMessageAsync("No setting was given. Using \"normal\" as setting");
                    setting = "normal";
                }
                //else if (setting == "leet")
                //{
                //    await message.Channel.SendMessageAsync("Setting \"leet\" not implemented yet. Using \"normal\"!"); //I'm lazy, okay?
                //    setting = "normal";
                //}

                sqlClient.PutCommand($"INSERT INTO triggers (server_id, trigger_word, trigger_setting, added_by) VALUES (" +
                    $"{chnl.Guild.Id}," +
                    $"'{trigger_word}'," +
                    $"'{setting}'," +
                    $"{message.Author.Id});");

                triggerWords.Add(trigger_word + ";" + setting + ";");
                triggerWords.Sort();
                await message.Channel.SendMessageAsync("Trigger added!");
                

            }
            /**
            * 
            **/
            else if (message.Content.ToLower().StartsWith("trigger info "))
            {
                if (!IsMod(message) && chnl.Guild.OwnerId != message.Author.Id)
                {
                    await message.Channel.SendMessageAsync("Sorry, but you are not a moderator");
                    return;
                }
                parameters = message.Content.ToLower().Remove(0, 13);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);

                ArrayList info = sqlClient.PutCommand($"SELECT trigger_word,trigger_setting,added_by " +
                    $"FROM triggers WHERE server_id = {chnl.Guild.Id} AND trigger_word = '{parameters}';");

                if (info.Count == 0)
                {
                    await message.Channel.SendMessageAsync("Trigger does not exist");
                    return;
                }

                string trigger_word = info[0].ToString().Split(';')[0];
                string setting = info[0].ToString().Split(';')[1];
                SocketUser user = _client.GetUser(Convert.ToUInt64(info[0].ToString().Split(';')[2]));
                string added_by = user.Username + "#" + user.Discriminator;

                await message.Channel.SendMessageAsync($"**Trigger Word:** {trigger_word}\n" +
                    $"**Trigger Setting:** {setting}\n" +
                    $"**Added by:** {added_by}");
            }
            /**
            * 
            **/
            else if (message.Content.ToLower().StartsWith("trigger remove "))
            {
                if (!IsMod(message) && chnl.Guild.OwnerId != message.Author.Id)
                {
                    await message.Channel.SendMessageAsync("Sorry, but you are not a moderator");
                    return;
                }
                parameters = message.Content.ToLower().Remove(0, 15);
                while (parameters[0] == ' ')
                    parameters = message.Content.ToLower().Remove(0, 1);

                if(triggerWords.Count == 0)
                {
                    await message.Channel.SendMessageAsync("There are no triggers to delete.");
                    return;
                }

                foreach (string item in triggerWords)
                {
                    if (item.Split(';')[0] == (parameters))
                    {
                        sqlClient.PutCommand($"DELETE FROM triggers WHERE server_id = {chnl.Guild.Id} AND trigger_word = '{parameters}'");
                        triggerWords.Remove(item);
                        await message.Channel.SendMessageAsync("Trigger removed.");
                        return;
                    }
                }
                await message.Channel.SendMessageAsync("Trigger not found.");
            }
            /**
            * 
            **/
            else if (message.Content.ToLower().StartsWith("trigger addmod "))
            {
                if(message.Author.Id != chnl.Guild.OwnerId)
                {
                    await message.Channel.SendMessageAsync("You are not the owner of the Server!");
                    return;
                }
                parameters = message.Content.Remove(0, 15);
                while (parameters[0] == ' ')
                    parameters = message.Content.Remove(0, 1);
                try
                {
                    SocketUser user = _client.GetUser(parameters.Remove(parameters.Length - 5), parameters.Substring(parameters.Length - 4));
                    UInt64 client_id = Convert.ToUInt64(user.Id.ToString());
                    if (moderators.Contains(client_id.ToString()))
                    {
                        await message.Channel.SendMessageAsync("User is already Moderator.");
                        return;
                    }
                    sqlClient.PutCommand($"INSERT INTO `moderators`(`client_id`, `server_id`) VALUES ({client_id},{chnl.Guild.Id})");
                    moderators.Add(client_id.ToString());
                    await message.Channel.SendMessageAsync("Moderator added");
                }
                catch (Exception)
                {
                    await message.Channel.SendMessageAsync("Could not create Client ID for given User. Is the user existing?");
                }
                
            }
            /**
            * 
            **/
            else if (message.Content.ToLower().StartsWith("trigger listmods"))
            {
                if (message.Author.Id != chnl.Guild.OwnerId)
                {
                    await message.Channel.SendMessageAsync("You are not the owner of the Server!");
                    return;
                }
                string output = "***Moderators***\n";
                foreach (string item in moderators)
                    output += "• " + _client.GetUser(Convert.ToUInt64(item)).Username + "#" + _client.GetUser(Convert.ToUInt64(item)).Discriminator + "\n";
                if (output == "***Moderators***\n")
                    await message.Channel.SendMessageAsync("There are no moderators yet.");
                else
                    await message.Channel.SendMessageAsync(output);
            }
            /**
            * 
            **/
            else if (message.Content.ToLower().StartsWith("trigger removemod "))
            {
                if (message.Author.Id != chnl.Guild.OwnerId)
                {
                    await message.Channel.SendMessageAsync("You are not the owner of the Server!");
                    return;
                }
                parameters = message.Content.Remove(0, 18);
                while (parameters[0] == ' ')
                    parameters = message.Content.Remove(0, 1);
                
                try
                {
                    SocketUser user = _client.GetUser(parameters.Remove(parameters.Length - 5), parameters.Substring(parameters.Length - 4));
                    UInt64 client_id = Convert.ToUInt64(user.Id.ToString());
                    if (!moderators.Contains(client_id.ToString()))
                    {
                        await message.Channel.SendMessageAsync("User is not a moderator.");
                        return;
                    }
                    sqlClient.PutCommand($"DELETE FROM moderators WHERE server_id = {chnl.Guild.Id} AND client_id = '{client_id}'");
                    moderators.Remove(client_id.ToString());
                    await message.Channel.SendMessageAsync("User removed as moderator");
                }
                catch (Exception)
                {
                    await message.Channel.SendMessageAsync("Could not create Client ID for given User. Is the user existing?");
                }
            }

            /////////////////////

            else if (message.Content.ToLower().StartsWith("trigger add")
                || message.Content.ToLower().StartsWith("trigger info")
                || message.Content.ToLower().StartsWith("trigger remove")
                || message.Content.ToLower().StartsWith("trigger addmod")
                || message.Content.ToLower().StartsWith("trigger removemod"))
#pragma warning disable CS0642 // Möglicherweise falsche leere Anweisung
                ; //Absolutely nothing
#pragma warning restore CS0642 // Möglicherweise falsche leere Anweisung

            else
            {
                string triggeredWord = CheckForTriggers(message);
                if(triggeredWord != "")
                {
                    List<SocketMessage> delMessage = new List<SocketMessage>() { message };
                    await message.Channel.DeleteMessagesAsync(delMessage);
                    sqlClient.PutCommand($"INSERT INTO `violations`(`server_id`, `user_id`, `datetime`, `trigger_word`, `message`) VALUES (" +
                        $"{chnl.Guild.Id}," +
                        $"{message.Author.Id}," +
                        $"{DateTime.Now.ToUniversalTime().ToBinary()}," +
                        $"'{triggeredWord}'," +
                        $"'{message.Content}');");
                    await message.Author.SendMessageAsync($"I had to delete your last message from Channel **{message.Channel.Name}** because you used" +
                        $" the Trigger Word ***{triggeredWord}***.");
                }
            }
            
        }

        private string CheckForTriggers(SocketMessage message)
        {
            foreach(string item in triggerWords)
            {
                string word = item.Split(';')[0];
                if (message.Content.ToLower().Contains(word))
                    return word;
            }
            return "";
        }

        private bool IsMod(SocketMessage message)
        {
            if(!moderators.Contains(message.Author.Id.ToString()))
                return false;
            return true;
        }

        
    }
}
