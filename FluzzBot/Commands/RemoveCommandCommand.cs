using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class RemoveCommandCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => _requireMod; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => throw new NotImplementedException(); }
        public int Cooldown { get => _cooldown; set => throw new NotImplementedException(); }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }

        public RemoveCommandCommand()
        {
            _commandName = "!removeCommand";
            _requireMod = true;
            _hasCooldown = false;
        }
        public bool Execute(FluzzBot bot, string message, string username)
        {
            string setListUpdate = "INSERT INTO banned_commands (user_id,command) SELECT Usernames.user_id, @command FROM Usernames WHERE username LIKE @username";

            int rows = MySQLHelper.RunSQLRequest(setListUpdate, new Dictionary<string, string>() { { "@command", message.Split(' ')[1] }, { "@username", username } });

            logger.Info("Removed " + message.Split(' ')[1] + " from channel " + username);
            bot.RemovedCommands[username].Add(message.Split(' ')[1]);
            bot.ConstructAndEnqueueMessage("Command has been removed from this channel, to re-add, run !addCommand",username);
            return rows >= 1;
        }
    }
}
