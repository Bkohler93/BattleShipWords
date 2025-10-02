shader_type canvas_item;

//uniform sampler2D tex : hint_albedo;
uniform vec4 glow_color : source_color = vec4(1,1,1,0.5);
uniform float glow_size : hint_range(0, 1) = 0.0;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);

    // calculate distance from alpha edge
    float alpha = tex_color.a;
    float glow_alpha = smoothstep(0.0, glow_size, glow_size - alpha * glow_size);

    vec4 glow = glow_color * glow_alpha;
    COLOR = tex_color + glow;
}
