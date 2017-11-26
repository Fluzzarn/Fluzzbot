using FluzzBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class ReloadSettingsCommand : Command,ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => _requireMod; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => throw new NotImplementedException(); }
        public int Cooldown { get => _cooldown; set => throw new NotImplementedException(); }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }

        public ReloadSettingsCommand()
        {
            _commandName = "!reloadSettings";
            _requireMod = true;
            _hasCooldown = false;
        }
        public bool Execute(FluzzBot bot, string message, string username)
        {
           // bot.ReloadSettings();
            return true;
        }
    }
}
