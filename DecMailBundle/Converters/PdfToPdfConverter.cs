using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace DecMailBundle.Converters;

public class PdfToPdfConverter : IPdfConverter
{
    public Task Convert(string originalFileName, Stream stream, PdfDocument outputDocument)
    {
        using var inputDocument = PdfReader.Open(stream, PdfDocumentOpenMode.Import);
        for (var i = 0; i < inputDocument.PageCount; i++)
        {
            var page = inputDocument.Pages[i];
            outputDocument.AddPage(page);
        }

        return Task.CompletedTask;
    }
}