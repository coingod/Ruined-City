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
    {   private static float aux=10;
        private ObjetoGrafico[] objetos;
        private ISound[] sonidoAviones; //uno para cada uno por si se quiere usar distintos o en momentos diferentes
        private static Vector3 origen = new Vector3(-75.0f, 2.0f, 0.0f)* aux; //de donde sale el avion y fin hasta donde llega
        private Vector3 fin = new Vector3(75.0f, 1.0f, 0.0f) * aux;
        private Vector3 posicion = origen;
        private Vector3 desplazamiento1 = new Vector3(-1.0f, 0.0f, 2.0f) * aux; //utilizados para los otros dos aviones
        private Vector3 desplazamiento2 = new Vector3(-1.0f, 0.0f, -2.0f) * aux;

        //los dos que siguen son para simular un avion siguiendo a otro
        private Vector3 centroPerseguido = new Vector3(0.0f, 140.0f, 0.0f);
        private static float radio=120f;
        private Vector3 posPerseguido, posPerseguidor;
        private static float retraso = 3.14f/ 6; //Se utiliza para posicionar a un avion tanto mas atras que otro

        private static Matrix4 escala = Matrix4.CreateScale(1f)* Matrix4.CreateRotationY(3.14f/2);
        private Boolean disparar = false;
        //List<Smoke> list = new List<Smoke>();
        List<Vector3> posicionesDisparos = new List<Vector3>();
        Cube cubo = new Cube(0.4f, 0.4f, 0.4f);
        private float rotX = 0f; //usado en el avion perseguido
        private Boolean rotar = false;
       // private static double inclinacion = 0.5f; //usado para que el circulo quede inclinado


        public Aviones(ShaderProgram sProgram, int cantAviones, ISoundEngine engine, ShaderProgram sProgramUnlit)
        {
            objetos = new ObjetoGrafico[cantAviones];
            sonidoAviones = new ISound[cantAviones];

            Vector3D origen3D = new Vector3D(origen.X, origen.Y, origen.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            for (int i = 0; i < cantAviones; i++)
            {
                objetos[i] = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/b17.obj");
                objetos[i].Build(sProgram);     
                
            }
            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(origen);
            objetos[1].transform.localToWorld = escala * Matrix4.CreateTranslation(origen+ desplazamiento1);
            objetos[2].transform.localToWorld = escala * Matrix4.CreateTranslation(origen+ desplazamiento2);

            posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(3.14f), radio * (float)Math.Sin(3.14), 0);
            objetos[3].transform.localToWorld = escala * Matrix4.CreateRotationZ(3.14f/2)* Matrix4.CreateTranslation(posPerseguido);

            posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(3.14f-retraso), radio * (float)Math.Sin(3.14-retraso), 0);
            objetos[4].transform.localToWorld = escala * Matrix4.CreateRotationZ(3.14f / 2-retraso) * Matrix4.CreateTranslation(posPerseguidor);

            sonidoAviones[0] = engine.Play3D("files/audio/bell.wav", origen3D.X, origen3D.Y, origen3D.Z, true);
            sonidoAviones[1] = engine.Play3D("files/audio/bell.wav", desp1.X, desp1.Y, desp1.Z, true);
            sonidoAviones[2] = engine.Play3D("files/audio/bell.wav", desp2.X, desp2.Y, desp2.Z, true);
            sonidoAviones[3] = engine.Play3D("files/audio/bell.wav", desp2.X, desp2.Y, desp2.Z, true);
            sonidoAviones[4] = engine.Play3D("files/audio/bell.wav", desp2.X, desp2.Y, desp2.Z, true);

            for (int i=0; i<sonidoAviones.Length; i++)
                sonidoAviones[i].Volume = sonidoAviones[i].Volume / 4;

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
            objetos[3].transform.localToWorld = escala * Matrix4.CreateRotationX(rotX) * Matrix4.CreateRotationZ(-3.14f / 2+ angulo ) * Matrix4.CreateTranslation(posPerseguido);
            objetos[4].transform.localToWorld = escala * Matrix4.CreateRotationZ(-3.14f / 2 + angulo-retraso+ aumento) * Matrix4.CreateTranslation(posPerseguidor);

            for (int i = 0; i < objetos.Length; i++)
            {
                objetos[i].Dibujar(sProgram);
            }


        }

        public void DibujarDisparos(ShaderProgram sProgramUnlit)
        {
            for (int i = 0; i < posicionesDisparos.Count; i++)
            {
                //list[i].Dibujar(sProgramParticles);
                sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.CreateTranslation(posicionesDisparos[i]));
                sProgramUnlit.SetUniformValue("figureColor", new Vector4(1f,0,0,1));
                cubo.Dibujar(sProgramUnlit);

            }

        }


        public void Actualizar(Double timeSinceStartup, ShaderProgram sProgramParticles ) {
            for (int i = 0; i < objetos.Length; i++)
            {
                float blend = (float)timeSinceStartup/12 % 1;
                posicion = Vector3.Lerp(origen, fin, blend);
            }

            Vector3D origen3D = new Vector3D(posicion.X, posicion.Y, posicion.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            sonidoAviones[0].Position = origen3D;
            sonidoAviones[1].Position = desp1;
            sonidoAviones[2].Position = desp2;

            //Actualizacion de la persecucion
            double angulo = 3.14f + timeSinceStartup/2;

            posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(angulo), radio * (float)Math.Sin(angulo), 0);
            posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(angulo - retraso), radio * (float)Math.Sin(angulo - retraso), 0);

            if (angulo > 9 && angulo < 9.2f)
            { //es despues de q haya dado una vuelta
                disparar = true;
                rotar = true;
            }
            if (disparar)
                if (angulo % 6.28 > 4.0f && angulo % 6.28 < 4.6f && posicionesDisparos.Count<20)
                {
                    Disparar(sProgramParticles);                    
                }

            if (rotar)
            {
                if (angulo % 6.28 > 3.8f)
                    rotX += 0.1f;    //usada para el avion de adelante
                if (rotX > 6.28f)
                    {rotX = 0;      //vuelve a la posicion original cuando sale del rango del if
                    rotar = false;
                    }                
            }
            /*   for (int i = 0; i < list.Count; i++)
                   list[i].Update();*/
        }

        private void Disparar(ShaderProgram sProgramParticles)
        {   //origen es perseguidor. Dir es perseguido
            Vector3 pendiente = posPerseguido - posPerseguidor;
            //recta se expresa como origen+ t*pendiente. Calculo para que valor de t va a dar y=0 (q punto esta dentro del plano)
            // 0 = rayOrigin.Y + t * pendiente.Y            
            float t = -posPerseguidor.Y / pendiente.Y;
            float x = posPerseguidor.X + t * pendiente.X;
            float z = posPerseguidor.Z + t * pendiente.Z;


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
