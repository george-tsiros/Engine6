in vec4 vertexPosition;
in vec2 texCoords;
uniform mat4 orientation, projection; 
out vec2 uv;
void main() {
    uv = texCoords;
    gl_Position = projection * orientation * vertexPosition;
}
