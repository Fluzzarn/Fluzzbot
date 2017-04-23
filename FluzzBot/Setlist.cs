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



        public Setlist()
        {
            SongSetlist = new List<Song>();
        }
        public bool AddSongToSetlist(Song s)
        {
            try
            {
                SongSetlist.Add(s);
                if (SongSetlist.Count == 1)
                    _CurrentSong = s;
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

                if(SongSetlist.Count >= 2)
                {
                    SongSetlist.RemoveRange(0, 1);
                    _CurrentSong = SongSetlist[0];
                }
                else
                {
                    if (SongSetlist.Count == 1)
                        SongSetlist.RemoveRange(0, 1);
                    _CurrentSong = null;

                }
                return _CurrentSong;
            

        }

        public TimeSpan SetlistLength()
        {
            TimeSpan ts = new TimeSpan();

            foreach (Song s in SongSetlist)
            {
                try
                {
                    ts = ts.Add(new TimeSpan(0, 0, s.Duration));
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
