using System;

namespace PeMirror.Injections
{
    public class PixelExperienceBuild
    {
        public DateTime Date { get; }

        public string DownloadCount { get; }

        public string Name { get; }

        public string Url { get; }

        public PixelExperienceBuild(string name, string url, string date, string downloadCount)
        {
            Name = name;
            Url = url;
            Date = DateTime.Parse(date);
            DownloadCount = downloadCount;
        }
    }
}