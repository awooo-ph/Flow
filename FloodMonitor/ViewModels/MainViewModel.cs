using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace FloodMonitor.ViewModels
{
    class MainViewModel:ViewModelBase
    {
        private MainViewModel(){}

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

        private ICommand _ShowSensorSettingsCommand;

        public ICommand ShowSensorSettingsCommand =>
            _ShowSensorSettingsCommand ?? (_ShowSensorSettingsCommand = new DelegateCommand<Sensor>(
                d =>
                {
                    SelectedSensor = d;
                    ShowSensorSettings = true;
                }));

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
