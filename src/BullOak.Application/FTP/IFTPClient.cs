namespace BullOak.Application.FTP
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface  IFTPClient
    {
        Task<IEnumerable<string>> GetFileListFromFTP();

        void DownLoadAllFiles(IEnumerable<string> list, string location);        
    }
}
