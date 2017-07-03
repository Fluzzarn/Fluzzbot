using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class RoflbotrSenpaiCommand : Command,ICommand
    {
        public string CommandName { get => "The love between fluzzbot and theroflBOTr is 21"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message,string username)
        {
            bot.ConstructAndEnqueueMessage("PunOko Why won't roflrbot senpai notice me? PunOko",username);
            return true;
        }
    }
}
