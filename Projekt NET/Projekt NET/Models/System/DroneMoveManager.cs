using System.Collections.Concurrent;
using System.Threading;

namespace Projekt_NET.Models.System
{
    public static class DroneMoveManager
    {
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _droneLocks = new();
        private static readonly ConcurrentDictionary<int, CancellationTokenSource> _cancellationTokens = new();

        public static async Task<bool> TryMoveDroneAsync(int droneId, Func<CancellationToken, Task> moveAction)
        {
            var semaphore = _droneLocks.GetOrAdd(droneId, _ => new SemaphoreSlim(1, 1));

            if (!await semaphore.WaitAsync(0))
                return false;

            var cts = new CancellationTokenSource();
            _cancellationTokens[droneId] = cts;

            try
            {
                await moveAction(cts.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Ruch drona {droneId} został przerwany.");
                return false;
            }
            finally
            {
                _cancellationTokens.TryRemove(droneId, out _);
                semaphore.Release();
            }
        }

        public static bool TryStopDrone(int droneId)
        {
            if (_cancellationTokens.TryRemove(droneId, out var cts))
            {
                cts.Cancel();
                return true;
            }

            return false;
        }
    }
}
