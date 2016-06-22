// FRAGMENT SHADER. Simple

#version 330
#define maxLights 5

//in vec3 LightDir;
in vec2 f_TexCoord;
//in vec3 ViewDir;
in mat3 TBN;
in vec3 fPos_CS;

struct Light {
	vec4 position; //Light position in World Space
	vec3 Ia;
	vec3 Id;
	vec3 Is;
	float coneAngle;
	vec3 coneDirection; //Cone direction in World Space
	int enabled;
};

struct Material {
	vec3 Ka;
	vec3 Kd;
	vec3 Ks;
	float Shininess;
};

uniform sampler2D ColorTex;
uniform sampler2D NormalMapTex;
uniform mat4 viewMatrix;
uniform vec3 cameraPosition; //In World Space.
uniform int numLights;
uniform Light allLights[maxLights];
uniform Material material;

uniform float A;
uniform float B;
uniform float C;

out vec4 FragColor;

vec3 phongModel( vec3 norm, vec3 diffR, Light light, vec3 ViewDir) 
{
	float fAtt;
	vec3 LightPos;
	float falloff;

	if(light.position.w == 0)
	{ 
		LightPos = normalize( transpose(inverse(TBN)) * ( (transpose(inverse(viewMatrix)) * -light.position).xyz) );
	}
	else
	{
		//Transformar POSICION de la Luz de CameraSpace a TangentSpace
		LightPos = normalize( TBN * ( (viewMatrix * light.position).xyz - fPos_CS) );

		//Restricciones del cono de luz
		vec3 coneDirection = normalize(TBN * (mat3(viewMatrix) * light.coneDirection) );
		vec3 rayDirection = -LightPos;
		float lightToSurfaceAngle = degrees(acos(dot(rayDirection, coneDirection)));
		if (lightToSurfaceAngle <= light.coneAngle) 
		{ 
			//Atenuacion a la distancia
			float distanceToLight = length(light.position.xyz);
			fAtt = (0.5 / ( A + B * distanceToLight + C * pow(distanceToLight, 2)) );

			//Atenuacion de los bordes 
			float innerCone = light.coneAngle*0.75;
			falloff = smoothstep(light.coneAngle, innerCone, lightToSurfaceAngle);
		} else 
			fAtt = 0.0;
	}

	//Ambiente
	vec3 ambient = material.Ka * diffR * light.Ia;

	//Difuso
	float sDotN = max( dot(LightPos, norm), 0.0 );
	vec3 diffuse = diffR * sDotN * light.Id;// * material.Kd;

	//Especular
	vec3 r = reflect( -LightPos, norm );
	vec3 spec = vec3(0.0);
	if( sDotN > 0.0 )
		spec = material.Ks * pow( max( dot(r,ViewDir), 0.0 ), material.Shininess ) * light.Is;

	return ambient + fAtt * falloff * (diffuse + spec*0.001) * light.enabled;
}

void main() 
{
	// Lookup the normal from the normal map
	vec4 normal = 2.0*texture2D( NormalMapTex, f_TexCoord ) - 1;

	// The color texture is used as the diff. reflectivity
	vec4 texColor = texture2D( ColorTex, f_TexCoord );

	//Transformar Posicion de CameraSpace a TangentSpace
	vec3 ViewDir = TBN * normalize(-fPos_CS);

	//Acumular iluminacion de cada fuente de luz
	vec3 linearColor=vec3(0);
	for(int i=0; i<numLights; i++)
		linearColor += phongModel(normal.xyz, texColor.rgb, allLights[i], ViewDir);

	FragColor = vec4( linearColor, 1.0 );
}