# Osel Engine

Framework 2D en C# inspirado en XNA/MonoGame/FNA, pero modernizado.
Sin APIs legacy, con shaders GLSL nativos y backend grafico moderno (SDL3 GPU API).

Filosofia: mantenerlo simple como MonoGame — el usuario organiza su codigo.
Extensiones futuras (Screen system estilo libGDX, Tilemap integration, packer)
se haran como paquetes separados, no en el core.

## Platforms

Idealmente me gustaria exportar a Windows, Mac (Intel y ARM) y Linux, pero que este listo
para poder exportar tambien a consolas, aunque se que esta parte no la puedo hacer
todavia porque no tengo los permisos, pero que la arquitectura quede lista, como
Monogame y FNA que pueden exportar a Switch y PS4 y PS5.

Quiero poder programar en Windows, Mac o Linux, por lo cual usaremos .NET 8 en adelante.

## Referencia

FNA esta integrando SDL3, consultar su codigo como referencia para dudas de implementacion
con SDL3 GPU API: https://github.com/FNA-XNA/FNA

## API

La api debera ser parecida a Monogame ya que si me gusta, aunque eliminando lo que no es necesario,
y apis viejas que ya no se usan. La idea es hacer un juego 2D (luego a lo mejor 3D) de Pixeles
pero quiero usar este framework para otros proyuectos.

Quiero que sea facil usar Shaders, o al menos tenga la posibilidad de usarlos con glsl.

Puedes usar la tecnologia que mejor se adapte a los proyectos.

## Codigo y documentacion del codigo

EL codigo y comentarios dentro del codigo deben ser en ingles, pero nosotros podemos
hablar en espanol.

---

## Arquitectura Tecnica

### Stack Tecnologico

| Componente | Tecnologia | Razon |
|---|---|---|
| Runtime | .NET 10 | AOT compilation para consolas, .slnx format |
| Windowing / Input | SDL3 (ppy.SDL3-CS) | Cross-platform, soporte de consolas, bien probado |
| Graphics | SDL3 GPU API | Abstrae Vulkan/Metal/D3D12, moderno, cross-platform |
| Shaders | GLSL → SPIR-V → SDL3 GPU + MSL embebido | GLSL para usuario, MSL built-in para macOS Metal |
| Audio | SDL3 Audio + NVorbis 0.10.5 | Streams SDL3 para WAV, NVorbis (pure C#) para OGG |
| Fonts | StbTrueTypeSharp 1.26.12 | TTF → atlas texture at load time, pure C# |
| Images | StbImageSharp 2.30.15 | PNG/JPG/BMP loading, pure C# |
| Math | Custom (readonly record structs) | Vector2, Vector3, Vector4, Matrix4x4, Rectangle |
| Content | ContentManager | Cache + auto-resolve extensions (.png, .wav, .ogg, .ttf) |

### Bindings SDL3

Usaremos **SDL3-CS** (bindings de la comunidad) — se mantiene activamente y
cubre toda la API de SDL3. Evitamos mantener bindings propios.

### Shaders - Pipeline

```
GLSL (.vert/.frag) -> glslc/glslangValidator -> SPIR-V (.spv) -> SDL3 GPU API
```

SDL3 GPU API acepta SPIR-V y lo cross-compila internamente a:
- SPIR-V (Vulkan)
- MSL (Metal - macOS/iOS)
- DXIL (DirectX 12 - Windows)

El usuario escribe GLSL puro y el framework se encarga de la compilacion.

### Estructura del Proyecto

```
Osel/
├── src/
│   ├── Osel/                              # Proyecto principal del framework (Osel.csproj)
│   │   ├── Core/                          # Game, GameTime, GameWindow, Color, OselException
│   │   ├── Graphics/                      # SpriteBatch, Texture2D, RenderTarget2D, SpriteFont, Effect
│   │   │   └── Shaders/                   # BuiltInShaders (MSL), GLSL sources, compile.sh
│   │   │       └── Compiled/              # SPIR-V bytecode (embedded resources)
│   │   ├── Audio/                         # AudioManager, SoundEffect, Music
│   │   ├── Input/                         # Keyboard, Mouse, Gamepad, InputManager
│   │   ├── Math/                          # Vector2, Vector3, Vector4, Rectangle, Matrix4x4, MathHelper
│   │   ├── Content/                       # ContentManager (Texture2D, SoundEffect, Music, SpriteFont)
│   │   └── Platform/                      # SDLPlatform, SDLWindow, SDLGpuDevice
│   │
│   └── Osel.Samples/
│       └── BasicGame/                     # Juego de ejemplo con todas las features
│
├── CLAUDE.md
└── Osel.slnx
```

Namespace root: `Osel` (e.g. `Osel.Graphics`, `Osel.Input`, `Osel.Audio`)
`Color` vive en `Osel.Core` (no en `Osel.Math`), junto con Game, GameTime, GameWindow.

### API Principal (Estilo MonoGame Modernizado)

```csharp
using Osel.Core;
using Osel.Graphics;
using Osel.Input;
using Osel.Audio;
using Osel.Math;

public class MyGame : Game
{
    private SpriteBatch spriteBatch;
    private Texture2D playerTexture;
    private SpriteFont font;
    private SoundEffect jumpSound;
    private Music bgMusic;

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        playerTexture = Content.Load<Texture2D>("player");
        font = Content.LoadFont("fonts/Arial", 24f);
        jumpSound = Content.Load<SoundEffect>("sounds/jump");
        bgMusic = Content.Load<Music>("music/theme");
        Music.Play(bgMusic, loop: true);
    }

    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.IsKeyDown(Keys.Escape)) Exit();

        // Gamepad support
        if (Gamepad.IsConnected())
        {
            var stick = Gamepad.GetLeftStick();
            // move player with stick...
        }

        if (Keyboard.IsKeyPressed(Keys.Space))
            jumpSound.Play();
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        spriteBatch.Begin();

        // Draw con rotacion, escala, origen, flip
        var origin = new Vector2(playerTexture.Width / 2f, playerTexture.Height / 2f);
        spriteBatch.Draw(playerTexture, new Vector2(100, 100), null, Color.White,
            rotation: 0.5f, origin: origin, scale: 2.0f, effects: SpriteEffects.FlipHorizontally);

        // Texto
        spriteBatch.DrawString(font, "Hello, Osel!", new Vector2(10, 10), Color.White);

        spriteBatch.End();
    }
}
```

### Diferencias vs MonoGame

| MonoGame | Este Engine | Razon |
|---|---|---|
| Effect (.fx) con MGFX compiler | Effect con GLSL puro (compile.sh → SPIR-V) | Moderno, estandar, sin herramientas extra |
| ContentPipeline (.mgcb) | Carga directa (PNG, WAV, OGG, TTF) | Mas simple para empezar |
| OpenGL 2.1 / DirectX | SDL3 GPU (Vulkan/Metal/D3D12) | APIs graficas modernas |
| GameComponent/DrawableGameComponent | No incluido | Patron legacy, el usuario organiza su codigo |
| ContentManager con .xnb | ContentManager con formatos estandar | Sin conversion innecesaria |
| SpriteFont via pipeline (.spritefont) | SpriteFont directo de .ttf (StbTrueType) | Sin herramientas extra, cualquier TTF |
| XACT / SoundEffect | SDL3 Audio streams + NVorbis | Streaming nativo, sin dependencias pesadas |

### Fases de Desarrollo

**Fase 1 - Foundation (MVP)** ✅
- [x] Proyecto .NET 10, SDL3 bindings (ppy.SDL3-CS)
- [x] Ventana con Game loop (Initialize, Update, Draw)
- [x] Input basico (teclado y mouse)
- [x] GraphicsDevice con SDL3 GPU API
- [x] SpriteBatch basico (pull model, storage buffer + vertex_id)
- [x] Carga de texturas PNG (StbImageSharp)
- [x] Tipos matematicos (Vector2, Vector3, Vector4, Rectangle, Matrix4x4, MathHelper)
- [x] Color en Osel.Core (float RGBA, constantes con nombre)
- [x] ContentManager con cache y RootDirectory configurable
- [x] MSL shaders embebidos para macOS Metal

**Fase 2 - Core Features** ✅
- [x] SpriteBatch completo (rotacion, escala, origen, flip) — SpriteData 80 bytes, MSL shader con transform chain
- [x] RenderTarget2D — COLOR_TARGET | SAMPLER, GraphicsDevice.SetRenderTarget()
- [x] Gamepad input — hasta 4 controllers, deadzone, static Gamepad facade
- [x] Audio (SoundEffect, Music) — SDL3 stream model, WAV via SDL_LoadWAV, OGG via NVorbis streaming
- [x] SpriteFont / texto — StbTrueTypeSharp atlas generation, kerning, DrawString, ASCII + Latin extendido
- [x] Sistema de Shaders GLSL (SPIR-V pipeline) — Effect class, compile.sh, GLSL 450 source shaders

**Fase 3 - Game Ready** ✅
- [x] Camera 2D — Matrix4x4 helpers, Camera2D class (follow, zoom, bounds clamping), SpriteBatch.Begin(transformMatrix:)
- [x] Sprite animation system — SpriteAnimation (immutable definition, FromGrid factory), AnimatedSprite (playback state)
- [x] Tilemap support — TileLayer, TileMap, JSON loading via ContentManager, camera-based culling
- [x] Basic collision detection — Collision utility (AABB, separation, MoveAndSlide), Rectangle.Offset
- [x] Asset hot-reload (desarrollo) — AssetWatcher (FileSystemWatcher + debounce), Texture2D.ReloadPixelData, ContentManager.EnableHotReload()

**Fase 4 - Polish & Consoles**
- [x] AOT compilation support — IsAotCompatible, EnableTrimAnalyzer, PublishAot
- [x] Abstraer platform layer para consolas — PlatformBackend coordinator, Game.cs/ContentManager.cs SDL-free
- [x] Profiling / debug tools — DebugOverlay (FPS, frame time, draw calls, sprites, memory), SpriteBatch counters
- [x] Documentacion

