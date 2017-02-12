using System;

namespace BullOak.Application.FTP
{
    public class FTPConfig
    {
        public FTPConfig(string ftpUrl, string userName, string password)
        {
            if (ftpUrl == null) throw new ArgumentNullException(nameof(ftpUrl));
            if (userName == null) throw new ArgumentNullException(nameof(userName));
            if (password == null) throw new ArgumentNullException(nameof(password));

            FtpUrl = ftpUrl;
            UserName = userName;
            Password = password;
        }

        public string FtpUrl { get; private set; }
                
        public string UserName { get; private set; }

        public string Password { get; private set; }
    }
}
