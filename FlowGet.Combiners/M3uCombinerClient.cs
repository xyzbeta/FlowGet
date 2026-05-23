using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.Settings;
using FlowGet.Combiners.M3uCombiners;
using FlowGet.Combiners.VideoConverter;

namespace FlowGet.Combiners
{
    public class M3uCombinerClient(ILog Log, IDownloadParamBase DownloadParams, IMergeSetting Settings)
    {
        private M3uCombiner? m3UCombiner;
        private FFmpeg? ffmpeg;
        public IDialogProgress DialogProgress { get; set; } = default!;

        public M3uCombiner M3u8FileMerger
        {
            get
            {
                m3UCombiner ??= new(Log, DownloadParams, Settings);
                return m3UCombiner;
            }
        }


        public FFmpeg FFmpeg
        {
            get
            {
                ffmpeg ??= new(Log, DownloadParams, Settings);
                return ffmpeg;
            }
        }
    }
}
