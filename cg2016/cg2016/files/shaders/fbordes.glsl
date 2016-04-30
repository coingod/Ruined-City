#version 330

in vec3 fPosition;
in vec3 fNormal;

out vec4 FragColor;

void main()
{
	//Calculo el producto escalar entre Normal y Posicion en espacio de ojo
	//La posicion no esta normalizada. Por lo que la distancia al objeto varia el resultado. (Arreglar?)
	float doty = 1.0 / abs(pow(dot(fPosition, fNormal),1.0));
	//Atenuo la intensidad del color lejos de los bordes
	vec3 color = vec3(1,0,0) * doty;

	//Solucion un toque manija
	//float doty =  1.0/(abs(dot(normalize(fPosition), fNormal)) + 1.0);
	//Atenuo la intensidad del color lejos de los bordes
	//vec3 color = vec3(1,0,0) * doty * doty;
    
	FragColor = vec4(color, 1.0);
}