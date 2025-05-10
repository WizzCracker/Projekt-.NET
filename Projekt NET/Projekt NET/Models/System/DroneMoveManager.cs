using System.Collections.Concurrent;

namespace Projekt_NET.Models.System
{
    public static class DroneMoveManager
    {
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _droneLocks = new();

        public static async Task<bool> TryMoveDroneAsync(int droneId, Func<Task> moveAction)
        {
            var semaphore = _droneLocks.GetOrAdd(droneId, _ => new SemaphoreSlim(1, 1));

            if (!await semaphore.WaitAsync(0)) // nie czekaj, jeśli zajęty
            {
                return false; // dron już się porusza
            }

            try
            {
                await moveAction();
                return true;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

}
