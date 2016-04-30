#version 330

in vec3 colorV;

out vec4 FragColor;

void main()
{
	FragColor = vec4(colorV, 1.0);
}