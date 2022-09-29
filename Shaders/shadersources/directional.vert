in vec4 vertexPosition;
in vec4 vertexNormal;
uniform mat4 model, view, projection;
out vec4 n;
void main() {
    n = model * vertexNormal;
    gl_Position = projection * view * model * vertexPosition;
}
