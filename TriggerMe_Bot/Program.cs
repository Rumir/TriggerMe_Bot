using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Net;

namespace TriggerMe_Bot
{
    class Program
    {
        static void Main(string[] args)
            => new TheBot().MainAsync().GetAwaiter().GetResult();
    }
}
