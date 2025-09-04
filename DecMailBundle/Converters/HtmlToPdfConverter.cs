using System.Text;
using PdfSharpCore.Pdf;
using Westwind.WebView.HtmlToPdf;

namespace DecMailBundle.Converters;

public class HtmlToPdfConverter(IPdfConverterFactory factory) : IPdfConverter
{
    public async Task Convert(string originalFileName, Stream stream, PdfDocument outputDocument)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        using var pdfStream = MemoryStreamManager.Get();

        await HtmlToPdfStream(await reader.ReadToEndAsync(), pdfStream);

        pdfStream.Position = 0;

        await factory.Convert($"{originalFileName}.pdf", pdfStream, outputDocument);
    }

    private static async Task HtmlToPdfStream(string html, Stream destination)
    {
        using var inputFile = AppDataPath.GetTempFile(".html");
        await using (var htmlFileStream = inputFile.Info.OpenWrite())
        {
            await using (var textWriter = new StreamWriter(htmlFileStream))
            {
                await textWriter.WriteAsync(html);
            }
        }
        
        using var outputFile = AppDataPath.GetTempFile(".pdf");

        var pdfHost = new HtmlToPdfHost()
        {
            WebViewEnvironmentPath = AppDataPath.WebViewData
        };

        var result = await pdfHost.PrintToPdfAsync(inputFile.Info.FullName, outputFile.Info.FullName, new WebViewPrintSettings
        {
            PageHeight = 11.7, // A4 paper size
            PageWidth = 8.3,
            MarginTop = 0.2,
            MarginBottom = 0.2,
            MarginLeft = 0.2,
            MarginRight = 0.2,
        });

        if (result.IsSuccess)
        {
            await using var outputStream = outputFile.Info.OpenRead();
            await outputStream.CopyToAsync(destination);
        }
        else
        {
            throw new InvalidOperationException("Failed to print HTML to PDF");
        }
    }
}