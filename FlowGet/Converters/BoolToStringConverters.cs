using System;
using System.Globalization;

namespace FlowGet.Converters
{
    public class BoolToStringConverters : BaseConverters<bool, string>
    {
        public static BoolToStringConverters Instance { get; } = new BoolToStringConverters();
        public override string Convert(bool value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value ? "开启" : "关闭";
        }

        public override bool ConvertBack(string value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
