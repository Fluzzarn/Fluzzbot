using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class CurrentSetlistCommand : ICommand
    {
        public string CommandName { get => "!currentSetlist"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message,string username)
        {
            string messageToSend = "Current songs in the setlist are:";
            List<Song> songs = bot.SongList.GetSetlist();

            foreach (Song s in songs)
            {
                messageToSend += " " + s.Name + " by " + s.Artist + ",";
            }

           if(songs.Count > 0)  messageToSend.Remove(messageToSend.Length - 1, 1);

            bot.ConstructAndEnqueueMessage(messageToSend,username);
            return true;
        }
    }
}
