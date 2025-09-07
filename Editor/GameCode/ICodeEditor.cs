using Editor.Common.Enums;
using Editor.GameProject;

namespace Editor.GameCode
{
    public interface ICodeEditor
    {
        public bool CanDebug { get; }

        private static ICodeEditor? _current;

        public static ICodeEditor Current
        {
            get => _current!;
            set => _current ??= value;
        }

        public static void SetCurrent(CodeEditor editor) => _current = editor switch
        {
            CodeEditor.VISUAL_STUDIO => new VisualStudio(),
            CodeEditor.NOTEPAD => new Notepad(),
            _ => new Notepad(),
        };

        public void Initialize(string solution);
        public void ShowWindow(string solution, string? file = null);
        public void Close();
        public bool IsDebugging();
        public void Run(bool debug);
        public void Stop();

    }
}
