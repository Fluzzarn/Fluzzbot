using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class GetSpookCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown = value; }
        public int Cooldown { get => _cooldown; set => _cooldown = value; }
        public bool OnCooldown { get => _onCooldown; set => _onCooldown = value; }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }



        public GetSpookCommand() : base()
        {
            _commandName = "!spooks";
            _hasCooldown = false;

        }

        public bool Execute(FluzzBot bot, string message, string username)
        {
            var _spookDict = bot.Spookdict;

            if (!_spookDict.ContainsKey(username))
            {
               _spookDict[username] = int.Parse(File.ReadAllText("./spooks_" + username + ".txt"));
            }
            bot.ConstructAndEnqueueMessage(username + " has been spooked " + _spookDict[username] + " times! kadSpoopy", username);
            return true;
        }
    }
}
