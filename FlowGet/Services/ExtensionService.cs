using System;
using System.IO;
using System.Reflection;

namespace FlowGet.Services
{
    public class ExtensionService
    {
        private const string ResourceName = "FlowGet.Assets.flowget-extension.zip";

        public static void SaveToFile(string targetPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream is null)
                throw new InvalidOperationException("扩展文件未找到，请检查嵌入资源");

            using var fileStream = File.Create(targetPath);
            stream.CopyTo(fileStream);
        }
    }
}
