using System.Windows.Input;

namespace Editor.Editors
{
    public static class TextureViewCommands
    {
        public static readonly RoutedCommand CenterCommand = new(nameof(CenterCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.Home)
        });

        public static readonly RoutedCommand ZoomInCommand = new(nameof(ZoomInCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.Add, ModifierKeys.Control)
        });

        public static readonly RoutedCommand ZoomOutCommand = new(nameof(ZoomOutCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.Subtract, ModifierKeys.Control)
        });

        public static readonly RoutedCommand ZoomFitCommand = new(nameof(ZoomFitCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.D0, ModifierKeys.Control)
        });

        public static readonly RoutedCommand ActualSizeCommand = new(nameof(ActualSizeCommand), typeof(TextureEditorView), new()
        {
            new KeyGesture(Key.D1, ModifierKeys.Control)
        });
    }
}
