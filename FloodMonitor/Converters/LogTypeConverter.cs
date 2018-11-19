using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using FloodMonitor.ViewModels;
using MaterialDesignThemes.Wpf;

namespace FloodMonitor.Converters
{
    class LogTypeConverter:ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is ModemLog.LogTypes l)) return PackIconKind.Information;
            switch (l)
            {
                case ModemLog.LogTypes.AtCommand:
                    return PackIconKind.Console;
                case ModemLog.LogTypes.Sms:
                    return PackIconKind.Message;
                default:
                    return PackIconKind.Alert;
            }
        }
    }
}
