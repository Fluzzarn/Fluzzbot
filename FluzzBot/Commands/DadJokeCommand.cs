using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace FluzzBot.Commands
{
    class DadJokeCommand : Command, ICommand 
    {
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }
        public bool HasCooldown { get => _hasCooldown; set => _hasCooldown = value; }
        public int Cooldown { get => _cooldown; set => _cooldown = value; }
        public bool OnCooldown { get => _onCooldown; set => _onCooldown = value; }
       


        public DadJokeCommand() : base()
        {
            _commandName = "!dad";
            _hasCooldown = true;
            _cooldown = 5;
            
        }

        public bool Execute(FluzzBot bot, string message,string username)
        {

                var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

                var response = client.GetAsync("https://icanhazdadjoke.com/").Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(response.Content);
                    bot.ConstructAndEnqueueMessage(response.Content.ReadAsStringAsync().Result,username);
                }


            return true;
        }


    }
}
