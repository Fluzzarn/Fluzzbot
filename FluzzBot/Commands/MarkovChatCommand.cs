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
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown = value; }
        public int Cooldown { get => _cooldown; set => _cooldown = value; }


        private int order = 1;
        private Dictionary<string, DateTime> _timerStartDict;

        public MarkovChatCommand()
        {
            _commandName = "!markov";
            _hasCooldown = true;
            _cooldown = 300;
        }
        public bool Execute(FluzzBot bot, string message,string username)
        {
            if(message.Split(' ').Length > 1)
            {
                if (int.TryParse(message.Split(' ')[1], out int newOrder))
                {
                    if(order > 0)
                        order = newOrder;
                }
                else
                {
                    bot.ConstructAndEnqueueMessage("Could not change order of bot, syntax is !markov <order>", username);
                }
            }
                var dict = MarkovHelper.BuildTDict(bot.MarkovText[username], order);
                bot.ConstructAndEnqueueMessage(MarkovHelper.BuildString(dict, 25, true),username);
            return true;
        }
    }
}
