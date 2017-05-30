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
            Credentials misskaddykins = new Credentials();
            if (args.Length > 0)
            if(args[0] != "")
            {
                fluzzarn.ChannelName = args[0];
            }


            misskaddykins.ChannelName = "misskaddykins";
            FluzzBot bot = new FluzzBot(fluzzarn);
            FluzzBot bot2 = new FluzzBot(misskaddykins);
            bot.Start();
            //Thread th = new Thread(() => bot.Start());
            ////Thread th2 = new Thread(() => bot2.Start());
            ////bot.Start();
            //
            //th.Start();
            ////th2.Start();
            //th.Join();
            ////th2.Join();
        }
    }
}
