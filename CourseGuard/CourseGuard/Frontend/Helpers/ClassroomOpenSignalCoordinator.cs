using System;
using System.Threading.Tasks;
using CourseGuard.Backend.Services.Realtime;

namespace CourseGuard.Frontend.Helpers
{
    internal sealed class ClassroomOpenSignalCoordinator
    {
        private readonly IClassroomSignalService _signalService;
        private readonly object _startLock = new();
        private bool _hasStartedListening;

        public ClassroomOpenSignalCoordinator(IClassroomSignalService signalService)
        {
            _signalService = signalService ?? throw new ArgumentNullException(nameof(signalService));
        }

        public async Task BroadcastClassOpenedAsync(int sessionId)
        {
            EnsureListeningStarted();
            await _signalService.BroadcastClassOpened(sessionId);
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
    }
}
