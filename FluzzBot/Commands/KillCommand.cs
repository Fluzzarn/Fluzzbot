using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class KillCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown = value; }
        public int Cooldown { get => _cooldown; set => _cooldown = value; }


        public KillCommand()
        {
            _commandName = "!kill";
            _hasCooldown = true;
            _cooldown = 5;
        }
        public bool Execute(FluzzBot bot, string message, string username)
        {
            string toKill = "";
            if (message.Split(' ').Length > 1)
            {
                toKill = message.Split(' ')[1];
            }
            else
            {
                return false;
            }

            string article = "";

            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = client.GetAsync("https://en.wikipedia.org/w/api.php?format=json&action=query&generator=random&grnnamespace=0&prop=revisions|images&rvprop=content&grnlimit=1").Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                int startIndex = result.IndexOf("\"title\":\"");
                article = result.Substring(startIndex + "\"title\":\"".Length, result.IndexOf("\",\"revisions",startIndex) - startIndex - 9);

                bot.ConstructAndEnqueueMessage(toKill + " was killed by " + article, username);
            }
            return true;
        }
    }
}
