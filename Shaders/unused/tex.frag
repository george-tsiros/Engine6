uniform sampler2D tex0;
in vec2 uv;
out vec4 color0, color1;
void main() {
    vec4 v = texture(tex0, uv);
    if (0 == v.a)
        discard;
    color1 = vec4(v.bgr, 1);
    color0 = v;
}
