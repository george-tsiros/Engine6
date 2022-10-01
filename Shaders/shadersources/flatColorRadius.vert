in vec4 vertexPosition;
uniform float scale;
uniform mat4 model, view, projection;
void main() {
    gl_Position = projection * view * model * scale * vertexPosition;
}
