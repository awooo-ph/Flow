using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private bool _showAtCommand;
        [Trackable]
        public bool ShowAtCommand
        {
            get => _showAtCommand;
            set
            {
                if (value == _showAtCommand) return;
                _showAtCommand = value;
                _ShowSms = false;
                _ShowUssd = false;
                OnPropertyChanged(nameof(ShowSms));
                OnPropertyChanged(nameof(ShowUssd));
                OnPropertyChanged(nameof(ShowAtCommand));
                //Log.Refresh();
            }
        }

        private bool _ShowUssd;
        [Trackable]
        public bool ShowUssd
        {
            get => _ShowUssd;
            set
            {
                if (value == _ShowUssd) return;
                _ShowUssd = value;
                _showAtCommand = false;
                _ShowSms = false;
                OnPropertyChanged(nameof(ShowSms));
                OnPropertyChanged(nameof(ShowAtCommand));
                OnPropertyChanged(nameof(ShowUssd));
            }
        }

        private bool _ShowSms;
        [Trackable]
        public bool ShowSms
        {
            get => _ShowSms;
            set
            {
                if (value == _ShowSms) return;
                _ShowSms = value;
                _showAtCommand = false;
                _ShowUssd = false;
                OnPropertyChanged(nameof(ShowUssd));
                OnPropertyChanged(nameof(ShowAtCommand));
                OnPropertyChanged(nameof(ShowSms));
            }
        }

        private bool _ShowAdvanceLog;
        [Trackable]
        public bool ShowAdvanceLog
        {
            get => _ShowAdvanceLog;
            set
            {
                if (value == _ShowAdvanceLog) return;
                _ShowAdvanceLog = value;
                OnPropertyChanged(nameof(ShowAdvanceLog));
                Messenger.Default.Broadcast(Messages.SettingsChanged);
            }
        }

        public static string GetMessage(int level)
        {
            if (!Default.SendSms) return null;
            switch (level)
            {
                    case 1:
                        return Default.Level1Message;
                    case 2:
                        return Default.Level2Message;
                    case 3:
                        return Default.Level3Message;
                    case 4:
                        return Default.Level4Message;
                    case 5:
                        return Default.Level5Message;
            }

            return null;
        }

        private string _Level1Message;
        [Trackable]
        public string Level1Message
        {
            get => _Level1Message;
            set
            {
                if (value == _Level1Message) return;
                _Level1Message = value;
                OnPropertyChanged(nameof(Level1Message));
            }
        }

        private string _Level2Message;
        [Trackable]
        public string Level2Message
        {
            get => _Level2Message;
            set
            {
                if (value == _Level2Message) return;
                _Level2Message = value;
                OnPropertyChanged(nameof(Level2Message));
            }
        }

        private string _Level3Message;
        [Trackable]
        public string Level3Message
        {
            get => _Level3Message;
            set
            {
                if (value == _Level3Message) return;
                _Level3Message = value;
                OnPropertyChanged(nameof(Level3Message));
            }
        }

        private string _Level4Message;
        [Trackable]
        public string Level4Message
        {
            get => _Level4Message;
            set
            {
                if (value == _Level4Message) return;
                _Level4Message = value;
                OnPropertyChanged(nameof(Level4Message));
            }
        }

        private string _Level5Message;
        [Trackable]
        public string Level5Message
        {
            get => _Level5Message;
            set
            {
                if (value == _Level5Message) return;
                _Level5Message = value;
                OnPropertyChanged(nameof(Level5Message));
            }
        }

        private bool _SendSms;
        [Trackable]
        public bool SendSms
        {
            get => _SendSms;
            set
            {
                if (value == _SendSms) return;
                _SendSms = value;
                OnPropertyChanged(nameof(SendSms));
            }
        }




        private ICommand _ToggleLogCommand;

        public ICommand ToggleLogCommand => _ToggleLogCommand ?? (_ToggleLogCommand = new DelegateCommand(d =>
        {
            ShowAdvanceLog = !ShowAdvanceLog;
        }));

    }
}
