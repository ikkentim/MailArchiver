namespace DecMailBundle;

public static class AppDataPath
{
    public static string WebViewData => GetDir("WebViewData");
    public static string Temp => GetDir("Temp");

    public static TempFile GetTempFile(string? extension = null)
    {
        var name = $"{Guid.NewGuid()}{extension ?? string.Empty}";
        var path = Path.Combine(Temp, name);

        return new TempFile(path);
    }

    private static string GetRoot()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DecMailBundle");
    }

    private static string GetDir(params ReadOnlySpan<string> folders)
    {
        var path = Path.Combine([GetRoot(), ..folders]);
        Directory.CreateDirectory(path);
        return path;
    }
}