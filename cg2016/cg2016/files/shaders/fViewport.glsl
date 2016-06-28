#version 330

layout(location = 0) out vec3 FragColor;

in vec2 fTextureCoordinates;

uniform sampler2D uShadowSampler;

void main()
{
	float depth = texture(uShadowSampler, fTextureCoordinates).r;
	FragColor   = vec3(depth);
}
