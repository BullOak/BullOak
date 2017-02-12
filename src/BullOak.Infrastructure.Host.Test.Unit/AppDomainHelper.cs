namespace BullOak.Infrastructure.Host.Test.Unit
{
    // Add the following as a COM reference - C:\WINDOWS\Microsoft.NET\Framework\vXXXXXX\mscoree.tlb
    // Set "Embed Interop Types" option to False for the reference.
    using mscoree;

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal sealed class AppDomainHelper
    {
        public static IEnumerable<string> GetAppDomainNames()
        {
            var names = new List<string>();
            IntPtr enumHandle = IntPtr.Zero;
            var host = new CorRuntimeHostClass();
            try
            {
                host.EnumDomains(out enumHandle);
                object domain = null;
                while (true)
                {
                    host.NextDomain(enumHandle, out domain);
                    if (domain == null) break;
                    AppDomain appDomain = (AppDomain)domain;
                    names.Add(appDomain.FriendlyName);
                }
                return names;
            }
            catch (Exception e)
            {
                var error = e.ToString();
                Trace.TraceError($"Error enumerating loaded application domains: {error}");
                return null;
            }
            finally
            {
                host.CloseEnum(enumHandle);
                Marshal.ReleaseComObject(host);
            }
        }
    }
}
