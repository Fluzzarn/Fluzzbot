using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FluzzBot.Commands
{
    class Command
    {

        protected string _commandName;
        protected bool _requireMod;
        protected bool _hasCooldown;
        protected int _cooldown;
        protected bool _onCooldown;
        protected Dictionary<string, int> _cooldownDict;
        protected Dictionary<string, bool> _onCoolDownDict;
        protected Dictionary<string, Timer> _timerDict;

        public void Execute(FluzzBot bot,string username)
        {
            if(_onCoolDownDict == null)
            {
                _cooldownDict = new Dictionary<string, int>();
                _onCoolDownDict = new Dictionary<string, bool>();
                _timerDict = new Dictionary<string, Timer>();
            }
            if (_hasCooldown)
            {
                _onCooldown = false;

                string selectStatement = "SELECT * FROM command_cooldown WHERE user_id LIKE(SELECT user_id from Usernames WHERE username LIKE @username)";

                MySqlDataReader dataReader = MySQLHelper.GetSQLDataFromDatabase(selectStatement, new Dictionary<string, string>() { { "@username", username } });

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        if((string)(dataReader["command_name"]) == _commandName)
                        {
                            _cooldownDict[username] = (int)dataReader["cooldown"];
                            _hasCooldown = true;
                            break;
                        }
                        else
                        {
                            _cooldownDict[username] = _cooldown;
                            continue;
                        }

                    }
                }
                else
                {
                    _cooldownDict[username] = _cooldown;

                }



                if(!_timerDict.ContainsKey(username))
                    _timerDict[username] = new Timer(_cooldownDict[username] * 1000);
            }
        }


        protected void _timer_Elapsed(object sender, ElapsedEventArgs e, string username)
        {
            _onCoolDownDict[username] = false;
            _onCooldown = false;
            _timerDict[username].Interval = _cooldownDict[username] * 1000;
            _timerDict[username].Stop();
        }
    }
}
