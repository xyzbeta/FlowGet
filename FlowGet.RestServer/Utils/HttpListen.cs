using EmbedIO;
using EmbedIO.Net;
using System.Net.Sockets;

namespace FlowGet.RestServer.Utils
{
    internal class HttpListen : IDisposable
    {
        private WebServer? _webServer;
        private readonly Dictionary<string, Func<IHttpContext, Task>> _callbackPostDict = [];

        public void RegisterService(string name, Func<IHttpContext, Task> callback)
        {
            _callbackPostDict.Add(name, callback);
        }

        public void Run(string port)
        {
            _webServer = new WebServer(o => o
                .WithUrlPrefix(port)
                .WithMode(HttpListenerMode.EmbedIO));
            _webServer.WithModule(new RestModule("/", _callbackPostDict));
            _ = _webServer.RunAsync();
        }

        public void Dispose()
        {
            _webServer?.Dispose();
        }

        internal static bool IsPortAvailable(int port)
        {
            try
            {
                using var listener = new TcpListener(System.Net.IPAddress.Any, port);
                listener.Start();
                listener.Stop();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private sealed class RestModule : WebModuleBase
        {
            private readonly Dictionary<string, Func<IHttpContext, Task>> _callbacks;

            public RestModule(string basePath, Dictionary<string, Func<IHttpContext, Task>> callbacks)
                : base(basePath)
            {
                _callbacks = callbacks;
            }

            public override bool IsFinalHandler => false;

            protected override async Task OnRequestAsync(IHttpContext context)
            {
                var url = context.Request.Url.AbsolutePath.Trim('/');

                if (string.IsNullOrEmpty(url))
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    var bytes = System.Text.Encoding.UTF8.GetBytes("不能是空方法");
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.OutputStream.Close();
                    return;
                }

                try
                {
                    if (context.Request.HttpVerb == HttpVerbs.Post
                        && _callbacks.TryGetValue(url, out var callback))
                    {
                        await callback(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/plain;charset=utf-8";
                        var bytes = System.Text.Encoding.UTF8.GetBytes($"没有找到【/{url}】方法");
                        await context.Response.OutputStream.WriteAsync(bytes);
                        context.Response.OutputStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/plain;charset=utf-8";
                    var bytes = System.Text.Encoding.UTF8.GetBytes(ex.Message);
                    await context.Response.OutputStream.WriteAsync(bytes);
                    context.Response.OutputStream.Close();
                }
            }
        }
    }
}
