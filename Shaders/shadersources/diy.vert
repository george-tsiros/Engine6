in vec4 vertexPosition;
uniform mat4 matrix;
void main() {
    gl_Position = matrix * vertexPosition;
}
