// VERTEX SHADER. Simple

#version 330

//uniform vec4 figureColor;
uniform mat4 Proy;
uniform mat4 MV;

in vec3 vPos;
//out vec4 fragColor;

void main(){
 // fragColor = figureColor;
 //fragColor = vec4(vPos.z +0.75, 0.6, 0.0, 1.0);
// fragColor = vec4(0, 1, 0, 1);
 //fragColor = vec4(vCol, 1.0);
  gl_Position = Proy * MV * vec4(vPos, 1.0);
}
