in vec4 vertexPosition;
uniform mat4 view, projection;

void main () {
    gl_Position = projection * view * vertexPosition; 
}
