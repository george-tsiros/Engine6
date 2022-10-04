uniform mat4 view, projection;

in vec4 vertexPosition;

void main() {
    gl_Position = projection * view * vertexPosition;
}
