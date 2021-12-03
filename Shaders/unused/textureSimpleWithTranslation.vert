#version 460 core
#pragma debug(on)

in vec4 vertexPosition;
in vec2 vertexUV;
in vec4 translation;

uniform mat4 view, projection;
out vec2 uv;

void main () {
    uv = vertexUV;
    gl_Position = projection * view * (vertexPosition + translation); 
}