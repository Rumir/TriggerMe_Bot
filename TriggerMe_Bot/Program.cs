using System;
using System.Collections;

namespace TriggerMe_Bot
{
    class Program
    {
        static void Main(string[] args)
            => new TheBot().MainAsync().GetAwaiter().GetResult();
        
    }
}
