#version 330

layout(location = 0) in vec2 vPos;

out vec2 fTextureCoordinates;

uniform mat4 uViewportOrthographic;
uniform vec2 uViewportSize;

void main() 
{
	gl_Position			= uViewportOrthographic * vec4(vPos * uViewportSize, 0.0f, 1.0f);
	fTextureCoordinates = vPos;
}