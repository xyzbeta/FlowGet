using FlowGet.Abstractions.Common;
using FlowGet.Common.DownloadPrams;
using FlowGet.Services;
using System;
using System.IO;

namespace FlowGet.Extensions;

internal static class DownloadParamBaseExtensions
{
    extension<T>(T downloadbase)
        where T : IDownloadParamBase
    {
        public void CompleteAttribute(SettingsService settingsService)
        {
            if(downloadbase is DownloadParamsBase downloadParamBase)
            {
                if (string.IsNullOrEmpty(downloadbase.SavePath))
                {
                    downloadParamBase.SavePath = settingsService.SavePath;
                }
                else
                {
                    var savePath = Path.GetFullPath(Path.Combine(settingsService.SavePath, downloadParamBase.SavePath));
                    var rootSavePath = Path.GetFullPath(settingsService.SavePath);
                    if (!savePath.StartsWith(rootSavePath))
                        throw new InvalidOperationException("路径非法");
                    downloadParamBase.SavePath = savePath;
                }
                downloadParamBase.Headers ??= settingsService.Headers;
            }
        }
    }
}
