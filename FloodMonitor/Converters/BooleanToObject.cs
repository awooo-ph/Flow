using System;

namespace FloodMonitor.Converters
{
    internal class BooleanToObject : ConverterBase
    {
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        public BooleanToObject(object trueValue = null, object falseValue = null)
        {
            TrueValue = trueValue;
            FalseValue = falseValue;
        }

        protected override object Convert(object value, Type targetType, object parameter)
        {
            if (value == null) return FalseValue;
            if ((bool)value) return TrueValue;
            return FalseValue;
        }
    }
}