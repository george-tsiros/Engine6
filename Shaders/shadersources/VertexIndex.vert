#pragma debug(on)

in vec4 vertexPosition;
uniform mat4 model, view, projection;
flat out int vertexId;
void main () {
    gl_Position = projection * view * model * vertexPosition; 
    vertexId = gl_VertexID + 1;
}
