using DecMailBundle.Converters;
using MimeKit;
using PdfSharpCore.Pdf;

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

    public string PathRoot { get; }
    private bool UseCurrentDate { get; }

    public bool RootExists() => Directory.Exists(PathRoot);

    /// <summary>
    /// Converts an EML file to a PDF and saves it in the archive.
    /// </summary>
    public async Task<ArchiveResult> AddToArchive(string emlFilePath)
    {
        try
        {
            // put EML into memory - we'll be reading it multiple times
            using var emlStream = await LoadFileToMemory(emlFilePath);

            var outputFile = await ComputeOutputFileName(emlStream);

            if (File.Exists(outputFile))
            {
                return new ArchiveResult(outputFile, ArchiveResultStatus.AlreadyExists);
            }
            
            using var outputDocument = new PdfDocument();

            await PdfConverterFactory.Instance.Convert(emlFilePath, emlStream, outputDocument);

            outputDocument.Save(outputFile);
            return new ArchiveResult(outputFile, ArchiveResultStatus.Created);
        }
        catch (Exception ex)
        {
            return new ArchiveResult(emlFilePath, ArchiveResultStatus.Error, ex.Message);
        }
    }

    private static async Task<MemoryStream> LoadFileToMemory(string emlFilePath)
    {
        var emlStream = MemoryStreamManager.Get();
        await using (var fileStream = File.OpenRead(emlFilePath))
        {
            await fileStream.CopyToAsync(emlStream);
        }
        emlStream.Position = 0;
        return emlStream;
    }

    private async Task<string> ComputeOutputFileName(MemoryStream emlStream)
    {
        var message = await MimeMessage.LoadAsync(emlStream);
        var yearDir = EnsureYearDirectory(message.Date.Year);

        var fileName = ComputeOutputFileName(message);
        var outputFile = PathHelper.NormalizePath(Path.Combine(yearDir, fileName));
        
        emlStream.Position = 0;

        return outputFile;
    }

    /// <summary>
    /// Builds a file name for the PDF based on the email's sender, subject, and date.
    /// </summary>
    private string ComputeOutputFileName(MimeMessage message)
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

    public string GetCurrentYearPath()
    {
        var year = DateTime.Now.Year;
        return PathHelper.NormalizePath(EnsureYearDirectory(year));
    }
}