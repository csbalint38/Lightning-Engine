namespace Editor.GameCode
{
    public interface ICodeEditor
    {
        private static ICodeEditor? _current;

        public static ICodeEditor Current {
            get => _current!;
            set => _current ??= value;
        }

        public void Initialize(string solution);
        public void ShowWindow(string solution, string? file = null);
        public void Close();
        public bool IsDebugging();
        public void Run(bool debug);
        public void Stop();

    }
}
