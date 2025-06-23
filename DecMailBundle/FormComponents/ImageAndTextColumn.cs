namespace DecMailBundle.FormComponents;

public abstract class ImageAndTextColumn : DataGridViewColumn
{
    protected ImageAndTextColumn()
    {
        CellTemplate = new ImageAndTextCell();
        ReadOnly = true;
    }

    protected abstract Icon? GetImage(DataGridViewRow? row);

    public virtual bool RenderText => true;

    public Icon? GetImage(int rowIndex)
    {
        var row = DataGridView != null && DataGridView.Rows.Count > rowIndex && rowIndex >= 0
            ? DataGridView.Rows[rowIndex]
            : null;

        return GetImage(row);
    }
}