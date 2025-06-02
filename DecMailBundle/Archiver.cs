using MimeKit;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.Net;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using PageSize = PdfSharpCore.PageSize;

namespace DecMailBundle;

/// <summary>
/// Handles archiving of EML files to PDF format.
/// </summary>
public class Archiver
{
    public Archiver(string pathRoot, bool useCurrentDate)
    {
        PathRoot = pathRoot;
        UseCurrentDate = useCurrentDate;
    }

    private string PathRoot { get; }
    private bool UseCurrentDate { get; }

    /// <summary>
    /// Converts an EML file to a PDF and saves it in the archive.
    /// </summary>
    public string ConvertEmlFileToPdfInArchive(string emlFilePath)
    {
        var message = MimeMessage.Load(emlFilePath);
        var yearDir = EnsureYearDirectory(message.Date.Year);

        var fileName = BuildFileName(message);
        var outputFile = Path.Combine(yearDir, fileName);

        if (File.Exists(outputFile))
        {
            return outputFile + " already exists";
        }

        var html = BuildEmailHtml(message);
        using var emailBodyPdf = GeneratePdfFromEmailBody(html);
        using var outputDocument = new PdfDocument();

        AddPdfToPdfDocument(emailBodyPdf, outputDocument);
        AddInlineFilesToPdf(message, outputDocument);
        AddAttachmentsToPdf(message, outputDocument);

        outputDocument.Save(outputFile);
        return outputFile;
    }

    /// <summary>
    /// Builds a file name for the PDF based on the email's sender, subject, and date.
    /// </summary>
    private string BuildFileName(MimeMessage message)
    {
        var sender = ToStringFileSafe(message.Sender ?? message.From.First());
        var subject = ToFileSafe(message.Subject);
        var date = UseCurrentDate ? DateTime.Now : message.Date.LocalDateTime;
        var fileName = $"{date:yyyyMMdd}_{sender}_{subject}.pdf";
        return fileName;
    }

    /// <summary>
    /// Ensures the year directory exists and returns its path.
    /// </summary>
    private string EnsureYearDirectory(int year)
    {
        var yearDir = Path.Combine(PathRoot, year.ToString());
        Directory.CreateDirectory(yearDir);
        return yearDir;
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
    /// Adds inline files from the email to the PDF document.
    /// </summary>
    private static void AddInlineFilesToPdf(MimeMessage message, PdfDocument outputDocument)
    {
        var inlineFiles = message.BodyParts.OfType<MimePart>().Where(IsInlineFile);
        foreach (var mimePart in inlineFiles)
        {
            var imgName = mimePart.FileName ?? $"{mimePart.ContentId}.{mimePart.ContentType.MediaSubtype}";
            using var stream = new MemoryStream();
            mimePart.Content.DecodeTo(stream);
            stream.Position = 0;
            AddFileToPdf(imgName, stream, outputDocument);
        }
    }

    /// <summary>
    /// Adds attachments from the email to the PDF document.
    /// </summary>
    private static void AddAttachmentsToPdf(MimeMessage message, PdfDocument outputDocument)
    {
        foreach (var attachment in message.Attachments)
        {
            if (attachment is not MimePart mimePart)
            {
                continue;
            }

            using var mimeStream = mimePart.Content.Open();
            using var copiedStream = new MemoryStream();
            mimeStream.CopyTo(copiedStream);
            copiedStream.Position = 0;

            AddFileToPdf(attachment.ContentDisposition?.FileName ?? "attachment", copiedStream, outputDocument);
        }
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

    /// <summary>
    /// Converts plain text to HTML by replacing newlines with &lt;br /&gt;
    /// </summary>
    private static string TextToHtml(string? text)
    {
        return string.IsNullOrEmpty(text) ? string.Empty : text.Replace("\n", "<br />").Replace("\r", "");
    }
    
    /// <summary>
    /// Adds a file (by extension) to the PDF document
    /// </summary>
    private static void AddFileToPdf(string fileName, MemoryStream copiedStream, PdfDocument outputDocument)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(ext))
        {
            return;
        }

        switch (ext)
        {
            case ".docx":
                using (var converted = ConvertDocxToPdf(copiedStream))
                {
                    AddPdfToPdfDocument(converted, outputDocument);
                }
                break;
            case ".jpg":
            case ".jpeg":
                AddJpgToPdfDocument(copiedStream, outputDocument);
                break;
            case ".pdf":
                AddPdfToPdfDocument(copiedStream, outputDocument);
                break;
            // Optionally: handle other file types or log unsupported
        }
    }

    /// <summary>
    /// Returns a file-system-safe string from an InternetAddress
    /// </summary>
    private static string ToStringFileSafe(InternetAddress addr)
    {
        var txt = addr.Name;
        if (string.IsNullOrWhiteSpace(txt) && addr is MailboxAddress m)
        {
            txt = m.Address;
        }
        var at = txt.IndexOfAny(['@', '<']);
        if (at > 0)
        {
            txt = txt[..at];
        }
        return ToFileSafe(txt);
    }

    /// <summary>
    /// Returns a file-system-safe string (removes invalid chars, trims, limits length)
    /// </summary>
    private static string ToFileSafe(string txt)
    {
        txt = txt.Trim();
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            txt = txt.Replace(c.ToString(), string.Empty);
        }
        if (txt.Length > 50)
        {
            txt = txt[..50];
        }
        return txt.Trim();
    }

    /// <summary>
    /// Adds a JPG image stream as a page to the PDF document
    /// </summary>
    private static void AddJpgToPdfDocument(Stream imageStream, PdfDocument outputDocument)
    {
        using var image = PdfSharpCore.Drawing.XImage.FromStream(() => imageStream);
        var page = outputDocument.AddPage();
        page.Width = image.PixelWidth * 72 / image.HorizontalResolution;
        page.Height = image.PixelHeight * 72 / image.VerticalResolution;
        using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
        gfx.DrawImage(image, 0, 0, page.Width, page.Height);
    }

    /// <summary>
    /// Appends all pages from a PDF stream to the output PDF document
    /// </summary>
    private static void AddPdfToPdfDocument(Stream document, PdfDocument outputDocument)
    {
        using var inputDocument = PdfReader.Open(document, PdfDocumentOpenMode.Import);
        for (var i = 0; i < inputDocument.PageCount; i++)
        {
            var page = inputDocument.Pages[i];
            outputDocument.AddPage(page);
        }
    }

    /// <summary>
    /// Generates a PDF stream from HTML content
    /// </summary>
    private static Stream GeneratePdfFromEmailBody(string html)
    {
        var stream = new MemoryStream();
        var pdf = new PdfDocument();
        PdfGenerator.AddPdfPages(pdf, html, PageSize.A4);
        pdf.Save(stream);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Converts a DOCX stream to a PDF stream
    /// </summary>
    private static Stream ConvertDocxToPdf(Stream docxStream)
    {
        using var wordDocument = new WordDocument(docxStream, FormatType.Docx);
        using var renderer = new DocIORenderer();
        using var pdf = renderer.ConvertToPDF(wordDocument);

        var pdfStream = new MemoryStream();
        pdf.Save(pdfStream);
        pdfStream.Position = 0;
        return pdfStream;
    }
}