using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot.Commands
{
    class NotTheOnionNewsCommand : Command, ICommand
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => _requireMod; set => throw new NotImplementedException(); }
        public bool HasCooldown { get =>_hasCooldown; set => throw new NotImplementedException(); }
        public int Cooldown { get => _cooldown; set => throw new NotImplementedException(); }


        public NotTheOnionNewsCommand()
        {
            _commandName = "!news";
            _requireMod = false;
            _hasCooldown = true;
            _cooldown = 1800;
        }

        public bool Execute(FluzzBot bot, string message, string username)
        {
            var client = new HttpClient();
           // client.DefaultRequestHeaders.Add("User-Agent", "windows:fluzzbot:1.0.0 (by /u/fluzzarn)");
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

            var response = client.GetAsync("https://www.reddit.com/r/nottheonion/new/.json?limit=1").Result;
            if (response.IsSuccessStatusCode)
            {
                var result =response.Content.ReadAsStringAsync().Result;
                int startIndex = result.IndexOf("\"title\":");
                int endIndex = result.IndexOf("\"created_utc", startIndex);
               string  article = result.Substring(startIndex + "\"title\":\"".Length,endIndex -startIndex - 11);

                startIndex = result.IndexOf("\"created\":");
                startIndex = result.IndexOf("\"url\":",startIndex);
                endIndex = result.IndexOf("\"author_flair_text", startIndex);
                string url = result.Substring(startIndex + "\"url\":\"".Length, endIndex - startIndex - "\"author_flair_text".Length +8);
                bot.ConstructAndEnqueueMessage(article + " (" +url + " )" , username);
            }
            return true;
        }
    }
}
