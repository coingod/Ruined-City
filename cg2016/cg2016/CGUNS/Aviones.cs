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
        private static float aux=10;
        private static Matrix4 escala = Matrix4.CreateScale(1f) * Matrix4.CreateRotationY(pi / 2);

        private ObjetoGrafico[] objetos;
        private ISound[] sonidoAviones; //uno para cada uno por si se quiere usar distintos o en momentos diferentes
        private static Vector3 origen = new Vector3(-75.0f, 2.0f, 0.0f)* aux; //de donde sale el avion y fin hasta donde llega
        private Vector3 fin = new Vector3(75.0f, 1.0f, 0.0f) * aux;
        private Vector3 posicion = origen;
        private Vector3 desplazamiento1 = new Vector3(-1.0f, 0.0f, 2.0f) * aux; //utilizados para los otros dos aviones
        private Vector3 desplazamiento2 = new Vector3(-1.0f, 0.0f, -2.0f) * aux;

        //las variables que siguen son para simular un avion persiguiendo a otro
        private Vector3 centroPerseguido = new Vector3(0.0f, 140.0f, 0.0f);
        private static float radio=120f;
        private Vector3 posPerseguido, posPerseguidor;
        private static float retraso = pi/ 6; //Se utiliza para posicionar a un avion tanto mas atras que otro
                
        private Boolean disparar = false;        
        List<Vector3> posicionesDisparos = new List<Vector3>();
        private float rotX = 0f; //usado en el avion perseguido para girar sobre si mismo
        private Boolean rotar = false;
        private static int MAXDisparos = 20; //maxima cantidad de disparon que se van a dibujar

        // private static double inclinacion = 0.5f; //usado para que el circulo quede inclinado
        //List<Smoke> list = new List<Smoke>();

        Cube cubo = new Cube(0.4f, 0.4f, 0.4f);


        public Aviones(ShaderProgram sProgram, int cantAviones, ISoundEngine engine, ShaderProgram sProgramUnlit)
        {
            objetos = new ObjetoGrafico[cantAviones];
            sonidoAviones = new ISound[cantAviones];
            
                        
            for (int i = 0; i < cantAviones; i++)
                {
                objetos[i] = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/b17.obj");
                objetos[i].Build(sProgram);                     
                }

            //Se calcula y setea la posicion inicial de todos los aviones
            posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(pi), radio * (float)Math.Sin(pi), 0);
            posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(pi - retraso), radio * (float)Math.Sin(pi - retraso), 0);

            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(origen);
            objetos[1].transform.localToWorld = escala * Matrix4.CreateTranslation(origen+ desplazamiento1);
            objetos[2].transform.localToWorld = escala * Matrix4.CreateTranslation(origen+ desplazamiento2);
            objetos[3].transform.localToWorld = escala * Matrix4.CreateRotationZ(pi/2)* Matrix4.CreateTranslation(posPerseguido);
            objetos[4].transform.localToWorld = escala * Matrix4.CreateRotationZ(pi /2 - retraso) * Matrix4.CreateTranslation(posPerseguidor);

            //Se coloca sonido a cada uno de los aviones. 0-3 son los que se mueven en linea recta. 3ro es perseguido
            Vector3D origen3D = new Vector3D(origen.X, origen.Y, origen.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            sonidoAviones[0] = engine.Play3D("files/audio/bell.wav", origen3D.X, origen3D.Y, origen3D.Z, true);
            sonidoAviones[1] = engine.Play3D("files/audio/bell.wav", desp1.X, desp1.Y, desp1.Z, true);
            sonidoAviones[2] = engine.Play3D("files/audio/bell.wav", desp2.X, desp2.Y, desp2.Z, true);
            sonidoAviones[3] = engine.Play3D("files/audio/bell.wav", posPerseguido.X, posPerseguido.Y, posPerseguido.Z, true);
            sonidoAviones[4] = engine.Play3D("files/audio/bell.wav", posPerseguidor.X, posPerseguidor.Y, posPerseguidor.Z, true);

            for (int i=0; i<sonidoAviones.Length; i++)
                sonidoAviones[i].Volume = sonidoAviones[i].Volume / 2;

            cubo.Build(sProgramUnlit); 

        }

        public void Dibujar(ShaderProgram sProgram, ShaderProgram sProgramParticles, Double timeSinceStartup )
        {
            float angulo = (float)timeSinceStartup/2;
            float aumento = 0.3f; //usado para que en determinado momento el de atras aumente la rotacion y apunte al otro avion
            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion);
            objetos[1].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento1);
            objetos[2].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento2);

            //El 3ro es el perseguido, el 4to el que lo sigue
            objetos[3].transform.localToWorld = escala * Matrix4.CreateRotationX(rotX) * Matrix4.CreateRotationZ(-pi / 2+ angulo ) * Matrix4.CreateTranslation(posPerseguido);
            objetos[4].transform.localToWorld = escala * Matrix4.CreateRotationZ(-pi / 2 + angulo-retraso+ aumento) * Matrix4.CreateTranslation(posPerseguidor);

            for (int i = 0; i < objetos.Length; i++)
            {
                objetos[i].Dibujar(sProgram);
            }


        }


        public void DibujarDisparos(ShaderProgram sProgramUnlit)
        {
            for (int i = 0; i < posicionesDisparos.Count; i++)
            {   //list[i].Dibujar(sProgramParticles);
                sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.CreateTranslation(posicionesDisparos[i]));
                sProgramUnlit.SetUniformValue("figureColor", new Vector4(1f,0,0,1));
                cubo.Dibujar(sProgramUnlit);
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

            //Solo dispara y rola cuando esta en la region del principio del semicirculo de abajo
            if (disparar)
                if (angulo % (pi*2) > 4.0f && angulo % (pi*2) < 4.6f && posicionesDisparos.Count< MAXDisparos)
                {   
                    Disparar(sProgramParticles);                    
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
            /*   for (int i = 0; i < list.Count; i++)
                   list[i].Update();*/

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

        private void Disparar(ShaderProgram sProgramParticles)
        {   //origen es perseguidor. Dir es perseguido
            Vector3 pendiente = posPerseguido - posPerseguidor;
            //recta se expresa como origen+ t*pendiente. Calculo para que valor de t va a dar y=0 (q punto esta dentro del plano)
            // 0 = rayOrigin.Y + t * pendiente.Y            

            float t = -posPerseguidor.Y / pendiente.Y;
            Vector3 posDisparo = posPerseguidor + t * pendiente; posDisparo.Y += 1f;

            if (posicionesDisparos.Count > 0)
                {//Se agrega un control para que haya cierta separacion entre disparos y no sean infinitos
                if (Math.Abs(posicionesDisparos[(posicionesDisparos.Count - 1)].X - posDisparo.X) > 0.4f)
                    posicionesDisparos.Add(posDisparo);
                }
            else posicionesDisparos.Add(posDisparo);
            
            
            /* Smoke smokeParticles = new Smoke(posDisparo);
             smokeParticles.Build(sProgramParticles);
             list.Add(smokeParticles);*/
        }
    }
}
