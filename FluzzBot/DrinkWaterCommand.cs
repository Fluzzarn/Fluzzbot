using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class DrinkWaterCommand : Command, ICommand
    {
        public string CommandName { get => "fluzz drink water"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => 0; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message, string username)
        {
            bot.ConstructAndEnqueueMessage("nah I don't need water, I'm a robot", username);
            return true;
        }
    }
}
