using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class NextSongCommand : Command
    {
        private string _commandName = "!nextsong";
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message)
        {
            try
            {
                bot.SongList.NextSong();
                if (bot.SongList.CurrentSong() == null)
                    bot.ConstructAndEnqueueMessage("Current no songs in the setlist!");
                else
                    bot.ConstructAndEnqueueMessage("Next Song is: " + bot.SongList.CurrentSong());
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }
    }
}
