namespace Tracker
{
    public class Program
    {
        private static Mutex? _mutex;
        public static async Task Main(string[] args)
        {
            if (!EnsureSingleInstance())
                return;

            var tracker = new TrackerHost();
            await tracker.StartAsync();

            await Task.Delay(Timeout.Infinite);

            await tracker.StopAsync();

            _mutex?.ReleaseMutex();
        }

        private static bool EnsureSingleInstance()
        {
            _mutex = new Mutex(true, "ScreenTimeTrackerUniqueMutexName", out bool createdNew);
            if (!createdNew)
            {
                return false;
            }
            return true;
        }
    }
}