using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
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
                _AllView.LiveSortingProperties.Add(nameof(SensorName));
                _AllView.Filter = o => !((Sensor) o).IsDeleted;

                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        foreach (var sensor in Cache)
                        {
                            sensor.OnPropertyChanged(nameof(LastHeartBeatText));
                        }
                        await Task.Delay(7777);    
                    }
                });
                return _AllView;
            }
        }

        protected override string GetErrorInfo(string prop)
        {
            if (prop == nameof(Number))
            {
                if (string.IsNullOrEmpty(Number)) return "REQUIRED";
                if (!Number.IsCellNumber()) return "INVALID NUMBER";
            } else if (prop == nameof(SensorName) && string.IsNullOrEmpty(SensorName))
                return "REQUIRED";

            return base.GetErrorInfo(prop);
        }

        public override string ToString()
        {
            return Number;
        }

        private static ICommand _AddCommand;
        public static ICommand AddCommand => _AddCommand ?? (_AddCommand = new DelegateCommand(async d =>
        {
            var sensor = await NewSensorDialog.Show();
            if (sensor == null) return;

            var oldSensor = Cache.FirstOrDefault(x => x.Equals(sensor));
            if (oldSensor != null)
            {
                if (oldSensor.IsDeleted)
                {
                    oldSensor.Undelete();
                    oldSensor.SensorName = sensor.SensorName;
                    oldSensor.Location = sensor.Location;
                    oldSensor.Save();
                }
                return;
            }

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

        private int _WarningLevel;

        public int WarningLevel
        {
            get => _WarningLevel;
            set
            {
                if (value == _WarningLevel) return;
                _WarningLevel = value;
                OnPropertyChanged(nameof(WarningLevel));
                if(Id>0)
                    Save();
                OnPropertyChanged(nameof(IsWarning));
            }
        }

        public bool IsWarning => WaterLevel >= WarningLevel;
        
        private int _WaterLevel;
        public int WaterLevel
        {
            get => _WaterLevel;
            set
            {
                if (value == _WaterLevel) return;
                _WaterLevel = value;
                OnPropertyChanged(nameof(WaterLevel));
                OnPropertyChanged(nameof(IsWarning));
            }
        }

        public bool NumberEquals(string number)
        {
            if (number.Length < 2) return false;
            var n1 = number.StartsWith("+") ? number.Substring(3) : number.Substring(1);
            var n2 = Number.StartsWith("+") ? Number.Substring(3) : Number.Substring(1);
            return n1 == n2;
        }

        public override async void Delete()
        {
            if(await MessageDialog.Show("Are you sure you want to delete this sensor?","","_YES","_CANCEL"))
                base.Delete();
        }

        private ICommand _EditCommand;
        public ICommand EditCommand => _EditCommand ?? (_EditCommand = new DelegateCommand(async d =>
        {
            var sensor = await NewSensorDialog.Show(this);
            sensor?.Save();
        },d=>Id>0));

        public override bool Equals(object obj)
        {
            if (!(obj is Sensor s)) return false;
            return NumberEquals(s.Number);
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
            var mapper = Mappers.Xy<WaterLevel>().X(x => x.Id).Y(x => x.Level);
            Charting.For<WaterLevel>(mapper);
            LabelFormatter = v =>
            {
                if ((int) v == 0) return "";
                return _levels.Skip((int)v).FirstOrDefault()?.DateTime.ToString("MMM d h:mm tt");
                //return ViewModels.WaterLevel.GetById((long) v)?.DateTime.ToShortDateString();
                //return new DateTime((long) v).ToString("M/d/yy h:m A");
            };
        }

        private ChartValues<int> _waterLevels;
        private List<WaterLevel> _levels;// = new List<WaterLevel>();
        public ChartValues<int> WaterLevels
        {
            get
            {
                if (_waterLevels != null) return _waterLevels;
                _levels = ViewModels.WaterLevel.Cache.Where(x => x.SensorId == Id).ToList();
                _waterLevels = new ChartValues<int>(_levels.Select(x=>x.Level));
                if (_waterLevels.Count == 0)
                {
                    SetLevel(0);
                    SetLevel(0);
                }
                return _waterLevels;
            }
        }

        [Ignore]
        public Func<double, string> LabelFormatter { get; set; }

        //[Ignore]
        //public long ChartStep
        //{
        //    get
        //    {
        //        if (WaterLevels.Count > 0)
        //        {
        //            return TimeSpan.FromMinutes(30).Ticks;
        //        }
        //        return 1;
        //    }
        //} 

        private string _LastHeartBeatText;
        [Ignore]
        public string LastHeartBeatText
        {
            get
            {
                if (LastHeartBeat == DateTime.MinValue) return "NEVER";
                var elapsed = DateTime.Now - LastHeartBeat;
                if (elapsed.TotalMinutes < 2) return $"{(long)elapsed.TotalSeconds} seconds";
                if (elapsed.TotalHours < 2) return $"{(long)elapsed.TotalMinutes} minutes";
                if (elapsed.TotalDays < 2) return $"{(long) elapsed.TotalHours} hours";
                return $"{(long)elapsed.TotalDays} days ago";
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

        public void SetLevel(int level)
        {
            awooo.Context.Post(d=>{
                var newLevel = new WaterLevel()
                {
                    Level = level,
                    SensorId = Id
                };
                newLevel.Save();
                WaterLevel = level;
                LastHeartBeat = DateTime.Now;
                Save();
                //WaterLevels.Add(newLevel);
                _levels.Add(newLevel);
                WaterLevels.Add(level);
                //OnPropertyChanged(nameof(ChartStep));
            },null);
        }
    }
}
