uniform sampler2D tex0;
in vec2 uv;
out vec4 color0;
void main() {
    color0 = texture(tex0, uv);
}
