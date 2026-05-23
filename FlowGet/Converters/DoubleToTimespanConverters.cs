using System;
using System.Globalization;

namespace FlowGet.Converters
{
    public class DoubleToTimespanConverters : BaseConverters<double, string>
    {
        public static DoubleToTimespanConverters Instance { get; } = new DoubleToTimespanConverters();
        public override string Convert(double value, Type targetType, object? parameter, CultureInfo culture)
        {
            return TimeSpan.FromSeconds(value).ToString(@"hh\:mm\:ss", culture);
        }

        public override double ConvertBack(string value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
