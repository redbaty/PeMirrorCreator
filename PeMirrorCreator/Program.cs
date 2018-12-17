using System;
using System.Linq;
using Kurukuru;
using Ninject;
using PeMirror.Injections;
using PeMirror.Injections.Extensions;

namespace PeMirrorCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var kernel = new StandardKernel();
            kernel.InjectPixelExperienceKernel();
            var pixelExperienceCrawler = kernel.Get<PixelExperienceCrawler>();


            var drive = kernel.Get<DriveController>();
            var pxManager = kernel.Get<PixelExperienceManager>();
            var latestBuild = pixelExperienceCrawler.GetLatestBuild();
            var files = drive.GetFiles(latestBuild.Name).ToList();

            if (files.Count <= 0)
            {
                var localFilePath = pxManager.DownloadBuild(latestBuild).Result;
                drive.UploadFile(localFilePath).Wait();
            }
            else
            {
                Spinner.Start(string.Empty, spinner =>
                {
                    spinner.Succeed("No new builds.");
                });
            }
        }
    }
}