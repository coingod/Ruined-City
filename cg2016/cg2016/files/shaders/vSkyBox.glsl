#version 330

layout(location = 0) in vec3 vPos;

out vec3 fTextureCoordinates;

uniform mat4 projMat;
uniform mat4 vMat;

void main() 
{
	gl_Position			= projMat * vMat * vec4(vPos, 1.0);
	fTextureCoordinates = vPos;
}