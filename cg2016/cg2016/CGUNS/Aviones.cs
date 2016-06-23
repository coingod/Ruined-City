using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrrKlang;
using CGUNS.Meshes;
using CGUNS.Shaders;
using OpenTK;
using CGUNS.Primitives;

namespace CGUNS
{

    class Aviones
    {
        private ObjetoGrafico[] objetos;
        private ISound[] sonidoAviones; //uno para cada uno por si se quiere usar distintos o en momentos diferentes
        private static Vector3 origen = new Vector3(-75.0f, 2.0f, 0.0f); //de donde sale el avion y fin hasta donde llega
        private Vector3 fin = new Vector3(75.0f, 1.0f, 0.0f);
        private Vector3 posicion = origen;
        private Vector3 desplazamiento1 = new Vector3(-1.0f, 0.0f, 2.0f); //utilizados para los otros dos aviones
        private Vector3 desplazamiento2 = new Vector3(-1.0f, 0.0f, -2.0f);

        private static Matrix4 escala = Matrix4.CreateScale(0.1f)* Matrix4.CreateRotationY(3.14f/2);
        

        public Aviones(ShaderProgram sProgram, int cantAviones, ISoundEngine engine)
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

            sonidoAviones[0] = engine.Play3D("files/audio/bell.wav", origen3D.X, origen3D.Y, origen3D.Z, true);
            sonidoAviones[1] = engine.Play3D("files/audio/bell.wav", desp1.X, desp1.Y, desp1.Z, true);
            sonidoAviones[2] = engine.Play3D("files/audio/bell.wav", desp2.X, desp2.Y, desp2.Z, true);

            for (int i=0; i<sonidoAviones.Length; i++)
                sonidoAviones[i].Volume = sonidoAviones[i].Volume / 4;

        }

        public void Dibujar(ShaderProgram sProgram)
        {
            objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion);
            objetos[1].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento1);
            objetos[2].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento2);

            for (int i = 0; i < objetos.Length; i++)
            {
                objetos[i].Dibujar(sProgram);
            }
        }


        public void Actualizar(Double timeSinceStartup) {
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
        } 
    }
}
