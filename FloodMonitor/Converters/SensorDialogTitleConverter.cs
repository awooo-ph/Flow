using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloodMonitor.Converters
{
    class SensorDialogTitleConverter:ConverterBase
    {
        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (!(value is long id)) return "";
            return id == 0 ? "NEW SENSOR" : "EDIT SENSOR";
        }
    }
}
