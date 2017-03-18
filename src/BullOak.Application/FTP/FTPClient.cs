namespace BullOak.Application.FTP
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class FTPClient : IFTPClient
    {
        private readonly FTPConfig config;

        public FTPClient(FTPConfig ftpconfig)
        {
            config = ftpconfig ?? throw new ArgumentNullException(nameof(ftpconfig));
        }

        public async Task<IEnumerable<string>> GetFileListFromFTP()
        {
            var ftpRequest = (FtpWebRequest)WebRequest.Create(config.FtpUrl);
            ftpRequest.Credentials = new NetworkCredential(config.UserName, config.Password);
            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

            var response = (FtpWebResponse)ftpRequest.GetResponse();

            var streamReader = new StreamReader(response.GetResponseStream());
            var list = new List<string>();

            var line = await streamReader.ReadLineAsync();
            while (!string.IsNullOrEmpty(line))
            {
                if (line.Any(char.IsLetterOrDigit))
                    list.Add(line);
                line = await streamReader.ReadLineAsync();
            }
            streamReader.Close();
            return list;
        }

        public void DownLoadAllFiles(IEnumerable<string> list, string location)
        {
            using (var ftpClient = new WebClient())
            {
                ftpClient.Credentials = new NetworkCredential(config.UserName, config.Password);
                foreach (var item in list)
                {
                    if (item.Contains("."))
                    {
                        var path = config.FtpUrl + item;
                        var trnsfrpth = location + item;
                        var uri = new Uri(path);
                        ftpClient.DownloadFileAsync(uri, trnsfrpth);
                    }
                }
            }
        }
    }
}
