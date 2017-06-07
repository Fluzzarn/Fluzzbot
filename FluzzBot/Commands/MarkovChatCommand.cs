using FluzzBot.Markov;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class MarkovChatCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => throw new NotImplementedException(); }
        public int Cooldown { get => _cooldown; set => throw new NotImplementedException(); }


        private int order = 1;

        public MarkovChatCommand()
        {
            _commandName = "!markov";
            _hasCooldown = true;
            _cooldown = 300;

        }
        public bool Execute(FluzzBot bot, string message,string username)
        {
            Execute(bot,username);


            if(message.Split(' ').Length > 1)
            {
                if (int.TryParse(message.Split(' ')[1], out int newOrder))
                {
                    order = newOrder;
                }
                else
                {
                    bot.ConstructAndEnqueueMessage("Could not change order of bot, syntax is !markov <order>", username);
                }
            }

            if (!_onCoolDownDict.ContainsKey(username))
                _onCoolDownDict.Add(username, false);

            if (!_onCoolDownDict[username])
            {
                if (HasCooldown)
                {
                    if (_onCoolDownDict.ContainsKey(username))
                    {
                        _onCoolDownDict[username] = true;
                    }
                    else
                        _onCoolDownDict.Add(username, true);
                    _timerDict[username].Interval = _cooldownDict[username] * 1000;
                    _timerDict[username].Elapsed += (sender, args) => { _timer_Elapsed(sender, args, username); };
                    _timerDict[username].Start();
                }

                var dict = MarkovHelper.BuildTDict(bot.MarkovText[username], order);
                bot.ConstructAndEnqueueMessage(MarkovHelper.BuildString(dict, 10, true),username);
            }
            return true;
        }
    }
}
