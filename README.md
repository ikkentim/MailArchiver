Mail Archiver
=============
Stupidly simple mail archiving tool. Drag .eml file from email client into the application, and it'll be archived. The
archived file is a PDF containing the mail body and attachments.

Currently supported attachment file types:
- PDF
- JPG
- DOCX

Application requires a SyncFusion license key to run. Create a `Licenses.cs` file in the project with the following content:
```csharp
namespace DecMailBundle;

internal static class Licences
{
    public static string SyncFusion => "your-license-key";
}
```