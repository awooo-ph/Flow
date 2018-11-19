using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodMonitor.ViewModels
{
    class ModemLog 
    {
        public enum LogTypes
        {
            AtCommand,
            Sms,
            Error
        }
        public LogTypes LogType { get; set; }
        public string Content { get; set; }
    }
}
