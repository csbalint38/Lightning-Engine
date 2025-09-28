using Editor.Utilities.Descriptors;
using System.Windows.Threading;

namespace Editor.Utilities
{
    internal class DelayEventTimer
    {
        private readonly DispatcherTimer _timer;
        private readonly TimeSpan _delay;
        private readonly List<object> _data = [];
        private DateTime _lastEventTime = DateTime.Now;

        public event EventHandler<DelayEventTimerArgs>? Triggered;

        public DelayEventTimer(TimeSpan delay, DispatcherPriority priority = DispatcherPriority.Normal)
        {
            _delay = delay;
            _timer = new DispatcherTimer(priority)
            {
                Interval = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * .5)
            };
            _timer.Tick += OnTick;
        }

        public void Trigger(object? data = null)
        {
            if (data is not null) _data.Add(data);

            _lastEventTime = DateTime.Now;
            _timer.IsEnabled = true;
        }

        public void Disable() => _timer.IsEnabled = false;

        private void OnTick(object? sender, EventArgs e)
        {
            if ((DateTime.Now - _lastEventTime) < _delay) return;

            var eventArgs = new DelayEventTimerArgs(_data);
            Triggered?.Invoke(this, eventArgs);

            if (!eventArgs.RepeatEvent) _data.Clear();

            _timer.IsEnabled = eventArgs.RepeatEvent;
        }
    }
}
