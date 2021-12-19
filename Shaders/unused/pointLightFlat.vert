#version 460 core
#pragma debug(on)

in vec4 vertex, normal;
uniform mat4 model, view, projection;
out vec4 not_FragPos, not_Normal;

void main () {
    not_Normal = model * normal;
    not_FragPos = model * vertex;
    gl_Position = projection * view * not_FragPos; 
}
