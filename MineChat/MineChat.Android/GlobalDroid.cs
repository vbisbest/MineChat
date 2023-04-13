using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Diagnostics;

namespace MineChat.Droid
{
    public class GlobalDroid
    {             
        public static Xamarin.Forms.Platform.Android.FormsApplicationActivity mainActivity;
        public static MineChat.IPurchases InAppServices;
        public static PausableTimer interstatialTimer;
        public static int interstatialTimeRemaining = 0;
    }

    public class PausableTimer
    {
        private Timer _timer;
        private Stopwatch _stopWatch;
        private bool _paused;
        private double _interval;
        private double _remainingTimeBeforePause;

        public PausableTimer(double interval, ElapsedEventHandler handler)
        {
            _interval = interval;
            _stopWatch = new Stopwatch();

            _timer = new Timer(interval);
            _timer.Elapsed += (sender, arguments) =>
            {
                if (handler != null)
                {
                    if (_timer.AutoReset)
                    {
                        _stopWatch.Restart();
                    }

                    handler(sender, arguments);
                }
            };

            _timer.AutoReset = false;
        }

        public bool AutoReset
        {
            get
            {
                return _timer.AutoReset;
            }
            set
            {
                _timer.AutoReset = value;
            }
        }

        public void Start()
        {
            _timer.Interval = _interval;
            _remainingTimeBeforePause = 0;
            _timer.Start();
            _stopWatch.Restart();
        }

        public void Stop()
        {
            _timer.Stop();
            _stopWatch.Stop();
        }

        public double Interval
        {
            set
            {
                _interval = value;
            }
        }

        public void Pause()
        {
            if (!_paused && _timer.Enabled)
            {
                _stopWatch.Stop();
                _timer.Stop();
                _remainingTimeBeforePause = Math.Max(0, _interval - _stopWatch.ElapsedMilliseconds);
                _paused = true;
            }
        }

        public void Resume()
        {
            if (_paused)
            {
                _paused = false;
                if (_remainingTimeBeforePause > 0)
                {
                    _timer.Interval = _remainingTimeBeforePause;
                    _timer.Start();
                }
            }
            else
            {
                Start();
            }
        }
    }

}
