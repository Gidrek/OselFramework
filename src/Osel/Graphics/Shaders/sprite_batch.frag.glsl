#version 450

// SpriteBatch fragment shader

layout(location = 0) in vec2 frag_tex_coord;
layout(location = 1) in vec4 frag_color;

layout(set = 2, binding = 0) uniform texture2D sprite_texture;
layout(set = 2, binding = 0) uniform sampler sprite_sampler;

layout(location = 0) out vec4 out_color;

void main() {
    out_color = frag_color * texture(sampler2D(sprite_texture, sprite_sampler), frag_tex_coord);
}
