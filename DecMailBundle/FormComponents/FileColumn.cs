namespace DecMailBundle.FormComponents;

public class FileColumn : ImageAndTextColumn
{
    private static Dictionary<string, Icon> _iconCache = [];
    protected override Icon? GetImage(DataGridViewRow? row)
    {
        if (row?.DataBoundItem is FileEntry entry)
        {
            return FileIconHelper.GetIcon(entry.FilePath);
        }

        return null;
    }
}