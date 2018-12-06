using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FloodMonitor.ViewModels;

namespace FloodMonitor
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private DateTime _shown = DateTime.MinValue;
        private async void ShowMessage(string message)
        {
            Message.Text = message;
            MessageBox.Visibility = Visibility.Visible;
            _shown = DateTime.Now;
            while ((DateTime.Now - _shown).TotalMilliseconds < 4444)
                await Task.Delay(7);
            MessageBox.Visibility = Visibility.Hidden;
        }

        

        private void LoginCLicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username.Text))
            {
                Username.Focus();
                ShowMessage("ENTER USERNAME!");
                return;
            }

            if (string.IsNullOrEmpty(PasswordBox.Password))
            {
                PasswordBox.Focus();
                ShowMessage("ENTER PASSWORD!");
                return;
            }
            User user;

            if (User.Cache.Count == 0)
            {
                user = new User
                {
                    Username = Username.Text,
                    Password = PasswordBox.Password,
                    Fullname = "Administrator",
                    Position = "ADMIN"
                };
                user.Save();
            } else 
            user = User.Cache.FirstOrDefault(x => x.Username.ToLower() == Username.Text.ToLower());

            if (user?.Password!=PasswordBox.Password)
            {
                ShowMessage("INVALID PASSWORD!");
                return;
            }

            Username.Text = "";
            PasswordBox.Password = "";

            MainViewModel.Instance.CurrentUser = user;
        }
    }
}
