in vec4 vertexPosition;
in vec2 vertexUV;
uniform mat4 model, view, projection;
out vec2 uv;
void main() {
    uv = vertexUV;
    gl_Position = projection * view * model * vertexPosition;
}
