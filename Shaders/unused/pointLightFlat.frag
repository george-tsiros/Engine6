#version 460 core
#pragma debug(on)

in vec4 not_FragPos, not_Normal;
uniform vec4 color, lightPosition;
out vec4 out0;

void main () { 
    vec3 fragToLight = lightPosition.xyz - not_FragPos.xyz;
    float d = pow(max(1, length(fragToLight)), 0.5);

    vec3 n = normalize(not_Normal).xyz;
    out0 = vec4(color.rgb * clamp(dot(n,normalize(fragToLight))/d, 0, 1), 1);
}
