//precision highp float;
in vec4 vertexPosition;
in vec3 vertexNormal;
uniform mat4 model, view, projection;
out vec4 n;
void main() {
    n = model * vec4(vertexNormal,0);
    gl_Position = projection * view * model * vertexPosition;
}
