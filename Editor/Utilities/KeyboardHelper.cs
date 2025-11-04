using Editor.Common.Enums;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Editor.Utilities;

public static partial class KeyboardHelper
{
    [LibraryImport("user32.dll")]
    public static partial short GetAsyncKeyState(VKey vKey);

    private static readonly Array _keys = Enum.GetValues<Key>();

    public static IEnumerable<Key> KeyDown()
    {
        foreach (Key key in _keys)
        {
            if (key != Key.None && Keyboard.IsKeyDown(key)) yield return key;
        }
    }
}
