#version 460 core
#pragma debug(on)

in vec4 vertexPosition;
in vec4 translation;
in vec2 vertexUV;

uniform mat4 view, projection;
flat out int inst;
out vec2 uv;
void main () {
    uv = vertexUV;
    inst = gl_InstanceID + 1;
    gl_Position = projection * view * (vertexPosition + translation); 
}