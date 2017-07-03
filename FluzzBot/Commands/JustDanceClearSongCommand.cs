using FluzzBot.Commands;
using FluzzBotCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class JustDanceClearSongCommand : Command,ICommand
    {
        public string CommandName { get => "!jdRemove"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message,string username)
        {
            JustDanceSetlist set = bot.JustDanceSetlist;
            string song = message.Substring(CommandName.Length + 1);
            bool result = set.RemoveSong(song,username);
            string sentMessage = "";
            if (result) sentMessage = ("Removed " + song + " from the queue"); else sentMessage = ("Could not remove song from queue, yell at Fluzzarn");
                bot.ConstructAndEnqueueMessage(sentMessage,username);

            return true;
        }
    }
}
