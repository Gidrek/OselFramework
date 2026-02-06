using Osel.Core;
using SDL;

namespace Osel.Audio;

/// <summary>
/// Internal audio system manager. Opens the default playback device and manages audio streams.
/// Uses SDL3 stream-based audio: streams are bound to the device and data is pushed into them.
/// </summary>
internal static unsafe class AudioManager
{
    private const int MaxSoundStreams = 32;

    private static SDL_AudioDeviceID _deviceId;
    private static SDL_AudioSpec _deviceSpec;
    private static bool _initialized;

    // Sound effect stream pool
    private static readonly SDL_AudioStream*[] _soundStreams = new SDL_AudioStream*[MaxSoundStreams];
    private static int _nextSoundStream;

    // Music stream
    private static SDL_AudioStream* _musicStream;
    private static NVorbis.VorbisReader? _musicReader;
    private static float[]? _musicDecodeBuffer;
    private static bool _musicLooping;
    private static string? _musicFilePath;
    private static float _musicVolume = 1.0f;

    // Music decode chunk size (in samples per channel)
    private const int MusicChunkSamples = 4096;

    internal static void Initialize()
    {
        if (_initialized) return;

        // Open default playback device with preferred format
        // SDL_AUDIO_F32 is a static readonly on SDL3 class, not on the enum
        _deviceSpec = new SDL_AudioSpec
        {
            format = SDL3.SDL_AUDIO_F32,
            channels = 2,
            freq = 48000,
        };

        fixed (SDL_AudioSpec* specPtr = &_deviceSpec)
        {
            _deviceId = SDL3.SDL_OpenAudioDevice(SDL3.SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, specPtr);
        }
        if (_deviceId == 0)
            throw new OselException($"SDL_OpenAudioDevice failed: {SDL3.SDL_GetError()}");

        // Create sound effect stream pool
        for (int i = 0; i < MaxSoundStreams; i++)
        {
            fixed (SDL_AudioSpec* specPtr = &_deviceSpec)
            {
                _soundStreams[i] = SDL3.SDL_CreateAudioStream(specPtr, specPtr);
            }
            if (_soundStreams[i] == null)
                throw new OselException($"SDL_CreateAudioStream failed: {SDL3.SDL_GetError()}");

            SDL3.SDL_BindAudioStream(_deviceId, _soundStreams[i]);
        }

        // Create music stream
        fixed (SDL_AudioSpec* specPtr = &_deviceSpec)
        {
            _musicStream = SDL3.SDL_CreateAudioStream(specPtr, specPtr);
        }
        if (_musicStream == null)
            throw new OselException($"SDL_CreateAudioStream (music) failed: {SDL3.SDL_GetError()}");
        SDL3.SDL_BindAudioStream(_deviceId, _musicStream);

        _musicDecodeBuffer = new float[MusicChunkSamples * 2]; // stereo
        _initialized = true;
    }

    /// <summary>
    /// Plays a sound effect by pushing its PCM data into the next available stream.
    /// </summary>
    internal static void PlaySound(byte[] pcmData, SDL_AudioSpec sourceSpec, float volume)
    {
        if (!_initialized) return;

        var stream = _soundStreams[_nextSoundStream];
        _nextSoundStream = (_nextSoundStream + 1) % MaxSoundStreams;

        // Clear any remaining data from previous playback
        SDL3.SDL_ClearAudioStream(stream);

        // Update stream format if source differs from device
        fixed (SDL_AudioSpec* devicePtr = &_deviceSpec)
        {
            SDL3.SDL_SetAudioStreamFormat(stream, &sourceSpec, devicePtr);
        }

        // Set volume
        SDL3.SDL_SetAudioStreamGain(stream, volume);

        // Push PCM data
        fixed (byte* ptr = pcmData)
        {
            SDL3.SDL_PutAudioStreamData(stream, (nint)ptr, pcmData.Length);
        }
    }

    /// <summary>
    /// Starts playing music from a VorbisReader. Called by Music.Play().
    /// </summary>
    internal static void PlayMusic(string filePath, bool loop, float volume)
    {
        if (!_initialized) return;

        StopMusic();

        _musicFilePath = filePath;
        _musicLooping = loop;
        _musicVolume = volume;
        SDL3.SDL_SetAudioStreamGain(_musicStream, _musicVolume);

        _musicReader = new NVorbis.VorbisReader(filePath);

        // Set source spec to match the OGG file
        var srcSpec = new SDL_AudioSpec
        {
            format = SDL3.SDL_AUDIO_F32,
            channels = _musicReader.Channels,
            freq = _musicReader.SampleRate,
        };
        fixed (SDL_AudioSpec* devicePtr = &_deviceSpec)
        {
            SDL3.SDL_SetAudioStreamFormat(_musicStream, &srcSpec, devicePtr);
        }
    }

    /// <summary>
    /// Feeds decoded OGG chunks to the music stream. Called once per frame from the game loop.
    /// </summary>
    internal static void UpdateMusic()
    {
        if (!_initialized || _musicReader == null || _musicDecodeBuffer == null) return;

        // Only feed more data if the stream is getting low
        int queued = SDL3.SDL_GetAudioStreamQueued(_musicStream);
        // Keep at least ~100ms buffered (48000 * 2 channels * 4 bytes * 0.1s ≈ 38400 bytes)
        if (queued > 38400) return;

        int samplesRead = _musicReader.ReadSamples(_musicDecodeBuffer, 0, _musicDecodeBuffer.Length);

        if (samplesRead > 0)
        {
            int byteCount = samplesRead * sizeof(float);
            fixed (float* ptr = _musicDecodeBuffer)
            {
                SDL3.SDL_PutAudioStreamData(_musicStream, (nint)ptr, byteCount);
            }
        }
        else if (_musicLooping && _musicFilePath != null)
        {
            // Restart from beginning
            _musicReader.Dispose();
            _musicReader = new NVorbis.VorbisReader(_musicFilePath);
        }
        else
        {
            // Playback finished
            _musicReader.Dispose();
            _musicReader = null;
        }
    }

    internal static void StopMusic()
    {
        if (_musicReader != null)
        {
            _musicReader.Dispose();
            _musicReader = null;
        }
        if (_musicStream != null)
            SDL3.SDL_ClearAudioStream(_musicStream);
    }

    internal static void SetMusicVolume(float volume)
    {
        _musicVolume = volume;
        if (_initialized && _musicStream != null)
            SDL3.SDL_SetAudioStreamGain(_musicStream, _musicVolume);
    }

    internal static void Shutdown()
    {
        if (!_initialized) return;

        StopMusic();

        if (_musicStream != null)
        {
            SDL3.SDL_DestroyAudioStream(_musicStream);
            _musicStream = null;
        }

        for (int i = 0; i < MaxSoundStreams; i++)
        {
            if (_soundStreams[i] != null)
            {
                SDL3.SDL_DestroyAudioStream(_soundStreams[i]);
                _soundStreams[i] = null;
            }
        }

        if (_deviceId != 0)
        {
            SDL3.SDL_CloseAudioDevice(_deviceId);
            _deviceId = 0;
        }

        _initialized = false;
    }
}
