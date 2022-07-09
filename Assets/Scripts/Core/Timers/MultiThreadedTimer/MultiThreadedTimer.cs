using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;


namespace RML
{
    public class MultiThreadedTimer : IDisposable
    {
        private float _duration;
        
        private Timer _timer;

        private Action _tickCallback;
        private Action _onCompletedCallback;

        private DateTime _startTime;
        private DateTime _stopTime;

        private TimeSpan _leftTime;
        
        private SynchronizationContext _synchronizationContext;
        private bool _completed;

        public TimeSpan GetLeftTime => _leftTime;
        
        public MultiThreadedTimer(SynchronizationContext synchronizationContext,float duration, float intervalBetweenTicks, Action tickCallback,
            Action onCompletedCallback)
        {
            _synchronizationContext = synchronizationContext;
            _duration = duration;
            _tickCallback = tickCallback;
            _onCompletedCallback = onCompletedCallback;

            _timer = new Timer {Interval = intervalBetweenTicks};
            _timer.Elapsed += TimerOnElapsed;

            _timer.AutoReset = true;
            
            _leftTime = TimeSpan.Zero;
            _stopTime = DateTime.MinValue;
            _startTime = DateTime.MinValue;
        }

        public void Start()
        {
            _completed = false;
            
            _startTime = DateTime.Now;

            _stopTime = _startTime + TimeSpan.FromMilliseconds(_duration);
            
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
        
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _leftTime = _stopTime - e.SignalTime;

            if (_leftTime <= TimeSpan.Zero)
            {
                _leftTime = TimeSpan.Zero;
                _completed = true;
            } 
            
            _synchronizationContext.Post(state => _tickCallback.Invoke(), null);

            if (_completed)
            {
                _synchronizationContext.Post(state => _onCompletedCallback.Invoke(), null);
                _timer.Stop();
            }
        }

        public void Dispose()
        {
           _timer.Stop();
           _timer.Dispose();
        }
    }

}
