namespace BullOak.Infrastructure.TestHelpers.Application.xUnit
{
    using System;
    using System.Runtime.InteropServices;

    internal static class CpuProfiler
    {
        // References:
        // http://stackoverflow.com/questions/6526828/how-to-get-the-number-of-cpu-cycles-used-by-a-process
        // https://gist.github.com/zhangz/6094657
        // http://www.pinvoke.net/default.aspx/kernel32.getcurrentthread

        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean QueryProcessCycleTime(IntPtr processHandle, out UInt64 CycleTime);


        [DllImport("Kernel32", ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean QueryThreadCycleTime(IntPtr threadHandle, out UInt64 CycleTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        public static ulong GetCurrentProcessCpuCycles()
        {
            ulong cycles;
            if (!QueryProcessCycleTime(GetCurrentProcess(), out cycles))
            {
                return 0;
            }

            return cycles;
        }

        public static ulong GetCurrentThreadCpuCycles()
        {
            ulong cycles;
            if (!QueryThreadCycleTime(GetCurrentThread(), out cycles))
            {
                return 0;
            }

            return cycles;
        }
    }
}
