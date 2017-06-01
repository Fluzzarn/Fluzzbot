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
        protected Timer _timer;


        public void Execute(FluzzBot bot)
        {
            if (_hasCooldown && _timer == null)
            {
                _onCooldown = false;

                MySql.Data.MySqlClient.MySqlConnection conn;
                var connString = String.Format("server={0};uid={1};pwd={2};database={3};SslMode=None", DatabaseCredentials.DatabaseHost, DatabaseCredentials.DatabaseUsername, DatabaseCredentials.DatabasePassword, DatabaseCredentials.DatabaseName);
                conn = new MySql.Data.MySqlClient.MySqlConnection();
                conn.ConnectionString = connString;
                conn.Open();


                string selectStatement = "SELECT * FROM command_cooldown WHERE user_id LIKE(SELECT user_id from Usernames WHERE username LIKE '" + bot.Credentials.ChannelName + "')";

                Console.WriteLine(selectStatement);
                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = selectStatement;
                cmd.Connection = conn;


                MySqlDataReader dataReader = cmd.ExecuteReader();

                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {

                        if((string)(dataReader["command_name"]) == _commandName)
                        {
                            _cooldown = (int)dataReader["cooldown"];
                            _hasCooldown = true;
                            break;
                        }
                        else
                        {
                            continue;
                        }

                    }
                }

                conn.Close();




                _timer = new Timer(_cooldown * 1000);
                _timer.Elapsed += _timer_Elapsed;
            }
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _onCooldown = false;
            _timer.Interval = _cooldown * 1000;
            _timer.Stop();
        }
    }
}
