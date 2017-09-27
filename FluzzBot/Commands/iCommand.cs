using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluzzBot
{
    public interface ICommand
    {


        
        bool AutoFire { get; set; }
        string CommandName { get; set; }
        bool RequireMod { get;  set; }
        bool HasCooldown { get; set; }
        int Cooldown { get; set; }
        bool Execute(FluzzBot bot, string message, string username);
    }
}
