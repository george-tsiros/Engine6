in vec4 vertexPosition;
uniform mat4 model, view, projection;

void main () {
    gl_Position = projection * view * model * vertexPosition; 
}
