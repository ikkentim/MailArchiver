using Microsoft.IO;

namespace DecMailBundle;

public static class MemoryStreamManager
{
    private static readonly RecyclableMemoryStreamManager _manager = new();

    public static MemoryStream Get()
    {
        return _manager.GetStream();
    }
}