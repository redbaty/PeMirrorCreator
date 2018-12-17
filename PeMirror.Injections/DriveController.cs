using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Kurukuru;
using File = Google.Apis.Drive.v3.Data.File;

namespace PeMirror.Injections
{
    public class DriveController
    {
        private DriveService _driveService;

        private DriveCredential Credentials { get; }

        private DriveService DriveService
        {
            get
            {
                if (_driveService == null) CreateService();

                return _driveService;
            }
            set => _driveService = value;
        }

        private NamesDictionary NamesDictionary { get; }

        private Parameters Parameters { get; }

        public DriveController(DriveCredential credentials, NamesDictionary namesDictionary, Parameters parameters)
        {
            Credentials = credentials;
            NamesDictionary = namesDictionary;
            Parameters = parameters;
        }

        private void CreateService()
        {
            DriveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = Credentials.GetCredential(),
                ApplicationName = NamesDictionary.ApplicationName
            });
        }

        public IEnumerable<File> GetFiles(string file = null)
        {
            var listRequest = DriveService.Files.List();

            if (!string.IsNullOrEmpty(file))
                listRequest.Q = $"name='{file}' and '{Parameters.RemoteUrlFolderId}' in parents";

            listRequest.Fields = "nextPageToken, files(id, name)";

            return listRequest.Execute()
                .Files;
        }

        public Task UploadFile(string localFilePath)
        {
            var file = new FileInfo(localFilePath);

            return Task.Run(() =>
            {
                var fileMetadata = new File
                {
                    Name = file.Name,
                    Parents = new List<string>
                    {
                        Parameters.RemoteUrlFolderId
                    }
                };

                Spinner.Start($"Uploading file {file.Name}...", spinner =>
                {
                    using (var stream = file.OpenRead())
                    {
                        var request = DriveService.Files.Create(
                            fileMetadata, stream, "application/zip");
                        request.Fields = "id";

                        request.ProgressChanged += progress =>
                        {
                            if (progress.BytesSent > 0)
                            {
                                var percentage = progress.BytesSent * 100 / file.Length;
                                spinner.Text =
                                    $"Uploading file {file.Name}... {percentage}%";
                            }
                            else
                            {
                                spinner.Text =
                                    $"Uploading file {file.Name}...";
                            }
                        };

                        request.ResponseReceived += file1 =>
                        {
                            spinner.Succeed($"Uploaded {file.Name} successfully [{file1.Id}]");
                        };

                        request.Upload();
                    }
                });
            });
        }
    }
}