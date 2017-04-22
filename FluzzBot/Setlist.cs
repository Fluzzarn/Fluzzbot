using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    public class Setlist
    {

        List<Song> SongSetlist;
        Song _CurrentSong;


        public bool AddSongToSetlist(Song s)
        {
            try
            {
                SongSetlist.Add(s);
            }
            catch (Exception)
            {

                return false;
            }
            return true;
        }

        public Song CurrentSong()
        {
            return _CurrentSong;
        }


        public Song NextSong()
        {
            try
            {
                _CurrentSong = SongSetlist[1];
                SongSetlist.RemoveRange(0, 1);
                return _CurrentSong;
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Setlist is currently Empty!");

                Song s = new Song { Name = "Your Choice" };
                SongSetlist.Add(s);
                SongSetlist.RemoveRange(0, 1);
                return s;
            }

        }

        public TimeSpan SetlistLength()
        {
            TimeSpan ts = new TimeSpan();

            foreach (Song s in SongSetlist)
            {
                try
                {
                    ts.Add(new TimeSpan(0, 0, s.Duration));
                }
                catch (NullReferenceException ex)
                {

                    Console.WriteLine(s.Name + " did not have a duration!");
                }
                
            }

            return ts;
        }
    }
}
