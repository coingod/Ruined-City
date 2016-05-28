#version 330

//in vec2 TexCoords;
in vec4 ParticleColor;
out vec4 fragColor;

//uniform sampler2D sprite;

void main()
{
    fragColor = ParticleColor;//(texture(sprite, TexCoords) * ParticleColor);
}  