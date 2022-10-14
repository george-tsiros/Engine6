//precision highp float;
in vec4 vertexPosition;
in vec3 vertexNormal;
in mat4 model;
uniform mat4 view, projection;
out vec4 n;
in vec4 color;
flat out vec4 c;
void main() {
    c = color;
    n = model * vec4(vertexNormal,0);
    gl_Position = projection * view * model * vertexPosition;
}
