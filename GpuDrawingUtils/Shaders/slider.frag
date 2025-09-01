#version 430
out vec4 outputColor;
in vec2 texCoord;

float width = 0.05;
float border = 0.004;
float smoothing = 0.002;

layout(std430, binding = 0) buffer sliderData
{
    vec2 points[4096];
};

// uniform sampler2D texture0;
// uniform float opacity;

vec2 osuNormalize(vec2 input)
{
    vec2 onField = vec2((64. + input.x) / 640., (64. + input.y) / 640.);
    return vec2(onField.x, onField.y);
}

float dline(vec2 p, vec2 a, vec2 b ) {
    vec2 v = a, w = b;
    float l2 = pow(distance(w, v), 2.);
    if(l2 == 0.0) return distance(p, v);
    float t = clamp(dot(p - v, w - v) / l2, 0., 1.);
    vec2 j = v + t * (w - v);
    return distance(p, j);
}

vec4 colorOfDistance(float distance)
{
    float m = 1. / (width - border);
    if (distance > width) {
        float s = (1. - (distance - (width))) * smoothing;
        if (s > 1.) s = 0.;
        return vec4(1., 1., 1., s);
    }
    if (distance > width - border)
        return vec4(1., 1., 1., 1.);
    return vec4(texCoord * 4. - vec2(1.2), distance * m, distance * m);
}

vec4 colorOfPoint(vec2 slider[4096], vec2 self)
{
    float minDistance = 1.;
    for (int i = 1; i < 4096; i++) {
        if (slider[i] == ivec2(0, 0)) continue;
        float dist = dline(self, osuNormalize(slider[i - 1]), osuNormalize(slider[i - 1]));
        if (dist < minDistance) minDistance = dist;
    }
    return colorOfDistance(minDistance);
}

void main()
{
    outputColor = colorOfPoint(points, texCoord);
}