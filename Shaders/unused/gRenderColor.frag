#version 460 core
#pragma debug(on)
in vec4 c;
out vec4 fragData;

void main () { 
    fragData = c;
}