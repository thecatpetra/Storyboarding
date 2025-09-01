#version 430
out vec4 outputColor;
in vec2 texCoord;

uniform sampler2D texture0;

void main()
{
    vec4 color = texture(texture0, texCoord);
    outputColor = color;
}