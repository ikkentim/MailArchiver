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

public class Archiver
{

    public Archiver(string pathRoot, bool useCurrentDate)
    {
        PathRoot = pathRoot;
        UseCurrentDate = useCurrentDate;
    }


    private string PathRoot { get; }
    private bool UseCurrentDate { get; }

    public string ConvertEmlFileToPdfInArchive(string path)
    {
        var message = MimeMessage.Load(path);

        var year = message.Date.Year;

        var yearDir = Path.Combine(PathRoot, year.ToString());
        Directory.CreateDirectory(yearDir);

        var senderAddress = message.Sender ?? message.From.First();
        var sender = ToStringFileSafe(senderAddress);
        var subject = ToFileSafe(message.Subject);

        var date = UseCurrentDate ? DateTime.Now : message.Date.LocalDateTime;

        var fileNameBase = $"{date:yyyyMMdd}_{sender}_{subject}";
        var fileName = $"{fileNameBase}.pdf";
        var outputFile = Path.Combine(yearDir, fileName);
            
        if (File.Exists(outputFile))
        {
            return outputFile + " already exists";
        }

        var fromAddress = WebUtility.HtmlEncode(message.From.ToString());
        var html = $"""
                    From: {fromAddress}<br/>
                    Date: {message.Date:yyyy-MM-dd HH:mm}<br/>
                    {message.HtmlBody ?? TextTohtml(message.TextBody)}
                    """;

        using var document = GeneratePdfFromEmailBody(html);

        using var outputDocument = new PdfDocument();
        AddPdfToPdf(document, outputDocument);

        var inlineImages = message.BodyParts.OfType<MimePart>()
            .Where(IsInlineFile)
            .ToList();

        foreach (var imagePart in inlineImages)
        {
            var contentId = imagePart.ContentId; // This matches the "cid:..." in the HTML
            var imgName = imagePart.FileName ?? $"{contentId}.{imagePart.ContentType.MediaSubtype}";

            var mem = new MemoryStream();
            imagePart.Content.DecodeTo(mem);
            mem.Position = 0;

            AddFileToPdf(imgName, mem, outputDocument);
        }

        foreach (var attachment in message.Attachments)
        {
            if(attachment is not MimePart mimePart)
                continue;
            
            using var mimeStream = mimePart.Content.Open();
            using var copiedStream = new MemoryStream();
            mimeStream.CopyTo(copiedStream);
            copiedStream.Position = 0;

            AddFileToPdf(attachment.ContentDisposition.FileName, copiedStream, outputDocument);
        }

        outputDocument.Save(outputFile);

        return outputFile;
    }

    private static bool IsInlineFile(MimePart part)
    {
        if (part.IsAttachment)
        {
            // will be handled in the attachments section; data is not inline
            return false;
        }
        if(part.ContentType == null || part.ContentDisposition != null && part.ContentDisposition.Disposition != ContentDisposition.Inline)
        {
            // not inline, or no content type
            return false;
        }

        if (part.ContentType.MediaType.StartsWith("text", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static string TextTohtml(string? text)
    {
        return text == null ? string.Empty : text.Replace("\n", "<br />").Replace("\r", "");
    }
    
    private static void AddFileToPdf(string fileName, MemoryStream copiedStream, PdfDocument outputDocument)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();

        if (ext == ".docx")
        {
            using var converted = ConvertDocxToPdf(copiedStream);
            AddPdfToPdf(converted, outputDocument);
        }
        else if (ext is ".jpg" or ".jpeg")
        {
            AddJpgToPdf(copiedStream, outputDocument);
        }
        else if (ext == ".pdf")
        {
            AddPdfToPdf(copiedStream, outputDocument);
        }
    }

    private static string ToStringFileSafe(InternetAddress addr)
    {
        var txt = addr.Name;

        if (string.IsNullOrWhiteSpace(txt))
        {
            if (addr is MailboxAddress m)
            {
                txt = m.Address;
            }
        }

        var at = txt.IndexOfAny(['@', '<']);

        if (at > 0)
        {
            txt = txt[..at];
        }

        return ToFileSafe(txt);
    }

    private static string ToFileSafe(string txt)
    {
        txt = txt.Trim();
        // Remove invalid characters for file names
        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            txt = txt.Replace(c.ToString(), string.Empty);
        }
        // Ensure the name is not too long
        if (txt.Length > 50)
        {
            txt = txt[..50];
        }

        return txt.Trim();
    }

    private static void AddJpgToPdf(Stream imageStream, PdfDocument outputDocument)
    {
        using var image = PdfSharpCore.Drawing.XImage.FromStream(() => imageStream);
        var page = outputDocument.AddPage();
        page.Width = image.PixelWidth * 72 / image.HorizontalResolution;
        page.Height = image.PixelHeight * 72 / image.VerticalResolution;
        using var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
        gfx.DrawImage(image, 0, 0, page.Width, page.Height);
    }

    private static void AddPdfToPdf(Stream document, PdfDocument outputDocument)
    {
        using var inputDocument = PdfReader.Open(document, PdfDocumentOpenMode.Import);

        for (var i = 0; i < inputDocument.PageCount; i++)
        {
            var page = inputDocument.Pages[i];
            outputDocument.AddPage(page);
        }
    }

    private static Stream GeneratePdfFromEmailBody(string html)
    {
        var mem = new MemoryStream();

        var pdf = new PdfDocument();
        PdfGenerator.AddPdfPages(pdf, html, PageSize.A4);
        pdf.Save(mem);

        mem.Position = 0;
        return mem;
    }



    private static Stream ConvertDocxToPdf(Stream stream)
    {
        using var wordDocument = new WordDocument(stream, FormatType.Docx);
        using var renderer = new DocIORenderer();

        using var pdf = renderer.ConvertToPDF(wordDocument);

        var mem = new MemoryStream();
        pdf.Save(mem);

        mem.Position = 0;

        return mem;
    }
}