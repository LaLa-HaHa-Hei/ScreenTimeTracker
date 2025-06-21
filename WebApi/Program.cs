namespace WebApi
{
    public class Program
    {
        private static Mutex? _mutex;
        public static async Task Main(string[] args)
        {
            if (!EnsureSingleInstance())
                return;

            var host = new WebApiHost();
            await host.StartAsync(args);

            await Task.Delay(Timeout.Infinite);

            await host.StopAsync();

            _mutex?.ReleaseMutex();

        }
        private static bool EnsureSingleInstance()
        {
            _mutex = new Mutex(true, "ScreenTimeWebApiUniqueMutexName", out bool createdNew);
            if (!createdNew)
            {
                return false;
            }
            return true;
        }
    }
}