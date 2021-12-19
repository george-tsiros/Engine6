#version 460 core
#pragma debug(on)

in vec2 uv;
flat in int inst;
uniform sampler2D tex[4];
uniform int selected;
uniform float angle;
out vec4 fragData[4];
out int fragData0;
void main () { 
    for (int i = 0; i < 4; ++i)
    if (inst == selected)
        fragData[i] = texture(tex[i], uv) * vec4(vec3(0.8 + 0.2 * sin(angle)),1);
    else 
        fragData[i] = texture(tex[i], uv);

    fragData0 = inst;
}
