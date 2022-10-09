out vec4 color0;

void main() {
    const float pi = 3.1415926535897931;
    vec2 pp = pi * gl_PointCoord.xy;
    float intensity = (pow(sin(pp.y),10) + pow(sin(pp.x),10)) * 0.5;
    color0 = vec4(vec3(intensity),1);
}
