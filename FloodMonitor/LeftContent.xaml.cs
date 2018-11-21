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

namespace FloodMonitor
{
    /// <summary>
    /// Interaction logic for LeftContent.xaml
    /// </summary>
    public partial class LeftContent : UserControl
    {
        public LeftContent()
        {
            InitializeComponent();
            Messenger.Default.AddListener<ModemLog>(Messages.ModemDataReceived, log =>
            {
                Dispatcher.Invoke(()=>ModemLog.ScrollIntoView(log));
            });
            Modem.Instance.Log.CurrentChanged += (sender, args) =>
            {
                if(Modem.Instance.Log.CurrentItem!=null)
                    ModemLog.ScrollIntoView(Modem.Instance.Log.CurrentItem);
            };

        }
    }
}
