using EmbedIO;
using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3u8;
using FlowGet.M3U8;
using FlowGet.M3U8.Extensions;
using FlowGet.RestServer.Extensions;
using FlowGet.RestServer.Models;
using FlowGet.RestServer.Utils;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace FlowGet.RestServer
{
    public class HttpListenService
    {
        private readonly char _DirectorySeparatorChar = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '/' : '\\';
        private readonly HttpListen httpListen = new();
        private IAppCommandService AppCommandService = default!;

        private readonly JsonSerializerOptions jsonSerializerOptions;
        private readonly static HttpListenService instance = new();

        public event EventHandler? RequestReceived;
        public DateTime? LastRequestTime { get; private set; }
        public int? Port { get; private set; }

        public static HttpListenService Instance => instance;
        private HttpListenService()
        {
            jsonSerializerOptions = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
            httpListen.RegisterService("downloadmedias", DownloadMedias);
            httpListen.RegisterService("downloadbyurl", DownloadByUrl);
            httpListen.RegisterService("downloadbycontent", DownloadByContent);
            httpListen.RegisterService("downloadbyjsoncontent", DownloadByJsonContent);
            httpListen.RegisterService("getm3u8data", GetM3u8FileInfo);
        }

        public void Initialization(IAppCommandService appCommandService)
        {
            AppCommandService = appCommandService;
        }

        public void Run(Action<int> SetPortAction)
        {
            for (int i = 65432; i > 65400; i--)
            {
                if (!HttpListen.IsPortAvailable(i))
                    continue;

                try
                {
                    httpListen.Run($"http://+:{i}/");
                    Port = i;
                    SetPortAction(i);
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private async Task DownloadMedias(IHttpContext context)
        {
            try
            {
                RequestWithMediaUri? requestWithMediaUri = await JsonSerializer.DeserializeAsync<RequestWithMediaUri>(context.OpenRequestStream(), jsonSerializerOptions);
                if (requestWithMediaUri is null)
                {
                    await context.Response.SendJson(Response.Error("序列化失败"));
                    return;
                }

                requestWithMediaUri.Validate();
                if (!string.IsNullOrWhiteSpace(requestWithMediaUri.SavePath))
                    requestWithMediaUri.SavePath = requestWithMediaUri.SavePath.Replace(_DirectorySeparatorChar, Path.DirectorySeparatorChar);
                AppCommandService.DownloadMedia(null, requestWithMediaUri.ToMediaDownloadParams());
                LastRequestTime = DateTime.UtcNow;
                RequestReceived?.Invoke(this, EventArgs.Empty);

                await context.Response.SendJson(Response.Success());
            }
            catch (Exception e)
            {
                await context.Response.SendJson(Response.Error($"请求失败,{e.Message}"));
            }
        }

        private async Task DownloadByUrl(IHttpContext context)
        {
            try
            {
                RequestWithURI? requestWithURI = await JsonSerializer.DeserializeAsync<RequestWithURI>(context.OpenRequestStream(), jsonSerializerOptions);
                if(requestWithURI is null)
                {
                    await context.Response.SendJson(Response.Error("序列化失败"));
                    return;
                }

                requestWithURI.Validate();
                if (!string.IsNullOrWhiteSpace(requestWithURI.SavePath))
                    requestWithURI.SavePath = requestWithURI.SavePath.Replace(_DirectorySeparatorChar, Path.DirectorySeparatorChar);
                AppCommandService.DownloadByUrl(null,requestWithURI.ToM3u8DownloadParams());
                LastRequestTime = DateTime.UtcNow;
                RequestReceived?.Invoke(this, EventArgs.Empty);

                await context.Response.SendJson(Response.Success());
            }
            catch (Exception e)
            {
                await context.Response.SendJson(Response.Error($"请求失败,{e.Message}"));
            }
        }

        private async Task DownloadByContent(IHttpContext context)
        {
            try
            {
                RequestWithContent? requestWithContent = await JsonSerializer.DeserializeAsync<RequestWithContent>(context.OpenRequestStream(), jsonSerializerOptions);
                if (requestWithContent is null)
                {
                    await context.Response.SendJson(Response.Error("序列化失败"));
                    return;
                }


                IM3uFileInfo? m3UFileInfo = M3u8FileInfoClient.CreateM3uFileReader(context.Request.Url!).GetM3u8FileInfo(requestWithContent.Content);
                if (m3UFileInfo is null)
                {
                    await context.Response.SendJson(Response.Error("m3u8内容读取失败,请检查传入的参数是否有误"));
                    return;
                }

                if(m3UFileInfo!.MediaFiles is null || !m3UFileInfo.MediaFiles.Any())
                {
                    await context.Response.SendJson(Response.Error("m3u8的ts列表为空"));
                    return;
                }
                requestWithContent.Validate();

                RequestWithM3u8FileInfo requestWithM3U8FileInfo = new()
                {
                    M3UFileInfos = m3UFileInfo,
                    VideoName = requestWithContent.VideoName,
                    SavePath = !string.IsNullOrWhiteSpace(requestWithContent.SavePath)? requestWithContent.SavePath.Replace(_DirectorySeparatorChar, Path.DirectorySeparatorChar) : requestWithContent.SavePath,
                    Headers = requestWithContent.Headers,
                };
                AppCommandService.DownloadByM3uFileInfo(null, requestWithM3U8FileInfo.ToDownloadParam(), requestWithM3U8FileInfo.M3UFileInfos);
                LastRequestTime = DateTime.UtcNow;
                RequestReceived?.Invoke(this, EventArgs.Empty);

                await context.Response.SendJson(Response.Success());
            }
            catch (Exception e)
            {
                await context.Response.SendJson(Response.Error($"请求失败,{e.Message}"));
            }

        }

        //视频地址 必须是http开头 或者磁盘根路径
        private async Task DownloadByJsonContent(IHttpContext context)
        {
            try
            {
                RequestWithM3u8FileInfo? requestWithM3U8FileInfo = await JsonSerializer.DeserializeAsync<RequestWithM3u8FileInfo>(context.OpenRequestStream(), jsonSerializerOptions);
                if (requestWithM3U8FileInfo is null)
                {
                    await context.Response.SendJson(Response.Error("序列化失败"));
                    return;
                }

                requestWithM3U8FileInfo.Validate();
                requestWithM3U8FileInfo.M3UFileInfos.PlaylistType = "VOD";
                if (!string.IsNullOrWhiteSpace(requestWithM3U8FileInfo.SavePath))
                    requestWithM3U8FileInfo.SavePath = requestWithM3U8FileInfo.SavePath.Replace(_DirectorySeparatorChar, Path.DirectorySeparatorChar);
                AppCommandService.DownloadByM3uFileInfo(null, requestWithM3U8FileInfo.ToDownloadParam(), requestWithM3U8FileInfo.M3UFileInfos);
                LastRequestTime = DateTime.UtcNow;
                RequestReceived?.Invoke(this, EventArgs.Empty);

                await context.Response.SendJson(Response.Success());
            }
            catch (Exception e)
            {
                await context.Response.SendJson(Response.Error($"请求失败,{e.Message}"));
            }
        }


        private async Task GetM3u8FileInfo(IHttpContext context)
        {
            try
            {
                RequestWithGetM3u8FileInfo? requestWIthGetM3U8FileInfo = await JsonSerializer.DeserializeAsync<RequestWithGetM3u8FileInfo>(context.OpenRequestStream(), jsonSerializerOptions);
                if (requestWIthGetM3U8FileInfo is null)
                {
                    await context.Response.SendJson(Response.Error("序列化失败"));
                    return;
                }

                requestWIthGetM3U8FileInfo.Validate();
                IM3uFileInfo m3UFileInfo = M3u8FileInfoClient.CreateM3uFileReader(requestWIthGetM3U8FileInfo.Url!).GetM3u8FileInfo(requestWIthGetM3U8FileInfo.Content);
                Response<IM3uFileInfo> r = m3UFileInfo.MediaFiles != null && m3UFileInfo.MediaFiles.Any()
                                        ? new Response<IM3uFileInfo>(0, "解析成功", m3UFileInfo)
                                        : new Response<IM3uFileInfo>(1, "没有包含任何数据", null);
                await context.Response.SendJson(r);
            }
            catch (Exception ex)
            {
                await context.Response.SendJson(Response.Error($"解析失败,{ex.Message}"));
            }
        }
    }
}