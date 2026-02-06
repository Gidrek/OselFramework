using System.Text.Json;
using Osel.Audio;
using Osel.Core;
using Osel.Graphics;
using Osel.Platform;
using StbImageSharp;

namespace Osel.Content;

public class ContentManager : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly Dictionary<string, object> _cache = new();
    private readonly Dictionary<string, string> _filePathToAssetName = new();
    private string _rootDirectory;
    private AssetWatcher? _assetWatcher;

    /// <summary>
    /// Gets or sets the root directory for content files.
    /// Can be an absolute path, or a relative path (resolved from the executable directory).
    /// </summary>
    public string RootDirectory
    {
        get => _rootDirectory;
        set => _rootDirectory = ResolvePath(value);
    }

    public ContentManager(GraphicsDevice graphicsDevice, string rootDirectory = "Content")
    {
        _graphicsDevice = graphicsDevice;
        _rootDirectory = ResolvePath(rootDirectory);
    }

    public T Load<T>(string assetName) where T : class
    {
        if (_cache.TryGetValue(assetName, out var cached))
            return (T)cached;

        object asset = typeof(T).Name switch
        {
            nameof(Texture2D) => LoadTexture(assetName),
            nameof(SoundEffect) => LoadSoundEffect(assetName),
            nameof(Music) => LoadMusic(assetName),
            nameof(SpriteFont) => LoadFont(assetName, 32f),
            nameof(TileMap) => LoadTileMap(assetName),
            _ => throw new OselException($"Unsupported asset type: {typeof(T)}")
        };

        _cache[assetName] = asset;
        return (T)asset;
    }

    /// <summary>
    /// Loads a font at a specific size. Cache key includes font size.
    /// </summary>
    public SpriteFont LoadFont(string assetName, float fontSize = 32f)
    {
        var cacheKey = $"{assetName}@{fontSize}";
        if (_cache.TryGetValue(cacheKey, out var cached))
            return (SpriteFont)cached;

        var filePath = ResolveAssetPath(assetName, ".ttf", ".otf");
        var fontData = File.ReadAllBytes(filePath);
        var font = SpriteFont.FromTtf(_graphicsDevice, fontData, fontSize);

        _cache[cacheKey] = font;
        return font;
    }

    /// <summary>Removes all cached assets and disposes any disposable ones.</summary>
    public void Unload()
    {
        foreach (var asset in _cache.Values)
        {
            if (asset is IDisposable disposable)
                disposable.Dispose();
        }
        _cache.Clear();
    }

    /// <summary>Enables file watching for automatic texture hot-reload during development.</summary>
    public void EnableHotReload()
    {
        if (_assetWatcher != null) return;
        _assetWatcher = new AssetWatcher();
        _assetWatcher.Start(_rootDirectory);
    }

    /// <summary>Disables file watching.</summary>
    public void DisableHotReload()
    {
        _assetWatcher?.Dispose();
        _assetWatcher = null;
    }

    /// <summary>
    /// Processes any pending file changes from the hot-reload watcher.
    /// Should be called once per frame from the game loop.
    /// </summary>
    public void ProcessPendingReloads()
    {
        if (_assetWatcher == null) return;

        var changes = _assetWatcher.GetPendingChanges();
        foreach (var fullPath in changes)
        {
            var normalized = Path.GetFullPath(fullPath);
            if (!_filePathToAssetName.TryGetValue(normalized, out var assetName))
                continue;

            if (!_cache.TryGetValue(assetName, out var asset) || asset is not Texture2D texture)
                continue;

            try
            {
                StbImage.stbi_set_flip_vertically_on_load(0);
                using var stream = File.OpenRead(fullPath);
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                if (texture.ReloadPixelData(image.Data, image.Width, image.Height))
                    Console.WriteLine($"[HotReload] Reloaded texture: {assetName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HotReload] Failed to reload '{assetName}': {ex.Message}");
            }
        }
    }

    private Texture2D LoadTexture(string assetName)
    {
        var filePath = ResolveAssetPath(assetName, ".png", ".jpg", ".bmp");

        // Track file path for hot-reload
        _filePathToAssetName[Path.GetFullPath(filePath)] = assetName;

        // Load image using StbImageSharp
        StbImage.stbi_set_flip_vertically_on_load(0);
        using var stream = File.OpenRead(filePath);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        return new Texture2D(_graphicsDevice, image.Width, image.Height, image.Data);
    }

    private SoundEffect LoadSoundEffect(string assetName)
    {
        var filePath = ResolveAssetPath(assetName, ".wav");
        return SDLPlatform.LoadWavFile(filePath);
    }

    private Music LoadMusic(string assetName)
    {
        var filePath = ResolveAssetPath(assetName, ".ogg");
        return new Music(filePath);
    }

    private TileMap LoadTileMap(string assetName)
    {
        var filePath = ResolveAssetPath(assetName, ".json");
        var json = File.ReadAllText(filePath);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        int tileWidth = root.GetProperty("tileWidth").GetInt32();
        int tileHeight = root.GetProperty("tileHeight").GetInt32();

        string tilesetName = root.GetProperty("tileset").GetString()
            ?? throw new OselException($"TileMap '{assetName}' is missing 'tileset' property.");

        var tileset = Load<Texture2D>(tilesetName);

        var layers = new List<TileLayer>();
        foreach (var layerEl in root.GetProperty("layers").EnumerateArray())
        {
            string name = layerEl.GetProperty("name").GetString() ?? "unnamed";
            int width = layerEl.GetProperty("width").GetInt32();
            int height = layerEl.GetProperty("height").GetInt32();
            bool collision = layerEl.TryGetProperty("collision", out var cProp) && cProp.GetBoolean();

            var tilesArray = layerEl.GetProperty("tiles");
            var tiles = new int[width * height];
            int i = 0;
            foreach (var tile in tilesArray.EnumerateArray())
            {
                if (i < tiles.Length)
                    tiles[i++] = tile.GetInt32();
            }

            layers.Add(new TileLayer(name, width, height, tileWidth, tileHeight, tiles, collision));
        }

        return new TileMap(tileset, tileWidth, tileHeight, layers);
    }

    private string ResolveAssetPath(string assetName, params string[] extensions)
    {
        // Try the exact name first
        var directPath = Path.Combine(_rootDirectory, assetName);
        if (File.Exists(directPath))
            return directPath;

        // Try with extensions
        foreach (var ext in extensions)
        {
            var candidate = Path.Combine(_rootDirectory, assetName + ext);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new FileNotFoundException($"Content asset not found: '{assetName}'. Searched in: {_rootDirectory}");
    }

    private static string ResolvePath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;

        return Path.Combine(AppContext.BaseDirectory, path);
    }

    public void Dispose()
    {
        DisableHotReload();
        Unload();
        GC.SuppressFinalize(this);
    }
}
