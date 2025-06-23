namespace DecMailBundle.FormComponents;

public class ImageAndTextCell : DataGridViewTextBoxCell
{
    public const int ImageOffset = 0;

    protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex,
        DataGridViewElementStates cellState, object? value, object? formattedValue, string? errorText,
        DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle,
        DataGridViewPaintParts paintParts)
    {
        base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, string.Empty, errorText, cellStyle,
            advancedBorderStyle, paintParts);

        var parent = (ImageAndTextColumn)OwningColumn!;
        var icon = parent.GetImage(rowIndex);
        
        var textColor = parent.InheritedStyle!.ForeColor;
        if ((cellState & DataGridViewElementStates.Selected) == DataGridViewElementStates.Selected)
        {
            textColor = parent.InheritedStyle.SelectionForeColor;
        }
        
        var valBounds = CalcValueBounds(advancedBorderStyle, cellBounds);
        
        var verticalTextMarginTop = cellStyle.WrapMode == DataGridViewTriState.True ? 1 : 2;
        valBounds.Offset(0, verticalTextMarginTop);
        valBounds.Height -= verticalTextMarginTop + 1;

        if (valBounds.Width > 0 && valBounds.Height > 0)
        {
            var flags = ComputeTextFormatFlagsForCellStyleAlignment(cellStyle.Alignment, cellStyle.WrapMode);

            if ((flags & TextFormatFlags.SingleLine) != 0)
            {
                flags |= TextFormatFlags.EndEllipsis;
            }

            if (icon != null)
            {
                var imageY = valBounds.Y;
                if ((flags & TextFormatFlags.VerticalCenter) != 0)
                {
                    imageY += valBounds.Height / 2 - icon.Height / 2;
                }
                else if ((flags & TextFormatFlags.Bottom) != 0)
                {
                    imageY += valBounds.Height - icon.Height;
                }

                var iconBounds = new Rectangle(valBounds.X, imageY, icon.Width, icon.Height);
                graphics.DrawIcon(icon, iconBounds);

                valBounds = valBounds with
                {
                    X = valBounds.X + ImageOffset + icon.Width, Width = valBounds.Width - ImageOffset - icon.Width
                };
            }

            if (valBounds is { Width: > 0, Height: > 0 } && parent.RenderText)
            {
                TextRenderer.DrawText(graphics, formattedValue?.ToString(), cellStyle.Font, valBounds, textColor, flags);
            }
        }
    }

    private Rectangle CalcValueBounds(DataGridViewAdvancedBorderStyle advancedBorderStyle, Rectangle cellBounds)
    {
        var borderWidths = BorderWidths(advancedBorderStyle);
        var valBounds = cellBounds;
        valBounds.Offset(borderWidths.X, borderWidths.Y);
        valBounds.Width -= borderWidths.Right;
        valBounds.Height -= borderWidths.Bottom;
        return valBounds;
    }

    private static TextFormatFlags ComputeTextFormatFlagsForCellStyleAlignment(DataGridViewContentAlignment alignment, DataGridViewTriState wrapMode)
    {
        TextFormatFlags tff;
        switch (alignment)
        {
            case DataGridViewContentAlignment.TopLeft:
                tff = TextFormatFlags.Top;
                tff |= TextFormatFlags.Left;
                break;
            case DataGridViewContentAlignment.TopCenter:
                tff = TextFormatFlags.Top | TextFormatFlags.HorizontalCenter;
                break;
            case DataGridViewContentAlignment.TopRight:
                tff = TextFormatFlags.Top;
                tff |= TextFormatFlags.Right;
                break;
            case DataGridViewContentAlignment.MiddleLeft:
                tff = TextFormatFlags.VerticalCenter;
                tff |= TextFormatFlags.Left;
                break;
            case DataGridViewContentAlignment.MiddleCenter:
                tff = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter;
                break;
            case DataGridViewContentAlignment.MiddleRight:
                tff = TextFormatFlags.VerticalCenter;
                tff |= TextFormatFlags.Right;
                break;
            case DataGridViewContentAlignment.BottomLeft:
                tff = TextFormatFlags.Bottom;
                tff |= TextFormatFlags.Left;
                break;
            case DataGridViewContentAlignment.BottomCenter:
                tff = TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter;
                break;
            case DataGridViewContentAlignment.BottomRight:
                tff = TextFormatFlags.Bottom;
                tff |= TextFormatFlags.Right;
                break;
            default:
                tff = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
                break;
        }

        if (wrapMode == DataGridViewTriState.False)
        {
            tff |= TextFormatFlags.SingleLine;
        }
        else
        {
            tff |= TextFormatFlags.WordBreak;
        }

        tff |= TextFormatFlags.NoPrefix;
        tff |= TextFormatFlags.PreserveGraphicsClipping;

        return tff;
    }
}