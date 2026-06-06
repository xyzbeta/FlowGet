using System.IO;

namespace FlowGet.Utils
{
    internal static class FileEx
    {
        /// <summary>
        /// 确保输出文件路径唯一，如同名文件已存在(大小>0)则自动追加 (1)(2) 后缀
        /// </summary>
        /// <param name="fileFullPath">原始完整路径（含扩展名）</param>
        /// <returns>唯一的目标文件路径</returns>
        public static string EnsureUniquePath(string fileFullPath)
        {
            if (!File.Exists(fileFullPath))
                return fileFullPath;

            FileInfo fileInfo = new(fileFullPath);
            if (fileInfo.Length == 0)
                return fileFullPath;

            string dir = Path.GetDirectoryName(fileFullPath) ?? ".";
            string nameWithoutExt = Path.GetFileNameWithoutExtension(fileFullPath);
            string ext = Path.GetExtension(fileFullPath);

            for (int counter = 1; ; counter++)
            {
                string candidate = Path.Combine(dir, $"{nameWithoutExt} ({counter}){ext}");
                if (!File.Exists(candidate))
                    return candidate;

                FileInfo candidateInfo = new(candidate);
                if (candidateInfo.Length == 0)
                    return candidate;
            }
        }
    }
}
