using System.Windows.Input;

namespace Editor.Editors
{
    public static class TextureViewCommands
    {
        public static RoutedCommand CenterCommand { get; } = new(nameof(CenterCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.Home)
        });

        public static RoutedCommand ZoomInCommand { get; } = new(nameof(ZoomInCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.Add, ModifierKeys.Control)
        });

        public static RoutedCommand ZoomOutCommand { get; } = new(nameof(ZoomOutCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.Subtract, ModifierKeys.Control)
        });

        public static RoutedCommand ZoomFitCommand { get; } = new(nameof(ZoomFitCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.D0, ModifierKeys.Control)
        });

        public static RoutedCommand ActualSizeCommand { get; } = new(nameof(ActualSizeCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.D1, ModifierKeys.Control)
        });
    }
}
