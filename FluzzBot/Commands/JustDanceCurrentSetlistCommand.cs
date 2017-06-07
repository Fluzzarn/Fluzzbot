using FluzzBot;
using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluzzBotCore
{
    class JustDanceCurrentSetlistCommand : Command,ICommand
    {
        public string CommandName { get => "!jdCurrentSetlist"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        public bool Execute(FluzzBot.FluzzBot bot, string message, string username)
        {
            Execute(bot,username);
            string msg = "Current setlist is: ";
            foreach (var item in bot.JustDanceSetlist.GetSetlist())
            {
                msg += item + ", ";
            }

            bot.ConstructAndEnqueueMessage(msg,username);
            return true;
        }
    }
}
