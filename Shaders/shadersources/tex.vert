in vec2 vertexPosition;
out vec2 uv;
void main() {
    uv = (vertexPosition + vec2(1)) / 2;
    gl_Position = vec4(vertexPosition, 0, 1);
}
