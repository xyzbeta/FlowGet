using System.Text;

namespace FlowGet.Common.Extensions
{
    public static class ConvertExtensions
    {
        public static byte[] ToBytes(this string s) => Encoding.UTF8.GetBytes(s);

        public static string GetString(this byte[] bytes) => Encoding.UTF8.GetString(bytes);

        public static byte[] ToHexBytes(this string s) => Convert.FromHexString(s);

        public static string GetHexString(this byte[] bytes) => Convert.ToHexString(bytes);
    }
}
