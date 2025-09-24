using Editor.Common;
using Editor.Common.Enums;
using Editor.Utilities;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace Editor.Content.ImportSettingsConfig;

class ImportingItem : ViewModelBase
{
    private ImportStatus _status;
    private double _progressMax;
    private double _progressValue;
    private double _normalizedValue;
    private DispatcherTimer? _timer;
    private Stopwatch? _stopwatch;
    private string? _importDuration;

    public string Name { get; }
    public Asset Asset { get; }

    public ImportStatus Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
    }

    public double ProgressMax
    {
        get => _progressMax;
        private set
        {
            if (!_progressMax.IsEqual(value))
            {
                _progressMax = value;
                OnPropertyChanged(nameof(ProgressMax));
            }
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        private set
        {
            if (!_progressValue.IsEqual(value))
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }
    }

    public double NormalizedValue
    {
        get => _normalizedValue;
        private set
        {
            if (!_normalizedValue.IsEqual(value))
            {
                _normalizedValue = value;
                OnPropertyChanged(nameof(NormalizedValue));
            }
        }
    }

    public string ImportDuration
    {
        get => _importDuration!;
        private set
        {
            if (_importDuration != value)
            {
                _importDuration = value;
                OnPropertyChanged(nameof(ImportDuration));
            }
        }
    }

    public ImportingItem(string name, Asset asset)
    {
        Debug.Assert(!string.IsNullOrEmpty(name) && asset is not null);

        Asset = asset;
        Name = name;

        Application.Current.Dispatcher.Invoke(() =>
        {
            _stopwatch = new();
            _timer = new();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += UpdateTimer;
            _timer.Start();
        });
    }

    public void SetProgress(int progress, int maxValue)
    {
        ProgressMax = maxValue;
        ProgressValue = progress;
        NormalizedValue = maxValue > 0 ? Math.Clamp(progress / maxValue, 0, 1) : 0.0;
    }

    private void UpdateTimer(object? sender, EventArgs e)
    {
        if (Status == ImportStatus.IMPORTING)
        {
            if (!_stopwatch!.IsRunning) _stopwatch.Start();

            var t = _stopwatch.Elapsed;

            ImportDuration = string.Format("{0:00}:{1:00}:{2:00}", t.Minutes, t.Seconds, t.Milliseconds / 10);
        }
        else
        {
            _timer?.Stop();
            _stopwatch?.Stop();
        }
    }
}
