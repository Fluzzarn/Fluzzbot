using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class SpookCountCommand : Command, ICommand
    {

        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => true; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown = value; }
        public int Cooldown { get => _cooldown; set => _cooldown = value; }
        public bool OnCooldown { get => _onCooldown; set => _onCooldown = value; }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }



        public SpookCountCommand() : base()
        {
            _commandName = "!addspook";
            _hasCooldown = true;
            _cooldown = 5;

        }

        public bool Execute(FluzzBot bot, string message, string username)
        {
            var _spookDict = bot.Spookdict;

            if (_spookDict.ContainsKey(username))
            {
                _spookDict[username]++;
            }
            else
            {
                if (File.Exists("./spooks_" + username + ".txt"))
                {
                    _spookDict[username] = int.Parse(File.ReadAllText("./spooks_" + username + ".txt"));
                }
                else
                    _spookDict[username] = 1;
            }

            File.WriteAllText("./spooks_" + username + ".txt", _spookDict[username].ToString());
            bot.Spookdict = _spookDict;

            bot.ConstructAndEnqueueMessage("kadSpoopy", username);
            return true;
        }
    }
}
