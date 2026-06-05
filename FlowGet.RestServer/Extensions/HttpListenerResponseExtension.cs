using EmbedIO;
using System.Text;

namespace FlowGet.RestServer.Extensions
{
    internal static class HttpResponseExtension
    {
        internal static async Task SendJson(this IHttpResponse response, byte[] message)
        {
            response.StatusCode = 200;
            response.ContentType = "application/json;charset=UTF-8";
            await response.OutputStream.WriteAsync(message);
            response.OutputStream.Close();
        }

        internal static async Task SendText(this IHttpResponse response, string message)
        {
            response.StatusCode = 200;
            response.ContentType = "text/plain;charset=utf-8";
            var bytes = Encoding.UTF8.GetBytes(message);
            await response.OutputStream.WriteAsync(bytes);
            response.OutputStream.Close();
        }
    }
}
