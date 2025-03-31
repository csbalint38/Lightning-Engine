using Editor.Utilities.Descriptors;
using System.Windows.Threading;

namespace Editor.Utilities
{
    internal class DelayEventTimer
    {
        private readonly DispatcherTimer _timer;
        private readonly TimeSpan _delay;
        private DateTime _lastEventTime = DateTime.Now;
        private object _data;

        public event EventHandler<DelayEventTimerArgs> Triggered;

        public DelayEventTimer(TimeSpan delay, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _delay = delay;
            _timer = new DispatcherTimer(priority)
            {
                Interval = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * .5)
            };
            _timer.Tick += OnTick;
        }

        public void Trigger(object data = null)
        {
            _data = data;
            _lastEventTime = DateTime.Now;
            _timer.IsEnabled = true;
        }

        public void Disable() => _timer.IsEnabled = false;

        private void OnTick(object sender, EventArgs e)
        {
            if ((DateTime.Now - _lastEventTime) < _delay) return;

            var eventArgs = new DelayEventTimerArgs(_data);
            Triggered?.Invoke(this, eventArgs);
            _timer.IsEnabled = eventArgs.RepeatEvent;
        }
    }
}
