in vec4 vertexPosition;

out vec2 not_FragPos;

void main () {
    not_FragPos = 0.5 * vertexPosition.xy + 0.5;
    gl_Position = vec4(vertexPosition.xy, 0.0, 1.0); 
}
