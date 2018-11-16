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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FloodMonitor.ViewModels;
using MaterialDesignThemes.Wpf;

namespace FloodMonitor
{
    /// <summary>
    /// Interaction logic for NewSensorDialog.xaml
    /// </summary>
    public partial class NewSensorDialog : UserControl
    {
        private NewSensorDialog()
        {
            InitializeComponent();
        }

        internal static async Task<Sensor> Show()
        {
            var dlg = new NewSensorDialog();
            var res = await DialogHost.Show(dlg, "Root");
            if ((res is bool b) && b)
            {
                if (string.IsNullOrEmpty(dlg.Name.Text))
                {
                    MessageBox.Show("Name is required");
                    return null;
                }
                if (string.IsNullOrEmpty(dlg.Number.Text))
                {
                    MessageBox.Show("Number is required");
                    return null;
                }

                if (!dlg.Number.Text.IsCellNumber())
                {
                    MessageBox.Show("Invalid number!");
                    return null;
                }

                var sensor = new Sensor()
                {
                    Number = dlg.Number.Text,
                    Location = dlg.Location.Text,
                    SensorName = dlg.Name.Text
                };

                var oldSensor = Sensor.Cache.FirstOrDefault(x => x.Equals(sensor));
                if (oldSensor != null)
                {
                    if (oldSensor.IsDeleted)
                    {
                        oldSensor.Undelete();
                        return null;
                    }

                    MessageBox.Show($"A sensor with mobile number {sensor.Number} already exists.");
                    return null;
                }

                return sensor;
            }

            return null;
        }
    }
}
