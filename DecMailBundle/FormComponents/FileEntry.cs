using System.ComponentModel;

namespace DecMailBundle.FormComponents;


public record FileEntry([property:Browsable(false)]string FilePath, string DateModified)
{
    public string FileName { get; } = Path.GetFileName(FilePath);
}