out vec4 color0;

void main() {
    const float pi = 3.1415926535897931;
    vec2 pp = pi * gl_PointCoord.xy;
    float brightness = pow((sin(pp.x)+sin(pp.y))/2, 10.0);
    color0 = vec4(vec3(brightness),1);
}
