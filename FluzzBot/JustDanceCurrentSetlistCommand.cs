using FluzzBot;
using System;
using System.Collections.Generic;
using System.Text;

namespace FluzzBotCore
{
    class JustDanceCurrentSetlistCommand : Command
    {
        public string CommandName { get => "!jdCurrentSetlist"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot.FluzzBot bot, string message)
        {
            string msg = "Current setlist is: ";
            foreach (var item in bot.JustDanceSetlist.GetSetlist())
            {
                msg += item + ", ";
            }

            bot.ConstructAndEnqueueMessage(msg);
            return true;
        }
    }
}
