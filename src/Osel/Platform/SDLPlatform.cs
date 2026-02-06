using Osel.Audio;
using Osel.Core;
using SDL;

namespace Osel.Platform;

internal static unsafe class SDLPlatform
{
    public static void Initialize()
    {
        if (!SDL3.SDL_Init(
            SDL_InitFlags.SDL_INIT_VIDEO
            | SDL_InitFlags.SDL_INIT_EVENTS
            | SDL_InitFlags.SDL_INIT_GAMEPAD
            | SDL_InitFlags.SDL_INIT_AUDIO))
        {
            throw new OselException($"SDL_Init failed: {SDL3.SDL_GetError()}");
        }
    }

    public static void Shutdown()
    {
        SDL3.SDL_Quit();
    }

    /// <summary>
    /// Loads a WAV file via SDL3 and returns a SoundEffect with managed PCM data.
    /// Keeps SDL_LoadWAV usage inside the Platform layer.
    /// </summary>
    internal static SoundEffect LoadWavFile(string filePath)
    {
        SDL_AudioSpec spec;
        byte* audioBuffer;
        uint audioLength;

        if (!SDL3.SDL_LoadWAV(filePath, &spec, &audioBuffer, &audioLength))
            throw new OselException($"SDL_LoadWAV failed: {SDL3.SDL_GetError()}");

        var pcmData = new byte[audioLength];
        fixed (byte* dst = pcmData)
        {
            Buffer.MemoryCopy(audioBuffer, dst, audioLength, audioLength);
        }
        SDL3.SDL_free(audioBuffer);

        return new SoundEffect(pcmData, spec);
    }
}
