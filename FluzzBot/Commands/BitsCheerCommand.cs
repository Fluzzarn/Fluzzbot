using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class BitsCheerCommand : Command,ICommand

    {
        public string CommandName { get => null; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message,string username)
        {
            if(username == "misskaddykins")
            bot.ConstructAndEnqueueMessage("kadCheer kadCheer THANKS FOR THE BITTIES kadCheer kadCheer",username);
            return true;
        }
    }
}
