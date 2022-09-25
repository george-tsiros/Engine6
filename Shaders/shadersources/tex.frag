uniform sampler2D tex0;
in vec2 uv;
out vec4 color0;
void main() {
    vec4 v = texture(tex0, uv);
    if (0 == v.a)
        discard;
    color0 = v;
}
