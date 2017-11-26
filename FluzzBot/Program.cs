using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace FluzzBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Credentials fluzzarn = new Credentials();
            if (args.Length > 0)
            if(args[0] != "")
            {
                fluzzarn.ChannelName = args[0];
            }


            FluzzBot bot = new FluzzBot(fluzzarn);
            bot.Start();
        }
    }
}
