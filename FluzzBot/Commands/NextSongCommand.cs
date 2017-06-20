using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class NextSongCommand : ICommand
    {
        private string _commandName = "!rbNextsong";
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message,string username)
        {
            try
            {
                bot.SongList.NextSong(username);
                if (bot.SongList.CurrentSong() == null)
                    bot.ConstructAndEnqueueMessage("Current no songs in the setlist!",username);
                else
                    bot.ConstructAndEnqueueMessage("Next Song is: " + bot.SongList.CurrentSong(),username);
            }
            catch (Exception)
            {

                throw;
            }
            return true;
        }
    }
}
