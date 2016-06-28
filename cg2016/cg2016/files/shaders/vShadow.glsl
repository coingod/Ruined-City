#version 330

layout(location = 0) in vec3 vPos;

uniform mat4 uLightSpaceMatrix;
uniform mat4 uModelMatrix;

void main() 
{
	gl_Position = uLightSpaceMatrix * uModelMatrix * vec4(vPos, 1.0f);
}