using FlowGet.Abstractions.Common;
using FlowGet.Abstractions.M3uDownloaders;
using FlowGet.Abstractions.Models;
using FlowGet.M3U8.AttributeReaders;
using FlowGet.M3U8.M3UFileReaderManangers;
using FlowGet.M3U8.M3UFileReaders;
using System;
using System.Net.Http;

namespace FlowGet.M3U8
{
    public partial class M3u8FileInfoClient(IDownloadContext context)
    {
        private M3UFileReaderManager? _m3UFileReaderManager;
        private IM3uFileReader? _m3UFileReader;

        public M3UFileReaderManager M3UFileReadManager
        {
            get
            {
                _m3UFileReaderManager ??= new M3UFileReaderManager(context, M3u8FileReader);
                return _m3UFileReaderManager;
            }
        }

        public IM3uFileReader M3u8FileReader
        {
            get
            {
                if(_m3UFileReader is null)
                {
                    IM3u8DownloadParam m3U8DownloadParam = (IM3u8DownloadParam)context.DownloadParam;
                    if (m3U8DownloadParam.RequestUrl.IsFile && m3U8DownloadParam.RequestUrl.OriginalString.EndsWith(".json"))
                    {
                        _m3UFileReader = new M3UFileReaderWithJson(m3U8DownloadParam.RequestUrl);
                    }
                    else
                    {
                        _m3UFileReader = new M3UFileReaderWithStream(m3U8DownloadParam.RequestUrl);
                    }
                    _m3UFileReader.InitAttributeReade(AttributeReaderRoot.Instance.AttributeReaders);
                }
                return _m3UFileReader;
            }
        }

    }

    public partial class M3u8FileInfoClient
    {
        public static IM3uFileReader CreateM3uFileReader(Uri requestUrl)
        {
            IM3uFileReader m3UFileReader = new M3UFileReaderWithStream(requestUrl);
            m3UFileReader.InitAttributeReade(AttributeReaderRoot.Instance.AttributeReaders);
            return m3UFileReader;
        }
    }
}
