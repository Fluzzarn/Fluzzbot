using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class AddCommandCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => _requireMod; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => throw new NotImplementedException(); }
        public int Cooldown { get => _cooldown; set => throw new NotImplementedException(); }


        public AddCommandCommand()
        {
            _commandName = "!addCommand";
            _requireMod = true;
            _hasCooldown = false;
        }
        public bool Execute(FluzzBot bot, string message, string username)
        {
            string queury = "SET SQL_SAFE_UPDATES = 0;DELETE FROM banned_commands WHERE command LIKE @command AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";
            MySQLHelper.RunSQLRequest(queury, new Dictionary<string, string>() { { "@command", message.Split(' ')[1] }, { "@username", username } });
            bot.RemovedCommands[username].RemoveAll((x) => x == message.Split(' ')[1]);
            bot.ConstructAndEnqueueMessage(message.Split(' ')[1] + " has been added to this channel", username);
            return true;
        }
    }
}
