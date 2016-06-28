using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrrKlang;
using CGUNS.Meshes;
using CGUNS.Shaders;
using OpenTK;
using CGUNS.Primitives;
using CGUNS.Particles;

namespace CGUNS
{

    class Aviones
    {
        private static float pi = 3.14f;
        private static Matrix4 escala = Matrix4.CreateScale(1f) * Matrix4.CreateRotationY(pi / 2);

        private ObjetoGrafico[] objetos;
        private ISound[] sonidoAviones; //uno para cada uno por si se quiere usar distintos o en momentos diferentes
        private static Vector3 origen = new Vector3(-75.0f, 2.0f, 0.0f); //de donde sale el avion y fin hasta donde llega
        private Vector3 fin = new Vector3(75.0f, 2.0f, 0.0f);
        private Vector3 posicion = origen;
        private Vector3 desplazamiento1 = new Vector3(-1.0f, 0.0f, 2.0f) ; //utilizados para los otros dos aviones
        private Vector3 desplazamiento2 = new Vector3(-1.0f, 0.0f, -2.0f);

        //las variables que siguen son para simular un avion persiguiendo a otro
        private Vector3 centroPerseguido = new Vector3(0.0f, 140.0f, 0.0f)/10;
        private static float radio=12f;
        private Vector3 posPerseguido, posPerseguidor;
        private static float retraso = pi/ 6; //Se utiliza para posicionar a un avion tanto mas atras que otro
                
        private Boolean disparar = false;        
        List<Vector3> posicionesDisparos = new List<Vector3>();
        private float rotX = 0f; //usado en el avion perseguido para girar sobre si mismo
        private Boolean rotar = false;
        private static int MAXDisparos = 20; //maxima cantidad de disparon que se van a dibujar

        // private static double inclinacion = 0.5f; //usado para que el circulo quede inclinado
        List<Smoke> list = new List<Smoke>();
        List<double> tiempoInicios = new List<double>(); //Se guarda en que momento comienzan los disparos en cada posicion

        Cube cubo = new Cube(0.04f, 0.04f, 0.04f);


        public Aviones(ShaderProgram sProgram1, ShaderProgram sProgram2, ISoundEngine engine, ShaderProgram sProgramUnlit, int cantAviones = 5)
        {
            int cantObj = 3;            
            objetos = new ObjetoGrafico[cantObj];
            sonidoAviones = new ISound[cantAviones];


            objetos[0] = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/b17.obj"); //Se utiliza para los primeros 3 aviones. Los que van en linea recta
            objetos[1] = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/b17.obj"); //Es el de adelante de los que van en circulo
            objetos[2] = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/b17.obj"); //Es el de atras de los que van en circulo

            for (int i = 0; i < cantObj; i++)            
                objetos[i].Build(sProgram1, sProgram2);
            

            //Se calcula y setea la posicion inicial de todos los aviones
            posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(pi), radio * (float)Math.Sin(pi), 0);
            posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(pi - retraso), radio * (float)Math.Sin(pi - retraso), 0);


            //Se coloca sonido a cada uno de los aviones. 0-3 son los que se mueven en linea recta. 3ro es perseguido
            Vector3D origen3D = new Vector3D(origen.X, origen.Y, origen.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            String pathSonido = "files/audio/bell.wav";
            sonidoAviones[0] = engine.Play3D(pathSonido, origen3D.X, origen3D.Y, origen3D.Z, true);
            sonidoAviones[1] = engine.Play3D(pathSonido, desp1.X, desp1.Y, desp1.Z, true);
            sonidoAviones[2] = engine.Play3D(pathSonido, desp2.X, desp2.Y, desp2.Z, true);
            sonidoAviones[3] = engine.Play3D(pathSonido, posPerseguido.X, posPerseguido.Y, posPerseguido.Z, true);
            sonidoAviones[4] = engine.Play3D(pathSonido, posPerseguidor.X, posPerseguidor.Y, posPerseguidor.Z, true);

            for (int i=0; i<sonidoAviones.Length; i++)
                sonidoAviones[i].Volume = sonidoAviones[i].Volume / 2;

            cubo.Build(sProgramUnlit); 

        }

        public void Dibujar(ShaderProgram sProgram, ShaderProgram sProgramParticles, Double timeSinceStartup )
        {
            float angulo = (float)timeSinceStartup/2;
            float aumento = 0.3f; //usado para que en determinado momento el de atras aumente la rotacion y apunte al otro avion
            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion);
            objetos[0].Dibujar(sProgram);

            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento1);
            objetos[0].Dibujar(sProgram);

            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento2);
            objetos[0].Dibujar(sProgram);

            //El 3ro es el perseguido, el 4to el que lo sigue
            objetos[1].transform.localToWorld = escala * Matrix4.CreateRotationX(rotX) * Matrix4.CreateRotationZ(-pi / 2+ angulo ) * Matrix4.CreateTranslation(posPerseguido);
            objetos[2].transform.localToWorld = escala * Matrix4.CreateRotationZ(-pi / 2 + angulo-retraso+ aumento) * Matrix4.CreateTranslation(posPerseguidor);

            for (int i = 1; i < 3; i++)
            {
                objetos[i].Dibujar(sProgram);
            }


        }




        public void DibujarCuadradosDisparos(ShaderProgram sProgramUnlit)
        {
            for (int i = 0; i < posicionesDisparos.Count; i++)
            {
                sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.CreateTranslation(posicionesDisparos[i]));
                sProgramUnlit.SetUniformValue("figureColor", new Vector4(1f, 0, 0, 1));
                cubo.Dibujar(sProgramUnlit);
            }
        }

        public void DibujarDisparos(ShaderProgram sProgramParticles)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].enabled)
                    list[i].Dibujar(sProgramParticles);

            }

        }

        public void Actualizar(Double timeSinceStartup, ShaderProgram sProgramParticles ) {

            //Se calcula la nueva posicion del 1ro que va en linea recta. En Dibujar se actualiza la posicion de los 3 aviones
            float blend = (float)timeSinceStartup/12 % 1;
            posicion = Vector3.Lerp(origen, fin, blend);

                        
            //Actualizacion de la persecucion
            double angulo = pi + timeSinceStartup/2;

            posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(angulo), radio * (float)Math.Sin(angulo), 0);
            posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(angulo - retraso), radio * (float)Math.Sin(angulo - retraso), 0);

            if (angulo > 9 && angulo < 9.2f)
            { //es despues de q haya dado una vuelta y media respecto al 0
                disparar = true;
                rotar = true;
            }
            else if (angulo > 12)
                disparar = false;

            //Solo dispara y rola cuando esta en la region del principio del semicirculo de abajo
            if (disparar)
                if (angulo % (pi*2) > 4.0f && angulo % (pi*2) < 4.6f && posicionesDisparos.Count< MAXDisparos)
                {   
                    Disparar(sProgramParticles, timeSinceStartup);                    
                }

            if (rotar)
                {
                 if (angulo % (pi*2) > 3.8f)
                     rotX += 0.1f;    //usada para rotar el avion de adelante
                 if (rotX > (pi*2))
                     {rotX = 0;      //vuelve a la posicion original despues de dar 2 vueltas
                      rotar = false;
                     }                
                }

            //Se actualizan los disparos si es necesario            
            for (int i = 0; i < list.Count; i++)
                {
                if (timeSinceStartup - tiempoInicios[i] > 10)
                    list[i].enabled = false;
                else list[i].Update();
                }

            //Actualizacion de la posicion de los sonidos
            Vector3D origen3D = new Vector3D(posicion.X, posicion.Y, posicion.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            sonidoAviones[0].Position = origen3D;
            sonidoAviones[1].Position = desp1;
            sonidoAviones[2].Position = desp2;
            sonidoAviones[3].Position = new Vector3D(posPerseguido.X, posPerseguido.Y, posPerseguido.Z);
            sonidoAviones[4].Position = new Vector3D(posPerseguidor.X, posPerseguidor.Y, posPerseguidor.Z);
            
        }

        private void Disparar(ShaderProgram sProgramParticles, double timeSinceStartup)
        {   //origen es perseguidor. Dir es perseguido
            Vector3 pendiente = posPerseguido - posPerseguidor;
            //recta se expresa como origen+ t*pendiente. Calculo para que valor de t va a dar y=0 (q punto esta dentro del plano)
            // 0 = rayOrigin.Y + t * pendiente.Y            

            float t = -posPerseguidor.Y / pendiente.Y;
            Vector3 posDisparo = posPerseguidor + t * pendiente; 
            //posDisparo.Y += 0.1f;

            if (posicionesDisparos.Count > 0)
                {//Se agrega un control para que haya cierta separacion entre disparos y no sean infinitos
                if (Math.Abs(posicionesDisparos[(posicionesDisparos.Count - 1)].X - posDisparo.X) > 0.04f)
                    posicionesDisparos.Add(posDisparo);
                }
            else posicionesDisparos.Add(posDisparo);
            
            
             Smoke smokeParticles = new Smoke(posDisparo,2);
             smokeParticles.Build(sProgramParticles);
             list.Add(smokeParticles);
             tiempoInicios.Add(timeSinceStartup);

        }
    }
}
