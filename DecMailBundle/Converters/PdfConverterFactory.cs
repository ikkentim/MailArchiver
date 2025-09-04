namespace DecMailBundle.Converters;

public class PdfConverterFactory : IPdfConverterFactory
{
    public static IPdfConverterFactory Instance { get; } = new PdfConverterFactory();

    public IPdfConverter? Create(string originalFileName)
    {
        var ext = Path.GetExtension(originalFileName)?.ToLowerInvariant();

        return ext switch
        {
            ".eml" => new EmlToPdfConverter(this),
            ".jpg" or ".jpeg" => new JpegToPdfConverter(),
            ".docx" when Licenses.SyncFusion != null => new DocxToPdfConverter(this),
            ".html" or ".htm" => new HtmlToPdfConverter(this),
            ".pdf" => new PdfToPdfConverter(),
            _ => null
        };
    }
}