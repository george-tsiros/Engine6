#version 460 core
#pragma debug(on)

in vec2 uv;

uniform sampler2D tex;

out vec4 fragData;

void main () { 
    fragData = texture(tex, uv);
}
