// VERTEX SHADER. Simple

#version 330

//uniform vec4 figureColor;
uniform mat4 projMatrix;
uniform mat4 modelMatrix;
uniform mat4 viewMatrix;

in vec3 vPos;
//out vec4 fragColor;

void main(){
 // fragColor = figureColor;
 //fragColor = vec4(vPos.z +0.75, 0.6, 0.0, 1.0);
// fragColor = vec4(0, 1, 0, 1);
 //fragColor = vec4(vCol, 1.0);
  gl_Position = projMatrix * viewMatrix * modelMatrix * vec4(vPos, 1.0);
}
