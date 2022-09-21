in vec4 vertexPosition;
uniform mat4 projection;

void main () {
    gl_Position = projection * vertexPosition; 
}
