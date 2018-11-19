﻿using System;
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
        private SerialPort _port;
        private Task _serialReader;
        private static Modem _instance;
        public static Modem Instance => _instance ?? (_instance = new Modem());

        private Modem()
        {
            _context = SynchronizationContext.Current;
            Start();
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
                    var log = new ModemLog()
                    {
                        Content = e.Message,
                        LogType = ModemLog.LogTypes.Error
                    };
                    _log.Add(log);
                    break;
                }
                if (!string.IsNullOrEmpty(cmd))
                {
                    ProcessCommand(cmd);
                    _context.Post(l =>
                    {
                        var log = new ModemLog()
                        {
                            Content = cmd,
                            LogType = ModemLog.LogTypes.AtCommand
                        };
                        _log.Add(log);
                        Messenger.Default.Broadcast(Messages.ModemDataReceived,log);
                    },null);
                }
                //await Task.Delay(111);
            }
            OnPropertyChanged(nameof(IsOnline));
            Start();
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
            else if(command.Contains("ERROR"))
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
            else if (command.StartsWith("+CMT: ")) //New message
                ParseSms(command);
        }

        private string GetNumber(string command)
        {
            command = command.Substring(command.IndexOf("\"")+1);
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

        private void ParseSms(string command)
        {
            if (_port == null) return;
            var sms = new Sms()
            {
                Sender = GetNumber(command),
                Message = _port.ReadLine()
            };
            var sensor = Sensor.Cache.FirstOrDefault(x => x.NumberEquals(sms.Sender));
            if (sensor == null)
            {
                _context.Post(d =>
                {
                    _log.Add(new ModemLog()
                    {
                       Content = sms.ToString(),
                       LogType = ModemLog.LogTypes.Sms
                    });
                },null);
            }
            else
            {
                ParseCommand(sensor, sms.Message);
            }
            Messenger.Default.Broadcast(Messages.SmsReceived, sms);
        }

        private void ParseCommand(Sensor sensor, string command)
        {
            if (sensor==null || string.IsNullOrEmpty(command)) return;
            if (command.StartsWith(".") && command.Length>=2)
            {
                var level = 0;
                if (int.TryParse(command.Substring(1, 1), out level))
                {
                    sensor.SetLevel(level);
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
                _port.NewLine = "\r";
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
            IsBooting = true;
            var found = Start(Config.Default.ComPort);

            while (!found)
            {
                var ports = GetAllPorts();

                while (ports == null)
                {
                    await Task.Delay(1111);
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
                var commands = new List<string>()
                {
                    "AT+CNUM",
                    "AT+CSQ",
                    "AT+CREG=1",            // Network Registration
                    "AT+CSMS=1",            // Select Message Service
                    "AT+CNMI=2,2,0,0,0",    // New Message Indication
                    "AT+CMGF=1",            // Select Message Format (0:PDU; 1:TEXT)
                    "AT+CSCS=\"GSM\"",
                    "AT+CSPN?"
                };
                foreach (var command in commands)
                {
                    _port.WriteLine(command);
                    await WaitOk();
                }
            }
            IsBooting = false;
        }

        public async void SendMessage(string number, string message)
        {
            if (!IsOnline) return;

            await Task.Factory.StartNew(async ()=>
            {
                pauseReading = true;
                _port.WriteLine("AT");
                await Task.Delay(111);
                _port.WriteLine($"AT+CMGS=\"{number}\"");
                _port.ReadLine();
                _port.WriteLine(message);
                _port.Write(new[]{(char)26},0,1);
                pauseReading = false;
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
                if (UseAtCommand)
                {
                    var msg = Message.Replace("^Z",$"{(char)26}");
                    _port.WriteLine(msg);
                }
                else
                {
                    if (!SendTo.IsCellNumber()) return;
                    if (string.IsNullOrEmpty(Message)) return;
                    SendMessage(SendTo, Message);
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

        private bool _UseAtCommand;
        public bool UseAtCommand
        {
            get => _UseAtCommand;
            set
            {
                if (value == _UseAtCommand) return;
                _UseAtCommand = value;
                OnPropertyChanged(nameof(UseAtCommand));
                //Log.Refresh();
            }
        }

        private ListCollectionView _logView;
        public ListCollectionView Log
        {
            get
            {
                if (_logView != null) return _logView;
                _logView = (ListCollectionView) CollectionViewSource.GetDefaultView(_log);
                _logView.Filter = FilterLog;
                return _logView;
            }
        }

        private bool FilterLog(object obj)
        {
            if (UseAtCommand) return true;
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
