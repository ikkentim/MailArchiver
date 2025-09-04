using PdfSharpCore.Pdf;

namespace DecMailBundle.Converters;

public class JpegToPdfConverter : IPdfConverter
{
    public Task Convert(string originalFileName, Stream stream, PdfDocument outputDocument)
    {
        using var image = PdfSharpCore.Drawing.XImage.FromStream(() => stream);
        var page = outputDocument.AddPage();
        page.Width = image.PixelWidth * 72 / image.HorizontalResolution;
        page.Height = image.PixelHeight * 72 / image.VerticalResolution;
        using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
        gfx.DrawImage(image, 0, 0, page.Width, page.Height);

        return Task.CompletedTask;
    }
}