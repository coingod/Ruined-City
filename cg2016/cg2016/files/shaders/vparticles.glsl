// VERTEX SHADER. Simple
//Referencias
//http://www.geeks3d.com/20140807/billboarding-vertex-shader-glsl/
#version 330

in vec3 vertexPos;
in vec4 particlePos;
in vec4 particleColor;
in vec2 TexCoords;

out vec2 f_TexCoords;
out vec4 ParticleColor;

uniform mat4 projMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

mat4 ClearRotation(mat4 matrix)
{
	// Column 0:
    matrix[0][0] = 1;
    matrix[0][1] = 0;
    matrix[0][2] = 0;
    // Column 1:
    matrix[1][0] = 0;
    matrix[1][1] = 1;
    matrix[1][2] = 0;
    // Column 2:
    matrix[2][0] = 0;
    matrix[2][1] = 0;
    matrix[2][2] = 1;

	return matrix;
}

void main()
{
    float scale = particlePos.w;

    ParticleColor = particleColor;

	mat4 modelViewMatrix = (viewMatrix * modelMatrix);

	//Para la posicion de las particulas tengo en cuenta las rotaciones
	vec4 particlePosition = modelViewMatrix * vec4(particlePos.xyz, 1.0);

	//Elimino las rotaciones de la ModelView
	modelViewMatrix = ClearRotation(modelViewMatrix);

	//Para los vertices de la prticula no tengo en cuenta rotaciones (Alinear con la camara)
	vec4 vertexPosition = modelViewMatrix * vec4(vertexPos * scale, 1.0);
	
	f_TexCoords = vec2(TexCoords.s, 1 - TexCoords.t);

	gl_Position = projMatrix * (vertexPosition + particlePosition);
}