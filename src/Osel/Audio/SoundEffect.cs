using SDL;

namespace Osel.Audio;

/// <summary>
/// A loaded sound effect containing raw PCM audio data (from WAV files).
/// </summary>
public unsafe class SoundEffect : IDisposable
{
    internal byte[] PcmData { get; }
    internal SDL_AudioSpec Spec { get; }

    internal SoundEffect(byte[] pcmData, SDL_AudioSpec spec)
    {
        PcmData = pcmData;
        Spec = spec;
    }

    /// <summary>
    /// Plays this sound effect at the specified volume (0.0 to 1.0).
    /// </summary>
    public void Play(float volume = 1.0f)
    {
        AudioManager.PlaySound(PcmData, Spec, volume);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
