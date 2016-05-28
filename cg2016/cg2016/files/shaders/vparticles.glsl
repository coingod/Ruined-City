// VERTEX SHADER. Simple

#version 330

in vec3 quad;

//out vec2 TexCoords;
out vec4 ParticleColor;

uniform mat4 projMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;
uniform vec3 offset;
uniform vec4 color;
uniform float life;

void main()
{
    float scale = 2.0f;
    //TexCoords = quad.zw;
    ParticleColor = color * clamp(1, 0, life);
    gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(scale * (quad + offset),1.0);
}