using System.Net;
using System.Text;
using MimeKit;
using PdfSharpCore.Pdf;

namespace DecMailBundle.Converters;

public class EmlToPdfConverter(IPdfConverterFactory factory) : IPdfConverter
{
    public async Task Convert(string originalFileName, Stream stream, PdfDocument outputDocument)
    {
        var message = await MimeMessage.LoadAsync(stream);

        await AddBody(message, outputDocument);
        await AddInlineFiles(message, outputDocument);
        await AddAttachments(message, outputDocument);
    }
    
    /// <summary>
    /// Generates a PDF stream from HTML content
    /// </summary>
    private async Task AddBody(MimeMessage message, PdfDocument outputDocument)
    {
        var html = BuildEmailHtml(message);
        var htmlStream = MemoryStreamManager.Get();
        await using (var sw = new StreamWriter(htmlStream, Encoding.UTF8, leaveOpen: true))
        {
            await sw.WriteAsync(html);
        }

        htmlStream.Position = 0;
        await factory.Convert("body.html", htmlStream, outputDocument);
    }

    /// <summary>
    /// Builds the HTML representation of the email.
    /// </summary>
    private static string BuildEmailHtml(MimeMessage message)
    {
        var fromAddress = WebUtility.HtmlEncode(message.From.ToString());
        var bodyHtml = message.HtmlBody ?? TextToHtml(message.TextBody);

        return $"""
                From: {fromAddress}<br/>
                Date: {message.Date:yyyy-MM-dd HH:mm}<br/>
                {bodyHtml}
                """;
    }
    
    /// <summary>
    /// Converts plain text to HTML by replacing newlines with &lt;br /&gt;
    /// </summary>
    private static string TextToHtml(string? text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\n", "<br />").Replace("\r", "");
    }

    private async Task AddInlineFiles(MimeMessage message, PdfDocument outputDocument)
    {
        var inlineFiles = message.BodyParts.OfType<MimePart>().Where(IsInlineFile);
        foreach (var mimePart in inlineFiles)
        {
            var imgName = mimePart.FileName ?? $"{mimePart.ContentId}.{mimePart.ContentType.MediaSubtype}";
            using var stream = MemoryStreamManager.Get();
            await mimePart.Content.DecodeToAsync(stream);
            stream.Position = 0;
            await factory.Convert(imgName, stream, outputDocument);
        }
    }

    private async Task AddAttachments(MimeMessage message, PdfDocument outputDocument)
    {
        foreach (var attachment in message.Attachments)
        {
            if (attachment is not MimePart mimePart)
            {
                continue;
            }

            await using var mimeStream = mimePart.Content.Open();
            using var copiedStream = MemoryStreamManager.Get();
            await mimeStream.CopyToAsync(copiedStream);
            copiedStream.Position = 0;

            await factory.Convert(GetAttachmentName(attachment), copiedStream, outputDocument);
        }
    }

    private static string GetAttachmentName(MimeEntity attachment)
    {
        var name = attachment.ContentDisposition?.FileName;

        name ??= attachment.ContentType.Name;
        return name ?? "attachment";
    }

    /// <summary>
    /// Determines if a MimePart is an inline file (not an attachment, not text, and has correct disposition)
    /// </summary>
    private static bool IsInlineFile(MimePart part)
    {
        if (part.IsAttachment)
        {
            return false; // Attachments are handled elsewhere
        }

        if (part.ContentType == null)
        {
            return false;
        }

        if (part.ContentDisposition != null && part.ContentDisposition.Disposition != ContentDisposition.Inline)
        {
            return false;
        }

        if (part.ContentType.MediaType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

}