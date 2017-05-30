using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    class BitsCheerCommand : Command

    {
        public string CommandName { get => "Cheer"; set => throw new NotImplementedException(); }
        public bool RequireMod { get => false; set => throw new NotImplementedException(); }

        public bool Execute(FluzzBot bot, string message)
        {
            bot.ConstructAndEnqueueMessage("kadCheer kadCheer THANKS FOR THE BITTIES kadCheer kadCheer");
            return true;
        }
    }
}
