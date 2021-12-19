#version 460 core
#pragma debug(on)

uniform mat4 model, view,projection;

in vec4 vertexPosition;
in vec2 vertexUV;
out vec2 uv;
void main () {
    uv = vertexUV;
    gl_Position = projection * view * model * vertexPosition; 
}
