in vec2 uv;
out vec4 color0;

void main() {
    color0 = vec4(0, uv.x, uv.y, 1);
}
