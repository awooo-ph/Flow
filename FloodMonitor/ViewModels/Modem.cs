using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace FloodMonitor.ViewModels
{
    internal class Modem : ViewModelBase, IDisposable
    {
        private SynchronizationContext _context;
        private ObservableCollection<ModemLog> _log = new ObservableCollection<ModemLog>();
        private static SerialPort _port;
        private Task _serialReader;
        private static Modem _instance;
        public static Modem Instance => _instance ?? (_instance = new Modem());

        private Modem()
        {
            _context = SynchronizationContext.Current;
            Start();
        }

        private DateTime _lastCREG = DateTime.MinValue;
        private Task _signalChecker;
        private void CheckSignal()
        {
            if (_signalChecker != null) return;
            _signalChecker = Task.Factory.StartNew(async () =>
            {
                while (HasSIM && _port != null)
                {
                    if ((DateTime.Now - _lastCREG).TotalSeconds >= 47)
                    {
                        _lastCREG = DateTime.Now;
                        if (receivedSms > 7)
                        {
                            SendAT("AT+CMGD=1,1");
                            receivedSms = 0;
                        }
                        else
                            SendAT("AT+CSQ");

                    }
                    else
                    {
                        await Task.Delay(1111);
                    }
                }

                _signalChecker = null;
            });
        }
        
        private bool pauseReading = false;
        
        private async void ReadSerial()
        {
            while (_port?.IsOpen??false)
            {
                while (pauseReading)
                {
                    await Task.Delay(111);
                }

                var cmd = "";
                try
                {
                    cmd = _port.ReadLine()?.Trim();
                }
                catch (Exception e)
                {
                    AddLog(ModemLog.LogTypes.Error, e.Message);
                    break;
                }
                if (!string.IsNullOrEmpty(cmd))
                {
                    ProcessCommand(cmd);
                    AddLog(ModemLog.LogTypes.AtCommand, cmd);
                }
                //await Task.Delay(111);
            }
            OnPropertyChanged(nameof(IsOnline));
            Start();
        }

        public void AddLog(ModemLog.LogTypes type, string content)
        {
            content = content.Replace("https://goo.gl/WD3Kka", "")
                             .Replace("https://goo.gl/RBy5eb",""); //hide
            if (string.IsNullOrEmpty(content)) return;
            var log = new ModemLog()
            {
                Content = content,
                LogType = type
            };
            _context.Post(l =>_log.Add(log),null);
            Messenger.Default.Broadcast(Messages.ModemDataReceived,log);
        }

        private bool _IsBooting;

        public bool IsBooting
        {
            get =>_IsBooting ||((_port?.IsOpen??false) && !IsOnline);
            set
            {
                if (value == _IsBooting) return;
                _IsBooting = value;
                OnPropertyChanged(nameof(IsBooting));
            }
        }
        
        private DateTime _startCheck;
        private bool _checking = false;
        public async void Check()
        {
            if (_port == null) return;
            if (_checking) return;
            _checking = true;
            _okReceived = false;
            _startCheck = DateTime.Now;
            _port.WriteLine("AT");
            await Task.Factory.StartNew(async () =>
            {
                while (!_okReceived && (DateTime.Now-_startCheck).TotalMilliseconds<4444)
                {
                    await Task.Delay(111);
                }

                IsOnline = _okReceived;
                _checking = false;

            });
        }

        private bool _okReceived = false;
        private void ProcessCommand(string command)
        {
            if (_port == null) return;

            if (command == "OK")
            {
                _okReceived = true;
                IsOnline = true;
                return;
            }
            if(command.Contains("ERROR"))
            {
                _okReceived = false;
                return;
            }
            if (command.StartsWith("+CSQ:"))
            {
                var csq = command.Substring(5, command.IndexOf(",") - 5);
                var iCsq = 0;
                if (!int.TryParse(csq, out iCsq))
                    Signal = 0;
                else
                {    
                    if (iCsq == 99 || iCsq==0) Signal = 0;
                    else if (iCsq < 7) Signal =1;
                    else if (iCsq < 14) Signal = 2;
                    else if (iCsq < 20) Signal = 3;
                    else Signal = 4;
                }
            } else if (command.StartsWith("+CNUM"))
            {
                command = command.Substring(command.IndexOf(",\"") + 2);
                Number = command.Substring(0, command.IndexOf("\","));
            } else if (command.StartsWith("+CSPN:"))
            {
                Operator = command.Substring(8, command.IndexOf("\",") - 8);
            } else if (command.StartsWith("+CLIP:"))
                _port.WriteLine("ATH");
            else if (command.StartsWith("+CMT:")) //New message
                ParseSms(command);
            else if (command.StartsWith("+CMTI:"))
            {
                var index = command.Substring(command.IndexOf(",") + 1);
                _port.WriteLine($"AT+CMGR={index}");
            }
        }

        private string GetNumber(string command)
        {
            if (command.StartsWith("+CMT:"))
            {
                command = command.Substring(command.IndexOf("\"") + 1);
            }
            else if (command.StartsWith("+CMGR:"))
            {
                command = command.Substring(command.IndexOf(",\"") + 2);
            }

            return command.Substring(0, command.IndexOf("\""));
        }

        public class Sms
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public override string ToString()
            {
                return $"Message received from {Sender}\r{Message.Trim()}";
            }
        }

        private Task _atSender;
        private object _atCommandsLock = new object();
        private Queue<Tuple<string,int>> _atCommands = new Queue<Tuple<string,int>>();
        private void SendAT(string at,int wait=0)
        {
            lock (_atCommandsLock)
                _atCommands.Enqueue(new Tuple<string, int>(at,wait));

            if (_atSender != null) return;
            _atSender = Task.Factory.StartNew(async () =>
            {
                while (_port!=null)
                {
                    Tuple<string,int> cmd = null;
                    lock (_atCommandsLock)
                    {
                        try
                        {
                            if(_atCommands.Count>0) cmd = _atCommands.Dequeue();
                        }
                        catch (Exception e)
                        {
                            //
                        }
                    }

                    if (cmd==null)
                    {
                        await Task.Delay(777);
                        continue;
                    }

                    while (_sendingMessage)
                    {
                        await Task.Delay(777);
                    }

                    _port?.WriteLine(cmd.Item1);
                    if (cmd.Item2 > 0)
                        await Task.Delay(cmd.Item2);
                }

                _atSender = null;
            });
        }

        private int receivedSms = 0;
        private async void ParseSms(string command)
        {
            if (_port == null) return;
            var msg = _port.ReadLine()?.Trim();
            
            var sms = new Sms()
            {
                Sender = GetNumber(command),
                Message = msg
            };

            var sensor = Sensor.Cache.FirstOrDefault(x => x.NumberEquals(sms.Sender));
            if (sensor == null)
            {
                AddLog(ModemLog.LogTypes.Sms, sms.ToString());
            }
            else
            {
                ParseCommand(sensor, sms.Message);
            }
            receivedSms++;
        }

        private void ParseCommand(Sensor sensor, string command)
        {
            if (sensor==null || string.IsNullOrEmpty(command)) return;
            if(command == "==")
                sensor.Update(nameof(sensor.SettingsSaved),true);
            else if (command == "https://goo.gl/RBy5eb")
            {
                sensor.Update(nameof(sensor.Verified),true);
            } else if (command.StartsWith(".") && command.Length>=2)
            {
                var level = 0;
                if (int.TryParse(command.Substring(1, 1), out level))
                {
                    sensor.SetLevel(level);
                    AddLog(ModemLog.LogTypes.WaterLevel, $"{sensor.SensorName.ToUpper()}: LEVEL {level}");
                }
            }
        }

        private async Task<bool> WaitOk()
        {
            _okReceived = false;
            for (var i = 0; i < 7; i++)
            {
                if(_okReceived) return true;
                await Task.Delay(777);
            }

            return _okReceived;
        }

        private bool Start(string port)
        {
            try
            {
                _port = new SerialPort(port,115200);
                _port.NewLine = "\r\n";
                _port.DtrEnable = false;
                _port.Open();
                        
                _serialReader = Task.Factory.StartNew(ReadSerial);
                return true;
            }
            catch (Exception )
            {
                return false;
            }
        }

        public async void Start()
        {
            AddLog(ModemLog.LogTypes.Info, "Initializing modem...");
            IsBooting = true;
            var found = Start(Config.Default.ComPort);

            while (!found)
            {
                var ports = GetAllPorts();

                while (ports == null)
                {
                    await Task.Delay(7777);
                    ports = GetAllPorts();
                }

                foreach (var port in ports)
                {
                    found = Start(port);
                    if (found)
                    {
                        Config.Default.ComPort = port;
                        break;
                    }
                }
            }

            if (_port == null)
            {
                return;
            }

            await Task.Delay(1111);
            
            while (!IsOnline)
            {
                _port.WriteLine("AT");
                await WaitOk();
            }

            _port.WriteLine("AT+CPIN?");
            HasSIM = await WaitOk();
            if (HasSIM)
            {
                var commands = new List<Tuple<string,string>>()
                {
                    new Tuple<string, string>("AT+CMGD=1,1","Clearing inbox"),
                    new Tuple<string, string>("AT+CNUM","Getting SIM card number"),
                    new Tuple<string, string>("AT+CSMS=1","Setting up SMS Service"),
                    new Tuple<string, string>("AT+CNMI=2,2,0,0,0",""),    // New message indication
                    new Tuple<string, string>("AT+CMGF=1",""),            // Select message format (0:PDU; 1:TEXT)
                    new Tuple<string, string>("AT+CSCS=\"GSM\"",""),
                    new Tuple<string, string>("AT+CSPN?",""),
                };
                foreach (var command in commands)
                {
                    AddLog(ModemLog.LogTypes.Info, command.Item2);
                    _port.WriteLine(command.Item1);
                    await WaitOk();
                }
                CheckSignal();
            }
            IsBooting = false;
            AddLog(ModemLog.LogTypes.Info, "Modem initialized");
        }

        private bool _sendingMessage;
        public async void SendMessage(string number, string message, bool log=true)
        {
            if (!IsOnline) return;
            if(!Config.Default.ShowAtCommand) AddLog(ModemLog.LogTypes.Info, $"Sending message...");
            await Task.Factory.StartNew(async ()=>
            {
                _sendingMessage = true;
                _port.WriteLine("AT");
                await Task.Delay(111);
                _port.WriteLine($"AT+CMGS=\"{number}\"");
                await Task.Delay(1111);
                _port.WriteLine(message);
                _port.Write($"{(char)26}");
                _sendingMessage = false;

                if(!Config.Default.ShowAtCommand && log) 
                if(await WaitOk())
                    AddLog(ModemLog.LogTypes.Info, "Message Sent!");
                else
                    AddLog(ModemLog.LogTypes.Info, "Sending Message Failed!");
            });
        }
        
        private string _Operator;

        public string Operator
        {
            get => _Operator;
            set
            {
                if (value == _Operator) return;
                _Operator = value;
                OnPropertyChanged(nameof(Operator));
            }
        }
        
        private bool _HasSIM;

        public bool HasSIM
        {
            get => _HasSIM;
            set
            {
                if (value == _HasSIM) return;
                _HasSIM = value;
                OnPropertyChanged(nameof(HasSIM));
            }
        }

        private ICommand _SendMessageCommand;

        public ICommand SendMessageCommand =>
            _SendMessageCommand ?? (_SendMessageCommand = new DelegateCommand(d =>
            {
                if (Config.Default.ShowAtCommand)
                {
                    var msg = Message.Replace("^Z",$"{(char)26}");
                    SendAT(msg);
                }
                else if(Config.Default.ShowSms)
                {
                    if (!SendTo.IsCellNumber()) return;
                    if (string.IsNullOrEmpty(Message)) return;
                    SendMessage(SendTo, Message);
                } else if (Config.Default.ShowUssd)
                {
                    var msg = Message.Replace("*", "#");
                    if (!msg.StartsWith("#"))
                        msg = $"#{Message}";
                    SendAT($"AT+CUSD=1,\"{msg}\",15");
                }

                Message = "";
            },d=>!IsBooting));

        private string _Message;

        public string Message
        {
            get => _Message;
            set
            {
                if (value == _Message) return;
                _Message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private string _SendTo;

        public string SendTo
        {
            get => _SendTo;
            set
            {
                if (value == _SendTo) return;
                _SendTo = value;
                OnPropertyChanged(nameof(SendTo));
            }
        }

        private string _AtCommand;

        public string AtCommand
        {
            get => _AtCommand;
            set
            {
                if (value == _AtCommand) return;
                _AtCommand = value;
                OnPropertyChanged(nameof(AtCommand));
            }
        }
        
        private ICommand _sendAtCommand;
        public ICommand SendAtCommand => _sendAtCommand ?? (_sendAtCommand = new DelegateCommand(d =>
        {
            _port.WriteLine(AtCommand);
            AtCommand = "";
        },d=>(_port?.IsOpen??false) && !string.IsNullOrEmpty(AtCommand)));

       

        private ListCollectionView _logView;
        public ListCollectionView Log
        {
            get
            {
                if (_logView != null) return _logView;
                _logView = (ListCollectionView) CollectionViewSource.GetDefaultView(_log);
                _logView.Filter = FilterLog;
                Messenger.Default.AddListener(Messages.SettingsChanged, () =>
                {
                    _logView.Refresh();
                    _logView.MoveCurrentToLast();
                });
                return _logView;
            }
        }

        private bool FilterLog(object obj)
        {
            if (Config.Default.ShowAdvanceLog) return true;
            if (!(obj is ModemLog l)) return false;
            return l.LogType != ModemLog.LogTypes.AtCommand;
        }

        public IEnumerable<string> AllPorts => GetAllPorts();

        public IEnumerable<string> GetAllPorts()
        {
            try
            {
                var list = new List<string>();
                var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSSerial_PortName");
                foreach (var queryObj in searcher.Get())
                    if (queryObj["InstanceName"].ToString().Contains("USB"))
                        list.Add($"{queryObj["PortName"]}");
                return list;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand => _refreshCommand ?? (_refreshCommand = new DelegateCommand(d =>
        {
            OnPropertyChanged(nameof(AllPorts));
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

        private bool _IsOnline;

        public bool IsOnline
        {
            get => _IsOnline && _port!=null;
            set
            {
                if (value == _IsOnline) return;
                _IsOnline = value;
                OnPropertyChanged(nameof(IsOnline));
                OnPropertyChanged(nameof(IsBooting));
            }
        }

        private int _Signal;

        public int Signal
        {
            get => _Signal;
            set
            {
                if (value == _Signal) return;
                _Signal = value;
                OnPropertyChanged(nameof(Signal));
            }
        }


        public void Dispose()
        {
            _port?.Close();
            _serialReader?.Dispose();
            _port?.Dispose();
        }
    }
}
