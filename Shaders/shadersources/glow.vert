in vec4 vertexPosition;
in vec2 vertexUV;
in vec3 vertexNormal;
uniform mat4 model, view, projection;
out vec2 uv;
flat out vec4 n;
void main() {
    uv = vertexUV;
    n = normalize(model * vec4(vertexNormal,0));
    gl_Position = projection * view * model * vertexPosition;
}
