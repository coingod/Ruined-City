// FRAGMENT SHADER. Simple

#version 330

in vec3 LightDir;
in vec2 f_TexCoord;
in vec3 ViewDir;
in vec3 fnorm;

uniform sampler2D ColorTex;
uniform sampler2D NormalMapTex;

struct Material {
	vec3 Ka;
	//vec3 Kd;
	vec3 Ks;
	float Shininess;
};

uniform Material material;
out vec4 FragColor;

vec3 phongModel( vec3 norm, vec3 diffR ) 
{
	vec3 r = reflect( -LightDir, norm );
	vec3 ambient = material.Ka;
	float sDotN = max( dot(LightDir, norm), 0.0 );
	vec3 diffuse = diffR * sDotN;
	vec3 spec = vec3(0.0);
	if( sDotN > 0.0 )
	spec = material.Ks * pow( max( dot(r,ViewDir), 0.0 ), material.Shininess );
	return ambient + diffuse + spec;
}

void main() 
{
	// Lookup the normal from the normal map
	vec4 normal = 2.0*texture2D( NormalMapTex, f_TexCoord ) - 1;

	// The color texture is used as the diff. reflectivity
	vec4 texColor = texture2D( ColorTex, f_TexCoord );

	FragColor = vec4( phongModel(normal.xyz, texColor.rgb), 1.0 );
	//FragColor = vec4( phongModel(normal.xyz*0.00001 + fnorm, texColor.rgb), 1.0 );

}