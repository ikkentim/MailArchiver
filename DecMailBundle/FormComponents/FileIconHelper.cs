using Windows.Win32;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.Shell;

namespace DecMailBundle.FormComponents;

public static class FileIconHelper
{
    private static readonly Dictionary<string, Icon> _knownIcons = new();

    public static unsafe Icon? GetIcon(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }
        
        // Assuming we're on the main thread - no locking required.
        if (_knownIcons.TryGetValue(filePath, out var icon))
        {
            return icon;
        }

        var f = new SHFILEINFOW();
        var fs = (uint)System.Runtime.InteropServices.Marshal.SizeOf(f);
        PInvoke.SHGetFileInfo(filePath, FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL, &f, fs,
            SHGFI_FLAGS.SHGFI_ICON | SHGFI_FLAGS.SHGFI_USEFILEATTRIBUTES | SHGFI_FLAGS.SHGFI_SMALLICON);
        
        icon = (Icon)Icon.FromHandle(f.hIcon).Clone();
        PInvoke.DestroyIcon(f.hIcon);
        _knownIcons[filePath] = icon;
        return icon;
    }
}