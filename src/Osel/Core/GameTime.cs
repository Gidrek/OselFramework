namespace Osel.Core;

public readonly record struct GameTime(TimeSpan TotalGameTime, TimeSpan ElapsedGameTime)
{
    /// <summary>Elapsed time since last frame in seconds.</summary>
    public float DeltaTime => (float)ElapsedGameTime.TotalSeconds;
}
