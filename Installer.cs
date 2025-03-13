using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WPT_Updater
{
    internal class Download
    {
        //attributes

        //The Path attribute is static since it is the same for all downloads
        private string Path = $"C:\\Users\\{Auth.UserName}\\Desktop\\";
        //The Link attribute is provided by the ProgramsClass object
        private readonly string Link;
        //The following attributes are used to control the download process (start, pause, resume, show progress, stop)
        private volatile bool AllowedToRun;
        private readonly int ChunkSize;
        private readonly IProgress<double> Progress;
        private readonly Lazy<long> contentLength;

        public long BytesWritten { get; private set; }
        public long ContentLength => contentLength.Value;

        public bool Done => ContentLength == BytesWritten;

        //constructor
        public Download(ProgramsClass program, int chunkSizeInBytes = 10000 /*Default to 0.01 mb*/, IProgress<double> progress = null)
        {
            if (string.IsNullOrEmpty(program.DownloadLink))
                throw new ArgumentNullException("source is empty");
            if (string.IsNullOrEmpty(Path))
                throw new ArgumentNullException("destination is empty");

            AllowedToRun = true;
            Link = program.DownloadLink;
            ChunkSize = chunkSizeInBytes;
            contentLength = new Lazy<long>(GetContentLength);
            Progress = progress;
            Path += program.DownloadLink.Substring(program.DownloadLink.LastIndexOf('/') + 1);

            if (!File.Exists(Path))
                BytesWritten = 0;
            else
            {
                try
                {
                    BytesWritten = new FileInfo(Path).Length;
                }
                catch
                {
                    BytesWritten = 0;
                }
            }
        }

        //methods

        private long GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(Link);
            request.Method = "HEAD";

            using (var response = request.GetResponse())
                return response.ContentLength;
        }

        private async Task Start(long range)
        {
            if (!AllowedToRun)
                throw new InvalidOperationException();

            if (Done)
                //file has been found in folder destination and is already fully downloaded 
                return;

            var request = (HttpWebRequest)WebRequest.Create(Link);
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
            request.AddRange(range);

            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var fs = new FileStream(Path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        while (AllowedToRun)
                        {
                            var buffer = new byte[ChunkSize];
                            var bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);

                            if (bytesRead == 0) break;

                            await fs.WriteAsync(buffer, 0, bytesRead);
                            BytesWritten += bytesRead;
                            Progress?.Report((double)BytesWritten / ContentLength);
                        }

                        await fs.FlushAsync();
                    }
                }
            }
        }

        public Task Start()
        {
            AllowedToRun = true;
            return Start(BytesWritten);
        }

        public void Pause()
        {
            AllowedToRun = false;
        }

    }
}
