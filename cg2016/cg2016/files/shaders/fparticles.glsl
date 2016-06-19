#version 330

in vec2 f_TexCoords;
in vec4 ParticleColor;

uniform sampler2D ColorTex;

out vec4 fragColor;

void main()
{
	vec4 texture = texture2D(ColorTex, f_TexCoords);
	fragColor = texture * ParticleColor;
}  