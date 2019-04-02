using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace FloodMonitor.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        private MainViewModel()
        {
            Monitor.Cache.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in args.OldItems)
                    {
                        (item as Monitor)?.Delete();
                    }
                }
            };
        }

        private static MainViewModel _instance;
        public static MainViewModel Instance => _instance ?? (_instance = new MainViewModel());

        private bool _ShowSensorSettings;

        public bool ShowSensorSettings
        {
            get => _ShowSensorSettings;
            set
            {
                if (value == _ShowSensorSettings) return;
                _ShowSensorSettings = value;
                OnPropertyChanged(nameof(ShowSensorSettings));
            }
        }

        private bool _ShowLogin = true;

        public bool ShowLogin
        {
            get => _ShowLogin;
            set
            {
                if (value == _ShowLogin) return;
                _ShowLogin = value;
                OnPropertyChanged(nameof(ShowLogin));
            }
        }

        private User _CurrentUser;

        public User CurrentUser
        {
            get => _CurrentUser;
            set
            {
                if (value == _CurrentUser) return;
                _CurrentUser = value;
                OnPropertyChanged(nameof(CurrentUser));
                ShowLogin = value == null;
            }
        }

        private bool _ShowSensors = true;

        public bool ShowSensors
        {
            get => _ShowSensors;
            set
            {
                if (value == _ShowSensors) return;
                _ShowSensors = value;
                OnPropertyChanged(nameof(ShowSensors));
            }
        }

        private bool _ShowUsers;

        public bool ShowUsers
        {
            get => _ShowUsers;
            set
            {
                if (value == _ShowUsers) return;
                _ShowUsers = value;
                OnPropertyChanged(nameof(ShowUsers));
            }
        }

        private bool _ShowResidents;

        public bool ShowResidents
        {
            get => _ShowResidents;
            set
            {
                if (value == _ShowResidents) return;
                _ShowResidents = value;
                OnPropertyChanged(nameof(ShowResidents));
            }
        }

        private ICommand _resetPasswordCommand;

        public ICommand ResetPasswordCommand => _resetPasswordCommand ?? (_resetPasswordCommand = new DelegateCommand(
                                                    d =>CurrentUser.Update(nameof(CurrentUser.Password),""),
                                                    d=>CurrentUser!=null && !string.IsNullOrEmpty(CurrentUser.Password)));

        private ICommand _logoutCommand;
        public ICommand LogoutCommand => _logoutCommand ?? (_logoutCommand = new DelegateCommand(d =>CurrentUser = null));
        
        private ICommand _ShowSensorSettingsCommand;

        public ICommand ShowSensorSettingsCommand =>
            _ShowSensorSettingsCommand ?? (_ShowSensorSettingsCommand = new DelegateCommand<Sensor>(
                d =>
                {
                    SelectedSensor = d;
                    ShowSensorSettings = true;
                }));

        private ICommand _deleteMonitorCommand;

        public ICommand DeleteMonitorCommand => _deleteMonitorCommand ?? (_deleteMonitorCommand = new DelegateCommand<Monitor>(
        async d =>
        {
            if (!(d is Monitor m)) return;

            if (!await MessageDialog.Show("Confirm Delete",
                $"Are you sure you want to delete {m.Name}?",
                "_DELETE RESIDENT", "CANCEL")) return;

            m.Delete();
        }, d =>
        {
            if (!(d is Monitor m)) return false;
            return CurrentUser?.IsAdmin??false;
        }));

        private ICommand _deleteUserCommand;

        public ICommand DeleteUserCommand => _deleteUserCommand ?? (_deleteUserCommand = new DelegateCommand<User>(
        async d =>
        {
            if (!(d is User m)) return;

            if (!await MessageDialog.Show("Confirm Delete",
                $"Are you sure you want to delete {m.Fullname}?",
                "_DELETE USER", "CANCEL")) return;

            m.Delete();
        }, d =>
        {
            if (!(d is User m)) return false;
            return CurrentUser?.IsAdmin ?? false;
        }));

        private ICommand _ShowSettingsCommand;

        public ICommand ShowSettingsCommand =>
            _ShowSettingsCommand ?? (_ShowSettingsCommand = new DelegateCommand(d =>
            {

            }));

        private bool _ShowSettings;

        public bool ShowSettings
        {
            get => _ShowSettings;
            set
            {
                if (value == _ShowSettings) return;
                _ShowSettings = value;
                OnPropertyChanged(nameof(ShowSettings));
            }
        }



        private ICommand _HideSensorSettingsCommand;

        public ICommand HideSensorSettingsCommand =>
            _HideSensorSettingsCommand ?? (_HideSensorSettingsCommand = new DelegateCommand(
                d => { ShowSensorSettings = false; }));

        private Sensor _SelectedSensor;

        public Sensor SelectedSensor
        {
            get => _SelectedSensor;
            set
            {
                if (value == _SelectedSensor) return;
                _SelectedSensor = value;
                OnPropertyChanged(nameof(SelectedSensor));
            }
        }

        private SnackbarMessageQueue _messageQueue;

        public SnackbarMessageQueue MessageQueue =>
            _messageQueue ?? (_messageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(7)));

        public void Notify(string message)
        {
            MessageQueue.Enqueue(message);
        }
    }
}
