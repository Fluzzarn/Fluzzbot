﻿using FluzzBot.Markov;
using Newtonsoft.Json;
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
        public bool AutoFire { get => true; set => throw new NotImplementedException(); }

        private int order = 4;

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


            TDict dict = MakeTDict(bot, username);
            SaveTDict(username);
            lock (bot.MarkovText)
            {
               // File.AppendAllText("./markov/" + username.ToLower() + ".txt",Environment.NewLine + bot.MarkovText[username.ToLower()]);
            }

            bot.MarkovText[username] = "";
            string sentMessage = MarkovHelper.BuildString(dict, 25, true).Replace('@', ' ');



            foreach (var user in bot.JoinedUsersDict[username])
            {
                if (sentMessage.ToLower().Contains(user.ToLower()))
                {
                    sentMessage = sentMessage.Replace(user, "[[SOME USER]]", StringComparison.OrdinalIgnoreCase);
                }
            }

            bot.ConstructAndEnqueueMessage(sentMessage, username);
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
                if (File.Exists("./markov/" + username.ToLower() + ".json"))
                {
                    dict = JsonConvert.DeserializeObject<TDict>(File.ReadAllText("./markov/" + username.ToLower() + ".json"));
                }
                else
                {

                    dict = MarkovHelper.BuildTDict(bot.MarkovText[username], order);
                }
                _tDictDict.Add(username, dict);
            }

            return dict;
        }

        public void SaveTDict(string username)
        {
            var dict = _tDictDict[username];
            string json = JsonConvert.SerializeObject(dict, Formatting.Indented);

            File.WriteAllText("./markov/" + username.ToLower() + ".json", json);
        }



    }

    public static class Extensions
    {
        public static string Replace(this string source, string oldString, string newString, StringComparison comp)
        {
            int index = source.IndexOf(oldString, comp);

            // Determine if we found a match
            bool MatchFound = index >= 0;

            if (MatchFound)
            {
                // Remove the old text
                source = source.Remove(index, oldString.Length);

                // Add the replacemenet text
                source = source.Insert(index, newString);
            }

            return source;
        }
    }
}
