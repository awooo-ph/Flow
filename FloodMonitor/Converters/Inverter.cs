using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodMonitor.Converters
{
    class Inverter:ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            return !(value is bool b && b);
        }
    }
}
