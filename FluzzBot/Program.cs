﻿using MySql.Data.MySqlClient;
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

            FluzzBot bot = new FluzzBot();

            
            bot.Start();

            while(bot.IsRunning)
            {
               
            }
        }
    }
}
