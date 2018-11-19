using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FloodMonitor.ViewModels;

namespace FloodMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            awooo.Context = SynchronizationContext.Current;
            awooo.IsRunning = true;
            base.OnStartup(e);
        }
    }

   
}
