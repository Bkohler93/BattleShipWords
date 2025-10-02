shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// normalized coordinates [0,1]
    vec2 uv = UV;
    // center at (0.5,0.5)
    vec2 centered = uv - vec2(0.5);
    float dist = length(centered);

    // keep inside circle
    if (dist > 0.5) {
        COLOR = vec4(0.0); // fully transparent
    } else {
        COLOR = texture(TEXTURE, UV);
    }
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
