using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FluzzBot
{
    class Program
    {
        static void Main(string[] args)
        {

            if(args.Length > 0)
            if(args[0] != "")
            {
                Credentials.ChannelName = args[0];
            }
            FluzzBot bot = new FluzzBot();

            
            bot.Start();

            while(bot.IsRunning)
            {
               
            }
        }
    }
}
