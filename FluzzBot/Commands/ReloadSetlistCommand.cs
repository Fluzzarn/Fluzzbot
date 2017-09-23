using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class ReloadSetlistCommand :Command,ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => false; set => throw new NotImplementedException(); }
        public int Cooldown { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }

        public ReloadSetlistCommand()
        {
            _commandName = "!jdReload";
        }

        public bool Execute(FluzzBot bot, string message, string username)
        {
            bot.JustDanceDict[username] = new JustDanceSetlist(bot);
            bot.JustDanceDict[username].LoadSetlistFromDatabase(username);
            bot.ConstructAndEnqueueMessage("Reloaded setlist from database",username);
            return true;
        }
    }
}
