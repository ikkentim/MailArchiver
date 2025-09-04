using PdfSharpCore.Pdf;

namespace DecMailBundle.Converters;

public interface IPdfConverter
{
    Task Convert(string originalFileName, Stream stream, PdfDocument outputDocument);
}