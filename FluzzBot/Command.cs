using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    public interface Command
    {

        string CommandName { get; set; }
        bool RequireMod { get;  set; }
        bool Execute(FluzzBot bot, string message);
    }
}
