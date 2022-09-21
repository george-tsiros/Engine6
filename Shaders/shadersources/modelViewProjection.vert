in vec4 vertexPosition;
uniform mat4 model, view, projection;

void main () {
    gl_Position = model * view * projection * vertexPosition; 
}
