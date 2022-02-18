#pragma debug(on)

in vec4 vertexPosition;

out vec2 not_FragPos;

void main () {
    not_FragPos = 0.5f * vertexPosition.xy + 0.5;
    not_FragPos = vec2(not_FragPos.x, 1.0 - not_FragPos.y);
    gl_Position = vec4(vertexPosition.xy, 0.0, 1.0); 
}
