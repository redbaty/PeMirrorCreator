using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace PeMirror.Injections
{
    public class DriveCredential
    {
        public UserCredential GetCredential()
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (string.IsNullOrEmpty(baseDir)) throw new InvalidOperationException();

            using (var stream =
                new FileStream(Path.Combine(baseDir, "credentials.json"), FileMode.Open, FileAccess.Read))
            {
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] {DriveService.Scope.Drive},
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Path.Combine(baseDir, "token.json"), true)).Result;
            }
        }
    }
}