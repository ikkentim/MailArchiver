namespace DecMailBundle;

public class TempFile : IDisposable
{
    public TempFile(string path)
    {
        Info = new FileInfo(path);
    }

    ~TempFile()
    {
        Dispose();
    }
    
    public FileInfo Info { get; }

    public void Dispose()
    {
        try
        {
            if (Info.Exists)
            {
                Info.Delete();
            }

            GC.SuppressFinalize(this);
        }
        catch
        {
            //
        }
    }
}