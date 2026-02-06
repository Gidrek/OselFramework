namespace Osel.Audio;

/// <summary>
/// Represents a music track loaded from an OGG file.
/// Uses streaming playback — only the file path is stored, audio is decoded in real-time.
/// </summary>
public class Music : IDisposable
{
    internal string FilePath { get; }

    internal Music(string filePath)
    {
        FilePath = filePath;
    }

    /// <summary>Starts playing this music track.</summary>
    public static void Play(Music music, bool loop = true, float volume = 1.0f)
    {
        AudioManager.PlayMusic(music.FilePath, loop, volume);
    }

    /// <summary>Stops the currently playing music.</summary>
    public static void Stop()
    {
        AudioManager.StopMusic();
    }

    /// <summary>Sets the music playback volume (0.0 to 1.0).</summary>
    public static void SetVolume(float volume)
    {
        AudioManager.SetMusicVolume(volume);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
