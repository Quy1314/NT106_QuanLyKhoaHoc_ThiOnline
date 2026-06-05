using System;
using System.Threading.Tasks;
using CourseGuard.Backend.Services.Realtime;

namespace CourseGuard.Frontend.Helpers
{
    internal sealed class ClassroomOpenSignalCoordinator
    {
        private static readonly TimeSpan DefaultReplayDelay = TimeSpan.FromMilliseconds(750);

        private readonly IClassroomSignalService _signalService;
        private readonly int _replayCount;
        private readonly TimeSpan _replayDelay;
        private readonly object _startLock = new();
        private bool _hasStartedListening;

        public ClassroomOpenSignalCoordinator(
            IClassroomSignalService signalService,
            int replayCount = 3,
            TimeSpan? replayDelay = null)
        {
            _signalService = signalService ?? throw new ArgumentNullException(nameof(signalService));
            if (replayCount < 0)
                throw new ArgumentOutOfRangeException(nameof(replayCount));

            TimeSpan resolvedReplayDelay = replayDelay ?? DefaultReplayDelay;
            if (resolvedReplayDelay < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(replayDelay));

            _replayCount = replayCount;
            _replayDelay = resolvedReplayDelay;
        }

        public async Task BroadcastClassOpenedAsync(int sessionId)
        {
            EnsureListeningStarted();
            await _signalService.BroadcastClassOpened(sessionId);
            QueueClassOpenedReplays(sessionId);
        }

        public void EnsureListeningStarted()
        {
            if (!_hasStartedListening)
            {
                lock (_startLock)
                {
                    if (_hasStartedListening)
                        return;

                    _signalService.StartListening();
                    _hasStartedListening = true;
                }
            }
        }

        private void QueueClassOpenedReplays(int sessionId)
        {
            if (_replayCount == 0)
                return;

            _ = ReplayClassOpenedAsync(sessionId);
        }

        private async Task ReplayClassOpenedAsync(int sessionId)
        {
            try
            {
                for (int attempt = 0; attempt < _replayCount; attempt++)
                {
                    await Task.Delay(_replayDelay).ConfigureAwait(false);
                    await _signalService.BroadcastClassOpened(sessionId).ConfigureAwait(false);
                }
            }
            catch
            {
                // The initial broadcast has already completed. Replay failures should
                // not interrupt opening the class form.
            }
        }
    }
}
