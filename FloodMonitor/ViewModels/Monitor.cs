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



        protected override bool GetIsEmpty()
        {
            return false;
        }
    }
}
