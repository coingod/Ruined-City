#version 330

in vec2 f_TexCoords;
in vec4 ParticleColor;

uniform sampler2D ColorTex;
uniform vec2 uvOffset;
uniform float time;
uniform int animated;

out vec4 fragColor;

void main()
{
	float u = 0;
	float v = 0;
	if(animated > 0)
	{
		float t = mod(floor(time * 8), 4);
		if(t == 0 || t == 2)
			u = 0;
		else
			u = 0.5;

		if(t == 0 || t == 1)
			v = 0.5;
		else
			v = 0;
	}
	//vec4 texture = texture2D(ColorTex, vec2(f_TexCoords.s * uvOffset.x * t, f_TexCoords.t * uvOffset.y * t));
	vec4 texture = texture2D(ColorTex, vec2(f_TexCoords.s * uvOffset.x + u, f_TexCoords.t * uvOffset.y + v));
	fragColor = texture * ParticleColor;
}  