using Editor.Common;
using Editor.Common.Enums;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;

namespace Editor.Utilities
{
    public static class Logger
    {
        private static readonly ObservableCollection<LogMessage> _messages = [];
        private static int _messageFilter = (int)(LogLevel.INFO | LogLevel.WARNING | LogLevel.ERROR);

        public static ReadOnlyObservableCollection<LogMessage> Messages { get; } = new(_messages);
        public static CollectionViewSource FilteredMessages { get; } = new() { Source = Messages };

        public static async void Log(
            LogLevel level,
            string message,
            [CallerFilePath] string file = "",
            [CallerMemberName] string caller = "",
            [CallerLineNumber] int line = 0
        ) => await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            _messages.Add(new LogMessage(level, message, file, caller, line));
        }));

        public static async void Clear() => await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            _messages.Clear();
        }));

        public static void SetMessageFilter(int mask)
        {
            _messageFilter = mask;

            FilteredMessages.View.Refresh();
        }

        static Logger()
        {
            FilteredMessages.Filter += (s, e) =>
            {
                var type = (int)(e.Item as LogMessage).LogLevel;
                e.Accepted = (type & _messageFilter) != 0;
            };
        }
    }
}
