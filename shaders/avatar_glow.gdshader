shader_type canvas_item;

uniform float radius : hint_range(0.0, 1.0) = 0.350;
uniform float glow_dispersion: hint_range(0.09,0.15) = 0.12;
uniform vec4 glow_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
    vec2 uv = UV - vec2(0.5); // center
    float dist = length(uv);
    float alpha = smoothstep(radius, radius - glow_dispersion, dist);
    COLOR = vec4(glow_color.rgb, alpha * glow_color.a);
	//COLOR = vec4(1,0,0,1);
}
