using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace PeMirror.Injections
{
    public class PixelExperienceCrawler
    {
        private Parameters Parameters { get; }

        public PixelExperienceCrawler(Parameters parameters)
        {
            Parameters = parameters;
        }

        public PixelExperienceBuild GetLatestBuild()
        {
            return GetBuildNodes().OrderByDescending(i => i.Date).FirstOrDefault();
        }

        public IEnumerable<PixelExperienceBuild> GetBuildNodes()
        {
            var url = $"{Parameters.BaseUrl}{Parameters.DeviceName}/";
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var tableNode = doc.DocumentNode.SelectSingleNode("//*[@id=\"collapse-1\"]/div/div/table");
            var buildNodes = tableNode.SelectNodes("tbody/tr").ToList();

            foreach (var buildNode in buildNodes)
            {
                var htmlNodes = buildNode.SelectNodes("td").ToList();
                var date = htmlNodes[0].InnerText;
                var name = htmlNodes[1].InnerText.Trim();
                var buildUrl =
                    $"https://iweb.dl.sourceforge.net/project/pixelexperience/{Parameters.DeviceName}/{name}";
                var downloadCount = htmlNodes[2].InnerText;

                yield return new PixelExperienceBuild(name, buildUrl, date, downloadCount);
            }
        }
    }
}