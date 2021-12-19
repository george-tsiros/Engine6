#version 460 core
#pragma debug(on)

in vec2 coords;

void main () {
    gl_Position = vec4(coords, 0, 1); 
}
