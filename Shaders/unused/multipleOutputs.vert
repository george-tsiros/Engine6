#version 460 core
#pragma debug(on)

in vec4 vertexPosition;
in vec2 vertexUV;

uniform mat4 model, view, projection;
flat out int inst;
out vec2 uv;
void main () {
    uv = vertexUV;
    inst = gl_InstanceID;
    gl_Position = projection * view * model * vertexPosition; 
}