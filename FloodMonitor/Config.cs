using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FloodMonitor.ViewModels;
using Jot;
using Jot.DefaultInitializer;
using Jot.Storage;
using Jot.Triggers;

namespace FloodMonitor
{
    class Config : ViewModelBase
    {
        private static Config _default;
        public static Config Default => _default ?? (_default = new Config("Default"));

        public static StateTracker Tracker { get; } = new StateTracker(new JsonFileStoreFactory("."), new DesktopPersistTrigger());

        public Config()
        {
            Tracker.Configure(this).Apply();
        }

        public Config(string name)
        {
            Tracker.Configure(this)
                .IdentifyAs(name, NamingScheme.KeyOnly)
                .Apply();
        }

        private string _ComPort;
        [Trackable]
        public string ComPort
        {
            get => string.IsNullOrEmpty(_ComPort) ? "N/A" : _ComPort;
            set
            {
                if (value == _ComPort) return;
                _ComPort = value;
                OnPropertyChanged(nameof(ComPort));
            }
        }


    }
}
