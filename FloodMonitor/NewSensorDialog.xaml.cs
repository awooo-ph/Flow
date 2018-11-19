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

        internal static async Task<Sensor> Show(Sensor sensor = null)
        {
            var dlg = new NewSensorDialog();

            if (sensor == null)
                sensor = new Sensor();

            dlg.DataContext = sensor;

            var res = await DialogHost.Show(dlg, "Root");
            if ((res is bool b) && b)
            {
                if (string.IsNullOrEmpty(sensor.SensorName))
                {
                    await MessageDialog.Show("Sensor name is required!");
                    return null;
                }
                if (string.IsNullOrEmpty(sensor.Number))
                {
                    await MessageDialog.Show("Sensor SIM number is required!");
                    return null;
                }

                if (!sensor.Number.IsCellNumber())
                {
                    await MessageDialog.Show("SIN number is invalid!");
                    return null;
                }

                return sensor;
            }

            return null;
        }
    }
}
