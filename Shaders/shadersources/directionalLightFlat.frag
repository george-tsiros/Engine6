#version 460 core
#pragma debug(on)

in vec4 not_FragPos, not_Normal;
uniform vec4 color, lightDirection;
out vec4 out0;

void main () { 
    vec3 n = normalize(not_Normal.xyz);
    vec3 l = -normalize(lightDirection.xyz);
    
    out0 = vec4(color.rgb * clamp(dot(n, l), 0.0, 1.0), 1.0);
}