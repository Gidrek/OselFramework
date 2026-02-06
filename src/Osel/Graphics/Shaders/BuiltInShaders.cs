using System.Text;
using Osel.Core;
using SDL;

namespace Osel.Graphics.Shaders;

/// <summary>
/// Provides built-in shader source/bytecode for the engine's rendering pipeline.
/// MSL source is embedded directly. SPIR-V bytecode is loaded from embedded resources.
/// </summary>
internal static class BuiltInShaders
{
    // MSL vertex shader for sprite batching (pull model using vertex_id)
    // SDL3 Metal buffer index mapping for vertex shaders:
    //   Uniform buffers: buffer(0), buffer(1), ... (starting at 0)
    //   Storage buffers: buffer(num_uniforms), buffer(num_uniforms+1), ... (after uniforms)
    //   Vertex buffers:  buffer(14+) (METAL_FIRST_VERTEX_BUFFER_SLOT = 14)
    // Our shader: 1 uniform (VP matrix) at buffer(0), 1 storage (sprites) at buffer(1)
    private const string MslVertexSource =
"#include <metal_stdlib>\n" +
"using namespace metal;\n" +
"\n" +
"struct SpriteData {\n" +
"    float2 position;\n" +
"    float2 size;\n" +
"    float4 source_rect;\n" +
"    float4 color;\n" +
"    float2 origin;\n" +
"    float2 scale;\n" +
"    float rotation;\n" +
"    float _pad0;\n" +
"    float _pad1;\n" +
"    float _pad2;\n" +
"};\n" +
"\n" +
"struct ViewProjection {\n" +
"    float4x4 matrix_transform;\n" +
"};\n" +
"\n" +
"struct VertexOutput {\n" +
"    float4 position [[position]];\n" +
"    float2 tex_coord;\n" +
"    float4 color;\n" +
"};\n" +
"\n" +
"vertex VertexOutput vertexMain(\n" +
"    uint vertex_id [[vertex_id]],\n" +
"    constant ViewProjection& vp [[buffer(0)]],\n" +
"    const device SpriteData* sprites [[buffer(1)]])\n" +
"{\n" +
"    const uint indices[6] = { 0u, 1u, 2u, 3u, 2u, 1u };\n" +
"    const float2 positions[4] = {\n" +
"        float2(0.0f, 0.0f),\n" +
"        float2(1.0f, 0.0f),\n" +
"        float2(0.0f, 1.0f),\n" +
"        float2(1.0f, 1.0f)\n" +
"    };\n" +
"\n" +
"    uint sprite_index = vertex_id / 6u;\n" +
"    uint vert_index = indices[vertex_id % 6u];\n" +
"\n" +
"    SpriteData sprite = sprites[sprite_index];\n" +
"    float2 vert_pos = positions[vert_index];\n" +
"\n" +
"    // Transform chain: local space -> scale -> rotate -> translate\n" +
"    float2 local = (vert_pos * sprite.size) - sprite.origin;\n" +
"    local *= sprite.scale;\n" +
"\n" +
"    float cos_r = cos(sprite.rotation);\n" +
"    float sin_r = sin(sprite.rotation);\n" +
"    float2 rotated = float2(\n" +
"        local.x * cos_r - local.y * sin_r,\n" +
"        local.x * sin_r + local.y * cos_r\n" +
"    );\n" +
"\n" +
"    float2 world_pos = sprite.position + rotated;\n" +
"    float2 uv = sprite.source_rect.xy + vert_pos * sprite.source_rect.zw;\n" +
"\n" +
"    VertexOutput output;\n" +
"    output.position = vp.matrix_transform * float4(world_pos, 0.0f, 1.0f);\n" +
"    output.tex_coord = uv;\n" +
"    output.color = sprite.color;\n" +
"    return output;\n" +
"}\n";

    // MSL fragment shader for sprite batching
    // Binding convention for SDL3 GPU on Metal:
    //   Fragment textures:  texture(0), texture(1), ...
    //   Fragment samplers:  sampler(0), sampler(1), ...
    private const string MslFragmentSource =
"#include <metal_stdlib>\n" +
"using namespace metal;\n" +
"\n" +
"struct FragmentInput {\n" +
"    float4 position [[position]];\n" +
"    float2 tex_coord;\n" +
"    float4 color;\n" +
"};\n" +
"\n" +
"fragment float4 fragmentMain(\n" +
"    FragmentInput in [[stage_in]],\n" +
"    texture2d<float> sprite_texture [[texture(0)]],\n" +
"    sampler sprite_sampler [[sampler(0)]])\n" +
"{\n" +
"    return in.color * sprite_texture.sample(sprite_sampler, in.tex_coord);\n" +
"}\n";

    internal static byte[] GetVertexShaderCode(SDL_GPUShaderFormat format)
    {
        return format switch
        {
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL => Encoding.UTF8.GetBytes(MslVertexSource),
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV => LoadEmbeddedResource("sprite_batch.vert.spv"),
            _ => throw new OselException($"Unsupported shader format: {format}")
        };
    }

    internal static byte[] GetFragmentShaderCode(SDL_GPUShaderFormat format)
    {
        return format switch
        {
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL => Encoding.UTF8.GetBytes(MslFragmentSource),
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV => LoadEmbeddedResource("sprite_batch.frag.spv"),
            _ => throw new OselException($"Unsupported shader format: {format}")
        };
    }

    internal static string GetVertexEntryPoint(SDL_GPUShaderFormat format)
    {
        return format switch
        {
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL => "vertexMain",
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV => "main",
            _ => "main"
        };
    }

    internal static string GetFragmentEntryPoint(SDL_GPUShaderFormat format)
    {
        return format switch
        {
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL => "fragmentMain",
            SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV => "main",
            _ => "main"
        };
    }

    /// <summary>
    /// Detects the best shader format supported by the device.
    /// Preference: MSL (macOS) > SPIRV (Vulkan) > DXIL (Windows).
    /// </summary>
    internal static SDL_GPUShaderFormat DetectFormat(SDL_GPUShaderFormat supported)
    {
        if (supported.HasFlag(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL))
            return SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_MSL;
        if (supported.HasFlag(SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV))
            return SDL_GPUShaderFormat.SDL_GPU_SHADERFORMAT_SPIRV;

        throw new OselException($"No supported shader format found. Device supports: {supported}");
    }

    private static byte[] LoadEmbeddedResource(string name)
    {
        var assembly = typeof(BuiltInShaders).Assembly;
        var resourceName = $"Osel.Graphics.Shaders.Compiled.{name}";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new OselException($"Embedded shader resource not found: {resourceName}. SPIR-V shaders may need to be compiled.");

        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
