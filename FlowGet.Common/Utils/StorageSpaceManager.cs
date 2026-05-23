using FlowGet.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FlowGet.Common.Utils;

public class StorageSpaceManager
{
    public static string GetCachesPath()
    {
#if DEBUG
        return @"C:\Users\admin\Desktop\666\Caches";
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Path.Combine(StorageSpace.UserProfile.GetDirectoryPath(), "Library", "Caches", "FlowGet");
        else
            return Path.Combine(StorageSpace.Instance.GetDirectoryPath(), "Caches");
#endif
    }

    public static string GetSavePath()
    {
#if DEBUG
            return @"C:\Users\admin\Desktop\666\download";
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Path.Combine(StorageSpace.UserProfile.GetDirectoryPath(), "Downloads", "FlowGet");
        else
            return Path.Combine(StorageSpace.Instance.GetDirectoryPath(), "Downloads");
#endif
    }


    public static string GetTempPath()
    {
#if DEBUG
        return @"C:\Users\admin\Desktop\666\Temp";
#else
        return Path.Combine(StorageSpace.UserDomain.GetDirectoryPath(), "FlowGet", "Temp");
#endif
    }


    public static string GetConfigPath()
    {
#if DEBUG
        return @"C:\Users\admin\Desktop\666\Config";
#else
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return Path.Combine(StorageSpace.UserDomain.GetDirectoryPath(), "FlowGet","Config");
        else
            return Path.Combine(StorageSpace.Instance.GetDirectoryPath(),"Config");
#endif
    }

    public static string GetFFmpegPath()
    {
#if DEBUG
        return @"C:\Users\admin\Desktop\666\ffmpeg.exe";
#else
        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return Path.Combine(StorageSpace.Instance.GetDirectoryPath(), "ffmpeg.exe");
        else 
            return Path.Combine(StorageSpace.Instance.GetDirectoryPath(), "ffmpeg");
#endif
    }
}
