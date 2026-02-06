#version 450

// SpriteBatch vertex shader (pull model — no vertex attributes)
// Matches the expanded SpriteData struct (80 bytes per sprite)

// Uniform buffer: view-projection matrix
layout(set = 1, binding = 0) uniform ViewProjection {
    mat4 matrix_transform;
} vp;

// Storage buffer: sprite data array
struct SpriteData {
    vec2 position;
    vec2 size;
    vec4 source_rect;  // xy = UV origin, zw = UV size
    vec4 color;
    vec2 origin;
    vec2 scale;
    float rotation;
    float _pad0;
    float _pad1;
    float _pad2;
};

layout(set = 0, binding = 0) readonly buffer SpriteBuffer {
    SpriteData sprites[];
};

// Outputs to fragment shader
layout(location = 0) out vec2 frag_tex_coord;
layout(location = 1) out vec4 frag_color;

void main() {
    const uint indices[6] = uint[6](0u, 1u, 2u, 3u, 2u, 1u);
    const vec2 positions[4] = vec2[4](
        vec2(0.0, 0.0),
        vec2(1.0, 0.0),
        vec2(0.0, 1.0),
        vec2(1.0, 1.0)
    );

    uint sprite_index = gl_VertexIndex / 6u;
    uint vert_index = indices[gl_VertexIndex % 6u];

    SpriteData sprite = sprites[sprite_index];
    vec2 vert_pos = positions[vert_index];

    // Transform chain: local space -> scale -> rotate -> translate
    vec2 local = (vert_pos * sprite.size) - sprite.origin;
    local *= sprite.scale;

    float cos_r = cos(sprite.rotation);
    float sin_r = sin(sprite.rotation);
    vec2 rotated = vec2(
        local.x * cos_r - local.y * sin_r,
        local.x * sin_r + local.y * cos_r
    );

    vec2 world_pos = sprite.position + rotated;
    vec2 uv = sprite.source_rect.xy + vert_pos * sprite.source_rect.zw;

    gl_Position = vp.matrix_transform * vec4(world_pos, 0.0, 1.0);
    frag_tex_coord = uv;
    frag_color = sprite.color;
}
