using Osel.Math;
using StbTrueTypeSharp;

namespace Osel.Graphics;

/// <summary>
/// Glyph metrics for a single character in a SpriteFont atlas.
/// </summary>
public readonly record struct GlyphData(Rectangle SourceRect, Vector2 Offset, float Advance);

/// <summary>
/// A bitmap font rendered from a TrueType file into a texture atlas.
/// Supports ASCII 32-126 and extended Latin 192-255 (accented characters).
/// </summary>
public class SpriteFont : IDisposable
{
    public Texture2D Atlas { get; }
    public Dictionary<char, GlyphData> Glyphs { get; }
    public float LineHeight { get; }

    private readonly Dictionary<(char, char), float> _kerning;

    private SpriteFont(Texture2D atlas, Dictionary<char, GlyphData> glyphs,
        float lineHeight, Dictionary<(char, char), float> kerning)
    {
        Atlas = atlas;
        Glyphs = glyphs;
        LineHeight = lineHeight;
        _kerning = kerning;
    }

    /// <summary>
    /// Returns the kerning adjustment between two characters (in pixels).
    /// </summary>
    public float GetKerning(char left, char right)
    {
        return _kerning.TryGetValue((left, right), out var kern) ? kern : 0f;
    }

    /// <summary>
    /// Measures the size of a string when rendered with this font.
    /// </summary>
    public Vector2 MeasureString(string text)
    {
        if (string.IsNullOrEmpty(text)) return Vector2.Zero;

        float maxWidth = 0;
        float cursorX = 0;
        int lineCount = 1;
        char prevChar = '\0';

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '\n')
            {
                maxWidth = MathF.Max(maxWidth, cursorX);
                cursorX = 0;
                lineCount++;
                prevChar = '\0';
                continue;
            }
            if (c == '\r') { prevChar = '\0'; continue; }

            if (!Glyphs.TryGetValue(c, out var glyph)) continue;

            if (prevChar != '\0')
                cursorX += GetKerning(prevChar, c);

            cursorX += glyph.Advance;
            prevChar = c;
        }

        maxWidth = MathF.Max(maxWidth, cursorX);
        return new Vector2(maxWidth, lineCount * LineHeight);
    }

    /// <summary>
    /// Creates a SpriteFont from TrueType font data at the specified pixel size.
    /// </summary>
    internal static unsafe SpriteFont FromTtf(GraphicsDevice graphicsDevice, byte[] fontData, float fontSize)
    {
        var info = new StbTrueType.stbtt_fontinfo();
        fixed (byte* fontPtr = fontData)
        {
            if (StbTrueType.stbtt_InitFont(info, fontPtr, 0) == 0)
                throw new Core.OselException("Failed to initialize font from TTF data.");

            float scale = StbTrueType.stbtt_ScaleForPixelHeight(info, fontSize);

            // Get vertical metrics
            int ascent, descent, lineGap;
            StbTrueType.stbtt_GetFontVMetrics(info, &ascent, &descent, &lineGap);
            float lineHeight = (ascent - descent + lineGap) * scale;

            // Character ranges: ASCII 32-126 + Extended Latin 192-255
            var charRanges = new List<(int start, int end)>
            {
                (32, 126),
                (192, 255),
            };

            // First pass: measure all glyphs to determine atlas size
            var glyphInfos = new List<(char c, int w, int h, int xoff, int yoff, int advance, int lsb)>();
            int totalArea = 0;

            foreach (var (start, end) in charRanges)
            {
                for (int cp = start; cp <= end; cp++)
                {
                    int glyphIndex = StbTrueType.stbtt_FindGlyphIndex(info, cp);
                    if (glyphIndex == 0 && cp != ' ') continue;

                    int advance, lsb;
                    StbTrueType.stbtt_GetGlyphHMetrics(info, glyphIndex, &advance, &lsb);

                    int x0, y0, x1, y1;
                    StbTrueType.stbtt_GetGlyphBitmapBox(info, glyphIndex, scale, scale, &x0, &y0, &x1, &y1);

                    int w = x1 - x0;
                    int h = y1 - y0;
                    if (w <= 0 || h <= 0)
                    {
                        // Whitespace character — still store metrics
                        glyphInfos.Add(((char)cp, 0, 0, 0, 0, advance, lsb));
                        continue;
                    }

                    totalArea += (w + 2) * (h + 2); // 1px padding on each side
                    glyphInfos.Add(((char)cp, w, h, x0, y0, advance, lsb));
                }
            }

            // Calculate atlas dimensions (power of 2, row-based packing)
            int atlasSize = 256;
            while (atlasSize * atlasSize < totalArea * 2) // heuristic: 2x area for padding
                atlasSize *= 2;
            if (atlasSize > 4096) atlasSize = 4096;

            // Pack glyphs into atlas (simple row-based packer)
            var rgba = new byte[atlasSize * atlasSize * 4];
            var glyphs = new Dictionary<char, GlyphData>();
            int cursorX = 1, cursorY = 1, rowHeight = 0;

            foreach (var (c, w, h, xoff, yoff, advance, lsb) in glyphInfos)
            {
                if (w <= 0 || h <= 0)
                {
                    // Whitespace — no bitmap, just metrics
                    glyphs[c] = new GlyphData(
                        Rectangle.Empty,
                        Vector2.Zero,
                        advance * scale);
                    continue;
                }

                // Check if glyph fits in current row
                if (cursorX + w + 1 > atlasSize)
                {
                    cursorX = 1;
                    cursorY += rowHeight + 1;
                    rowHeight = 0;
                }

                // Render glyph bitmap
                int glyphIndex = StbTrueType.stbtt_FindGlyphIndex(info, c);
                var bitmap = new byte[w * h];
                fixed (byte* bmpPtr = bitmap)
                {
                    StbTrueType.stbtt_MakeGlyphBitmap(info, bmpPtr, w, h, w, scale, scale, glyphIndex);
                }

                // Copy to atlas as RGBA (white + alpha)
                for (int row = 0; row < h; row++)
                {
                    for (int col = 0; col < w; col++)
                    {
                        int atlasIdx = ((cursorY + row) * atlasSize + (cursorX + col)) * 4;
                        byte alpha = bitmap[row * w + col];
                        rgba[atlasIdx + 0] = 255; // R
                        rgba[atlasIdx + 1] = 255; // G
                        rgba[atlasIdx + 2] = 255; // B
                        rgba[atlasIdx + 3] = alpha; // A
                    }
                }

                var srcRect = new Rectangle(cursorX, cursorY, w, h);
                var offset = new Vector2(xoff, yoff + ascent * scale);

                glyphs[c] = new GlyphData(srcRect, offset, advance * scale);

                cursorX += w + 1;
                rowHeight = System.Math.Max(rowHeight, h);
            }

            // Build kerning table
            var kerning = new Dictionary<(char, char), float>();
            var chars = glyphs.Keys.ToArray();
            for (int i = 0; i < chars.Length; i++)
            {
                for (int j = 0; j < chars.Length; j++)
                {
                    int kern = StbTrueType.stbtt_GetCodepointKernAdvance(info, chars[i], chars[j]);
                    if (kern != 0)
                        kerning[(chars[i], chars[j])] = kern * scale;
                }
            }

            // Create texture from atlas
            var atlas = new Texture2D(graphicsDevice, atlasSize, atlasSize, rgba);

            return new SpriteFont(atlas, glyphs, lineHeight, kerning);
        }
    }

    public void Dispose()
    {
        Atlas?.Dispose();
        GC.SuppressFinalize(this);
    }
}
