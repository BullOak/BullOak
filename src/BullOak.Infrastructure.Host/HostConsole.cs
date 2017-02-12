namespace BullOak.Infrastructure.Host
{
    using System;

    public static class HostConsole<T>
        where T : class, IHost, new()
    {
        private const string Quit = "quit";

        public static void Host()
        {
            var host = new T();
            using (var handle = host.Start())
            {
                do
                {
                    Console.WriteLine($"Enter '{Quit}' to exit.");
                } while (!string.Equals(Console.ReadLine(), Quit, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
