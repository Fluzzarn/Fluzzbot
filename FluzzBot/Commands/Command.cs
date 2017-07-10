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

        public Dictionary<string,DateTime> _timerStartDict { get; private set; }

        public Command()
        {
            if (_onCoolDownDict == null)
            {
                _cooldownDict = new Dictionary<string, int>();
                _onCoolDownDict = new Dictionary<string, bool>();
                _timerDict = new Dictionary<string, Timer>();
                _timerStartDict = new Dictionary<string, DateTime>();
            }
        }

        public bool PreExecute(FluzzBot bot, string username)
        {
            if (_onCoolDownDict == null)
            {
                _cooldownDict = new Dictionary<string, int>();
                _onCoolDownDict = new Dictionary<string, bool>();
                _timerDict = new Dictionary<string, Timer>();
                _timerStartDict = new Dictionary<string, DateTime>();
            }
            if (_hasCooldown)
            {
                CheckForCooldown(username);
                if (!_onCoolDownDict.ContainsKey(username))
                    _onCoolDownDict.Add(username, false);

                if (!_onCoolDownDict[username])
                {
                    if (_hasCooldown)
                    {
                        if (_onCoolDownDict.ContainsKey(username))
                        {
                            _onCoolDownDict[username] = true;
                        }
                        else
                            _onCoolDownDict.Add(username, true);
                        _timerDict[username].Interval = _cooldownDict[username] * 1000;
                        _timerDict[username].Elapsed += (sender, args) => { _timer_Elapsed(sender, args, username); };

                        if (_timerStartDict.ContainsKey(username))
                        {
                            _timerStartDict[username] = DateTime.Now;
                        }
                        else
                        {
                            _timerStartDict.Add(username, DateTime.Now);
                        }

                        _timerDict[username].Start();
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    double secs = (DateTime.Now - _timerStartDict[username]).TotalSeconds;
                    bot.ConstructAndEnqueueMessage("I'm on cooldown for another " + Math.Floor( _cooldownDict[username] + 0.0 - secs ) + " seconds", username);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        private void CheckForCooldown(string username)
        {
            _onCooldown = false;

            string selectStatement = "SELECT * FROM command_cooldown WHERE user_id LIKE(SELECT user_id from Usernames WHERE username LIKE @username)";
            MySql.Data.MySqlClient.MySqlConnection conn;
            MySqlDataReader dataReader = MySQLHelper.GetSQLDataFromDatabase(selectStatement, new Dictionary<string, string>() { { "@username", username } }, out conn);

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {

                    if ((string)(dataReader["command_name"]) == _commandName)
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



            if (!_timerDict.ContainsKey(username))
                _timerDict[username] = new Timer(_cooldownDict[username] * 1000);

            conn.Close();
        }

        public void Execute(FluzzBot bot,string username)
        {
           
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
