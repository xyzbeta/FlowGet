using System;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using FlowGet.Utils;

namespace FlowGet.Converters
{
    public class PortToLocalAddress : BaseConverters<int, string>
    {
        public static PortToLocalAddress Instance { get; } = new PortToLocalAddress();
        public override string Convert(int value, Type targetType, object? parameter, CultureInfo culture)
        {
            return $"http://{LocalIpHelper.GetPreferredLocalIPv4()?.ToString() ?? "0.0.0.0"}:{value}";
        }

        public override int ConvertBack(string value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
