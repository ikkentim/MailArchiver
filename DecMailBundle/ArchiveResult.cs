namespace DecMailBundle;

public record ArchiveResult(string Path, ArchiveResultStatus Status, string? ErrorMessage = null);