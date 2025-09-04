using PdfSharpCore.Pdf;

namespace DecMailBundle.Converters;

public static class PdfConverterFactoryExtensions
{
    public static Task Convert(this IPdfConverterFactory factory, string originalFileName, Stream stream, PdfDocument outputDocument)
    {
        var converter = factory.Create(originalFileName);

        return converter is not null 
            ? converter.Convert(originalFileName, stream, outputDocument) 
            : Task.CompletedTask;
    }
}