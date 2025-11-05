shader_type canvas_item;

uniform float custom_time = 0.0;
uniform float wave_intensity : hint_range(0.0, 1.0) = 0.1;
uniform float distortion_frequency : hint_range(0.0, 20.0) = 8.0;
uniform float gradient_scale : hint_range(0.1, 4.0) = 1.0;
uniform float gradient_offset : hint_range(-1.0, 1.0) = 0.0;
uniform vec4 color_start : source_color = vec4(1.0, 0.5, 0.0, 1.0);
uniform vec4 color_end : source_color = vec4(1.0, 0.0, 0.5, 1.0);

void fragment() {
    vec2 uv = UV;
    float wave = sin(uv.y * distortion_frequency + custom_time) * wave_intensity;
    uv.x += wave;

    float gradient = clamp(uv.y * gradient_scale + gradient_offset, 0.0, 1.0);
    vec4 gradient_color = mix(color_start, color_end, gradient);

    vec4 tex_color = texture(TEXTURE, uv);
    float alpha = tex_color.a * gradient_color.a;
    COLOR = vec4(gradient_color.rgb * tex_color.rgb, alpha);
}
