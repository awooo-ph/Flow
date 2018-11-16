using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Jot;
using Jot.DefaultInitializer;
using Jot.Storage;
using Jot.Triggers;
using LiveCharts;
using LiveCharts.Configurations;

namespace FloodMonitor.ViewModels
{
    class Sensor:ModelBase<Sensor>
    {
       
        private static ObservableCollection<Sensor> _AllSensors;
        
        private static ListCollectionView _AllView;

        public static ListCollectionView AllView
        {
            get
            {
                if (_AllView != null) return _AllView;
                _AllView = (ListCollectionView) CollectionViewSource.GetDefaultView(Cache);
                _AllView.LiveFilteringProperties.Add(nameof(IsDeleted));
                _AllView.IsLiveFiltering = true;
                _AllView.SortDescriptions.Add(new SortDescription(nameof(SensorName), ListSortDirection.Ascending));
                _AllView.Filter = o => !((Sensor) o).IsDeleted;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        foreach (var sensor in Cache)
                        {
                            sensor.OnPropertyChanged(nameof(LastHeartBeatText));
                        }
                        await Task.Delay(1111);    
                    }
                });
                return _AllView;
            }
        }
        
        private static ICommand _AddCommand;
        public static ICommand AddCommand => _AddCommand ?? (_AddCommand = new DelegateCommand(async d =>
        {
            var sensor = await NewSensorDialog.Show();
            if (sensor == null) return;
            sensor.Save();
        }));

        private string _Number;
        public string Number
        {
            get => _Number;
            set
            {
                if (value == _Number) return;
                _Number = value;
                OnPropertyChanged(nameof(Number));
            }
        }
        
        private int _WaterLevel;
        public int WaterLevel
        {
            get => _WaterLevel;
            set
            {
                if (value == _WaterLevel) return;
                _WaterLevel = value;
                OnPropertyChanged(nameof(WaterLevel));
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Sensor s)) return false;
            var n1 = s.Number.StartsWith("+") ? s.Number.Substring(2) : s.Number.Substring(1);
            var n2 = Number.StartsWith("+") ? Number.Substring(2) : Number.Substring(1);
            return n1 == n2;
        }

        private DateTime _LastHeartBeat;
        public DateTime LastHeartBeat
        {
            get => _LastHeartBeat;
            set
            {
                if (value == _LastHeartBeat) return;
                _LastHeartBeat = value;
                OnPropertyChanged(nameof(LastHeartBeat));
            }
        }

        public Sensor()
        {
            var mapper = Mappers.Xy<WaterLevel>().X(x => x.DateTime.Ticks).Y(x => x.Level);
            Charting.For<WaterLevel>(mapper);
            LabelFormatter = v => new DateTime((long)v).ToString("M/d/yy");
        }

        private ChartValues<WaterLevel> _waterLevels;

        public ChartValues<WaterLevel> WaterLevels
        {
            get
            {
                if (_waterLevels != null) return _waterLevels;
                _waterLevels = new ChartValues<WaterLevel>(ViewModels.WaterLevel.Cache.Where(x=>x.SensorId == Id));
                return _waterLevels;
            }
        }

        [Ignore]
        public Func<double, string> LabelFormatter { get; set; }

        private string _LastHeartBeatText;
        [Ignore]
        public string LastHeartBeatText
        {
            get
            {
                if (LastHeartBeat == DateTime.MinValue) return "NEVER";
                var elapsed = DateTime.Now - LastHeartBeat;
                if (elapsed.TotalMinutes < 1) return $"{elapsed.Seconds} sec ago";
                if (elapsed.TotalHours < 1) return $"{elapsed.Minutes} min ago";
                if (elapsed.TotalDays < 1) return $"{elapsed.Days} hr ago";
                return $"{elapsed.Days} days ago";
            }
            set
            {
                if (value == _LastHeartBeatText) return;
                _LastHeartBeatText = value;
                OnPropertyChanged(nameof(LastHeartBeatText));
            }
        }
        
        private string _SensorName;
        public string SensorName
        {
            get => _SensorName;
            set
            {
                if (value == _SensorName) return;
                _SensorName = value;
                OnPropertyChanged(nameof(SensorName));
            }
        }

        private string _Location;
        public string Location
        {
            get => _Location;
            set
            {
                if (value == _Location) return;
                _Location = value;
                OnPropertyChanged(nameof(Location));
            }
        }
        
        protected override bool GetIsEmpty()
        {
            return false;
        }
    }
}
