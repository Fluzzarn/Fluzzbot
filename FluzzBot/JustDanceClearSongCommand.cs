using FluzzBotCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class JustDanceClearSongCommand : Command
    {
        public string CommandName { get => "!jdRemove"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message)
        {
            JustDanceSetlist set = bot.JustDanceSetlist;
            string song = message.Substring(CommandName.Length + 1);
            bool result = set.RemoveSong(song);
            string sentMessage = "";
            if (result) sentMessage = ("Removed " + song + " from the queue"); else sentMessage = ("Could not remove song from queue, yell at Fluzzarn");
                bot.ConstructAndEnqueueMessage(sentMessage);

            return true;
        }
    }
}
