#version 460 core
#pragma debug(on)

in float x,y;
uniform float a, b;
void main () {

/*
min - max
-1, 1

a * max + b = 1
a * min + b = -1

a ( max - min) = 2
a = 2 / ( max - min )

b = 1 - a * max 


*/
    gl_Position = vec4(x, a * y + b, 0, 1); 
}