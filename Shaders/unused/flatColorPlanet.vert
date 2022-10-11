in vec4 vertexPosition;
uniform float radius;
uniform mat4 model, view, projection;
void main() {
    gl_Position = projection * view * model * vec4(vec3(radius), 1) * vertexPosition;
}
