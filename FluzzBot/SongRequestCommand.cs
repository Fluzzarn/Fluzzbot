using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class SongRequestCommand : Command
    {
        private string _commandName = "request";
        public string CommandName { get => _commandName; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot,string message)
        {
            bot.EnqueueMessage(" Added to Request");
            return true;
        }
    }
}
