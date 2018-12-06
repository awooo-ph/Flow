using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodMonitor.ViewModels
{
    class Monitor:ModelBase<Monitor>
    {
        private string _Number;
        [Required]
        [Unique]
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

        private string _Name;
        [Required]
        public string Name
        {
            get => _Name;
            set
            {
                if (value == _Name) return;
                _Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private string _Address;

        public string Address
        {
            get => _Address;
            set
            {
                if (value == _Address) return;
                _Address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private string _Position;

        public string Position
        {
            get => _Position;
            set
            {
                if (value == _Position) return;
                _Position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        private string _Remarks;

        public string Remarks
        {
            get => _Remarks;
            set
            {
                if (value == _Remarks) return;
                _Remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }

        protected override string GetErrorInfo(string prop)
        {
            if (prop == nameof(Number))
            {
                if (!Number.IsCellNumber()) return "INVALID NUMBER";
            }
            return base.GetErrorInfo(prop);
        }

        protected override bool GetIsEmpty()
        {
            if (string.IsNullOrEmpty(Number)) return true;
            if (string.IsNullOrEmpty(Name)) return true;
            return false;
        }
    }
}
