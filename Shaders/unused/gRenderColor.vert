#version 460 core
#pragma debug(on)

in vec4 vertexPosition;
in vec4 displacement;
in vec4 color;
out vec4 c;
void main () {
    c = color;
    gl_Position = vertexPosition + displacement; 
}