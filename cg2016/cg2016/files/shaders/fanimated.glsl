// FRAGMENT SHADER.
#version 330

uniform sampler2D ColorTex; 
in vec2 f_TexCoord;
out vec4 fColor;

void main(){
	vec4 colorTex = texture2D(ColorTex, f_TexCoord);

	fColor = colorTex;
}
