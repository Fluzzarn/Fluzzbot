using FluzzBot.Markov;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    using TDict = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, uint>>;

    class MarkovChatCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown = value; }
        public int Cooldown { get => _cooldown; set => _cooldown = value; }


        private int order = 1;

        private Dictionary<string, TDict> _tDictDict;

        public MarkovChatCommand()
        {
            _commandName = "!markov";
            _hasCooldown = true;
            _cooldown = 300;
            _tDictDict = new Dictionary<string, TDict>();
        }
        public bool Execute(FluzzBot bot, string message,string username)
        {
            if (message.Split(' ').Length > 1)
            {
                if (int.TryParse(message.Split(' ')[1], out int newOrder))
                {
                    if (order > 0)
                        order = newOrder;
                }
                else
                {
                    bot.ConstructAndEnqueueMessage("Could not change order of bot, syntax is !markov <order>", username);
                }
            }

            TDict dict = MakeTDict(bot, username);
            lock (bot.MarkovText)
            {
                File.AppendAllText("./markov/" + username.ToLower() + ".txt", bot.MarkovText[username.ToLower()]);
            }

            bot.MarkovText[username] = "";

            bot.ConstructAndEnqueueMessage(MarkovHelper.BuildString(dict, 25, true), username);
            return true;
        }

        public TDict MakeTDict(FluzzBot bot, string username)
        {
            TDict dict;
            if (_tDictDict.ContainsKey(username))
            {
                dict = _tDictDict[username];
                dict = MarkovHelper.BuildTDict(bot.MarkovText[username], order, dict);
                _tDictDict[username] = dict;
            }
            else
            {
                dict = MarkovHelper.BuildTDict(bot.MarkovText[username], order);
                _tDictDict.Add(username, dict);
            }



            return dict;
        }
    }
}
