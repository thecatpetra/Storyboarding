#version 430
out vec4 outputColor;
in vec2 texCoord;

uniform sampler2D texture0;

vec4 change(vec4 c)
{
    return vec4(c.b * 0.7, c.g * 0.3, c.r * 0.2, c.a);
}

void main()
{
    vec4 color = texture(texture0, texCoord);
    outputColor = change(color);
}