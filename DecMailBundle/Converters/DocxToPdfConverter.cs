using PdfSharpCore.Pdf;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;

namespace DecMailBundle.Converters;

public class DocxToPdfConverter(IPdfConverterFactory factory) : IPdfConverter
{
    public async Task Convert(string originalFileName, Stream stream, PdfDocument outputDocument)
    {
        using var wordDocument = new WordDocument(stream, FormatType.Docx);
        using var renderer = new DocIORenderer();
        using var pdf = renderer.ConvertToPDF(wordDocument);

        using var pdfStream = MemoryStreamManager.Get();
        pdf.Save(pdfStream);
        pdfStream.Position = 0;

        var pdfName = $"{originalFileName}.pdf";
        await factory.Convert(pdfName, pdfStream, outputDocument);
    }
}