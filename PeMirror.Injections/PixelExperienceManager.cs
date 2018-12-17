using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using ByteSizeLib;
using Kurukuru;

namespace PeMirror.Injections
{
    public class PixelExperienceManager
    {
        private long _lastBytes;

        private DateTime _lastUpdate;

        private Parameters Parameters { get; }

        private PixelExperienceCrawler PixelExperienceCrawler { get; }

        public PixelExperienceManager(Parameters parameters, PixelExperienceCrawler pixelExperienceCrawler)
        {
            Parameters = parameters;
            PixelExperienceCrawler = pixelExperienceCrawler;
        }

        private string GetDownloadPath(PixelExperienceBuild build)
        {
            var combine = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ?? throw new
                                           InvalidOperationException(), Parameters.DownloadPath);
            var dir = new DirectoryInfo(combine);

            if (!dir.Exists) dir.Create();

            return Path.Combine(combine, build.Name);
        }

        private void ProgressChanged(long bytes, Spinner spinner, ProgressChangedEventArgs args,
            PixelExperienceBuild latestBuild)
        {
            if (_lastBytes == 0)
            {
                _lastUpdate = DateTime.Now;
                _lastBytes = bytes;
                return;
            }

            var now = DateTime.Now;
            var timeSpan = now - _lastUpdate;

            if (timeSpan.Seconds <= 0) return;

            var bytesChange = bytes - _lastBytes;
            var bytesPerSecond = bytesChange / timeSpan.Seconds;

            _lastBytes = bytes;
            _lastUpdate = now;

            spinner.Text =
                $"Downloading file {latestBuild.Name}... {args.ProgressPercentage}% @ {ByteSize.FromBytes(bytesPerSecond).KiloBytes}Kb/s";
        }

        public async Task<string> DownloadBuild(PixelExperienceBuild latestBuild)
        {
            var downloadLocation = new FileInfo(GetDownloadPath(latestBuild));

            await Spinner.StartAsync(GetDownloadingText(latestBuild), async spinner =>
            {
                using (var wc = new WebClient())
                {
                    wc.DownloadProgressChanged += (sender, args) =>
                        ProgressChanged(args.BytesReceived, spinner, args, latestBuild);

                    wc.DownloadFileCompleted += (sender, args) =>
                    {
                        if (!args.Cancelled && args.Error == null)
                            spinner.Succeed($"Downloaded {latestBuild.Name} successfully");
                        else
                            spinner.Fail(args.Error?.Message);
                    };

                    await wc.DownloadFileTaskAsync(latestBuild.Url, downloadLocation.FullName);
                }
            });


            return downloadLocation.FullName;
        }

        private static string GetDownloadingText(PixelExperienceBuild latestBuild)
        {
            return $"Downloading file {latestBuild.Name}...";
        }
    }
}