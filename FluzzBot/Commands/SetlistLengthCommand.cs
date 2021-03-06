﻿using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class SetlistLengthCommand : Command,ICommand
    {
        //private string _commandName = "!rbSetlistLength";
        public string CommandName { get =>  _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }

        public SetlistLengthCommand()
        {
            _commandName = "!rbSetlistLength";
        }
        public bool Execute(FluzzBot bot, string message,string username)
        {

            

            TimeSpan ts = bot.SongList.SetlistLength();
            int hours = (int)(ts.TotalSeconds / 3600);
            int minutes = (int)(ts.TotalSeconds / 60);
            int seconds = (int)(ts.TotalSeconds % 60);
            bool hasLength = false;
            string stringMessage = "Setlist is currently ";

            if (hours == 0 && minutes == 0 && seconds == 0)
            {
                hasLength = true;
                stringMessage += "empty!";
            }

            if(hours > 0)
            {
                stringMessage += hours + " hours ";
            }
            if (minutes > 0)
            {
                if (hours > 0)
                    stringMessage += ",";
                stringMessage += minutes + " minutes ";
            }
            if (seconds > 0)
            {
                if (minutes > 0 || hours > 0)
                    stringMessage += " and ";
                stringMessage += seconds + " seconds";
            }
            if(hasLength)
            {
                stringMessage += " long!";
            }
            bot.ConstructAndEnqueueMessage(stringMessage,username);
            return true;
        }
    }
}
