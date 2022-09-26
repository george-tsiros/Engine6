uniform vec4 color;
out vec4 color0,color1;
void main() {
    color0 = color;
    color1 = vec4(vec3(1)-color.rgb, 1);
}
