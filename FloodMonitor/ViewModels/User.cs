using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FloodMonitor.ViewModels
{
    public sealed class User:ModelBase<User>
    {
        protected override bool GetIsEmpty()
        {
            return false;
        }

        private string _Username;

        public string Username
        {
            get => _Username;
            set
            {
                if (value == _Username) return;
                _Username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _Password;

        public string Password
        {
            get => _Password;
            set
            {
                if (value == _Password) return;
                _Password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string _Fullname;

        public string Fullname
        {
            get => _Fullname;
            set
            {
                if (value == _Fullname) return;
                _Fullname = value;
                OnPropertyChanged(nameof(Fullname));
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

        private bool _IsAdmin;

        public bool IsAdmin
        {
            get => _IsAdmin;
            set
            {
                if (value == _IsAdmin) return;
                _IsAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }


    }
}
