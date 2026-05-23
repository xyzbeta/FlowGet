using System;
using System.Threading;
using System.Timers;

namespace FlowGet.Utils
{
    internal class TimerContainer
    {
        public static TimerContainer Instance { get; } = new TimerContainer();

        private readonly System.Timers.Timer _timer = new(1000);
        private readonly Lock _lock = new();
        private int _counter = 0;

        private TimerContainer()
        {
        }

        private void Refresh()
        {
            lock (_lock)
            {
                if (_counter > 0 && !_timer.Enabled)
                    _timer.Start();
                else if (_counter < 1 && _timer.Enabled)
                {
                    _timer.Stop();
                }
            }
        }

        public IDisposable AddTimerCallback(ElapsedEventHandler elapsedEventHandler)
        {
            lock (_lock)
            {
                _timer.Elapsed += elapsedEventHandler;
                _counter++;
            }
            Refresh();

            return new Disposable(() =>
            {
                lock (_lock)
                {
                    _timer.Elapsed -= elapsedEventHandler;
                    _counter--;
                }
                Refresh();
            });
        }

    }



}
