namespace BullOak.Repositories.EventStore.Test.Integration.EventStoreServer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    public static class EventStoreServerStarterHelper
    {
        public static Process StartServer()
        {
            var assemblyPath = WebUtility.UrlDecode((new Uri(typeof(EventStoreServerStarterHelper).Assembly.CodeBase)).AbsolutePath);

            var currentDir = new DirectoryInfo(Path.GetDirectoryName(assemblyPath)).FullName;
            var eventStoreServerExe = Path.Combine(currentDir, "EventStoreServer", "EventStore.ClusterNode.exe");
            return StartProcess(eventStoreServerExe, "", currentDir, true);
        }

        private static Process StartProcess(string command,
            string arguments,
            string workingDirectory,
            bool showWindow)
        {
            var p = new Process
            {
                StartInfo =
                    {
                        FileName = command,
                        Arguments = arguments,
                        RedirectStandardOutput = !showWindow,
                        UseShellExecute = showWindow,
                        WorkingDirectory = workingDirectory
                    }
            };
            p.Start();
            return p;
        }

        public static void StopProcess(Process process)
        {
            if (process == null || process.HasExited)
            {
                return;
            }

            try
            {
                process.Kill();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }

    }
}