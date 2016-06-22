#version 330

out vec4 FragColor;

in vec3 fTextureCoordinates;

uniform samplerCube uSamplerSkybox;

void main()
{
	FragColor = texture(uSamplerSkybox, fTextureCoordinates);
}