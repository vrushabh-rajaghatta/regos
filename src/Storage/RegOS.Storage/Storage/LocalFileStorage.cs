namespace RegOS.Storage;

/// <summary>
/// Stores files under a configured root directory. Relative paths (the values
/// persisted on DocumentVersion.StoragePath) are combined with the root at
/// runtime, so the database stays portable across environments.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(string rootPath)
    {
        _root = Path.GetFullPath(rootPath);
    }

    public async Task SaveAsync(
        string relativePath,
        Stream content,
        CancellationToken cancellationToken)
    {
        var fullPath = ResolveWithinRoot(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<Stream> OpenReadAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        var fullPath = ResolveWithinRoot(relativePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Stored file not found.", relativePath);

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(
        string relativePath,
        CancellationToken cancellationToken)
    {
        var fullPath = ResolveWithinRoot(relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveWithinRoot(string relativePath)
    {
        var normalized = relativePath.Replace('\\', '/').TrimStart('/');
        var fullPath = Path.GetFullPath(Path.Combine(_root, normalized));

        // Guard against path traversal escaping the storage root.
        if (!fullPath.StartsWith(_root, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid storage path.");

        return fullPath;
    }
}
