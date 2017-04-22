using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    public class Song
    {

        public string Name { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public char Gender{ get; set; }
        public int VocalParts { get; set; }
        public bool Cover { get; set; }
        public int Guitar { get; set; }
        public int Bass { get; set; }
        public int Drums { get; set; }
        public int Vocals { get; set; }
        public bool FreestyleGuitar { get; set; }
        public bool FreestyleVocals { get; set; }
        public int BPM { get; set; }
        public DateTime Released { get; set; }
        public bool Delisted{ get; set; }
        public string Sources { get; set; }
        public string RequestedBy { get; set; }
        public int Duration { get; set; } //in seconds

    }
}
