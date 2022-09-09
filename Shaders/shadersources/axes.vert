in vec4 vertexPosition, color;
out vec4 color0;
uniform mat4 model, view, projection;

void main () {
    gl_Position = projection * view * model * vertexPosition; 
    color0 = color;
}
