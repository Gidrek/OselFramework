using System.Collections.Concurrent;

namespace Osel.Content;

/// <summary>
/// Watches the content directory for file changes and queues reload requests.
/// Thread-safe: file system events are queued and processed on the main thread.
/// </summary>
internal class AssetWatcher : IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly ConcurrentQueue<string> _changedFiles = new();
    private readonly Dictionary<string, DateTime> _debounce = new();
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromMilliseconds(50);

    public void Start(string rootDirectory)
    {
        if (_watcher != null) return;

        _watcher = new FileSystemWatcher(rootDirectory)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;

        // Watch texture files
        _watcher.Filter = "*.*";
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher?.Dispose();
        _watcher = null;
    }

    /// <summary>
    /// Dequeues all pending file change paths. Call from the main thread.
    /// </summary>
    public List<string> GetPendingChanges()
    {
        var changes = new List<string>();
        while (_changedFiles.TryDequeue(out var path))
        {
            changes.Add(path);
        }
        return changes;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var ext = Path.GetExtension(e.FullPath).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".bmp")
            return;

        // Debounce: ignore duplicate events within the interval
        var now = DateTime.UtcNow;
        lock (_debounce)
        {
            if (_debounce.TryGetValue(e.FullPath, out var last) && (now - last) < DebounceInterval)
                return;
            _debounce[e.FullPath] = now;
        }

        _changedFiles.Enqueue(e.FullPath);
    }

    public void Dispose()
    {
        Stop();
    }
}
