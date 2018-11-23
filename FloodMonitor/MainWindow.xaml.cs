using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Dragablz;
using FloodMonitor.ViewModels;

namespace FloodMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

           
            awooo.Context = SynchronizationContext.Current;

            AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(ItemDragCompleted), true);        
        }

        private object[] _order;
        
        private void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs e)
        {
            var item = e.DragablzItem.DataContext;
            
            if (_order == null) return;

            for(var i=0L;i < _order.Length;i++)
            {
                var sensor = ((Sensor) _order[i]);
                if (sensor.Order != i + 1)
                {
                    sensor.Order = i + 1;
                    sensor.Save();
                }
            }
        }

        private void StackPositionMonitor_OnOrderChanged(object sender, OrderChangedEventArgs e)
        {
            _order = e.NewOrder;
        }
    }
}
