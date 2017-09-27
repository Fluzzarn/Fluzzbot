using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class SetCooldownCommand : Command, ICommand
    {



        public string CommandName { get => "!setcd"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => _requireMod; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown= value; }
        public bool AutoFire { get => false; set => throw new NotImplementedException(); }


        public int Cooldown { get => _cooldown;  set { _cooldown = value; } }


        public SetCooldownCommand()
        {
            _requireMod = true;
        }

        public bool Execute(FluzzBot bot, string message,string username)
        {
            try
            {
                string toChange = message.Split(' ')[1];
                foreach (var command in bot.Commands)
                {
                    //found it
                    if(command.CommandName == toChange)
                    {
                        command.HasCooldown = true;
                        int newDuration = Convert.ToInt32(message.Split(' ')[2]);
                        command.Cooldown = newDuration;
                        string insertStatement = "INSERT into command_cooldown(user_id,command_name,cooldown) SELECT Usernames.user_id, @toChange , @newDuration FROM Usernames WHERE username LIKE @username";

                        string updateStatement = "UPDATE command_cooldown SET cooldown = @newDuration WHERE command_name LIKE @toChange AND user_id LIKE(SELECT Usernames.user_id FROM Usernames WHERE Usernames.username like @username)";

                        int rows = MySQLHelper.RunSQLRequest(updateStatement, new Dictionary<string, string>() { { "@toChange", toChange }, { "@newDuration", newDuration.ToString() }, { "@username", username } });


                        if (rows < 1)
                        {
                            MySQLHelper.RunSQLRequest(insertStatement, new Dictionary<string, string>() { { "@toChange", toChange }, { "@newDuration", newDuration.ToString() }, { "@username", username } });
                        }


                        bot.ConstructAndEnqueueMessage("Successfully changed command's cooldown",username);
                        return true;
                    }
                }
                bot.ConstructAndEnqueueMessage("Could not change "+ toChange+"'s cooldown, could not find the command",username);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                bot.ConstructAndEnqueueMessage("Error trying to change cooldown, correct syntax is: !setcd <command> <new cooldown in seconds>",username);
                
            }
            return false;
        }
    }
}
