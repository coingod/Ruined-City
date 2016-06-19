using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CGUNS.Shaders;
using CGUNS.Meshes;
using OpenTK;
using CGUNS.Particles;

namespace CGUNS
{
    class Explosiones
    {
        private Vector3 sphereCenters = new Vector3(2.0f, 0.0f, 0.0f);
        private float sphereRadius = 3f;
        private double[] inicioExplosiones;
        private int maxExplosiones = 5;
        private ParticleEmitter[] explosiones;

        public Explosiones(int maximo) {
            explosiones = new ParticleEmitter[maximo];
            maxExplosiones = maximo;

            inicioExplosiones = new double[maxExplosiones];
            for (int i = 0; i < maxExplosiones; i++)
                inicioExplosiones[i] = -3;
        }


        public void CrearExplosion(double tiempoInicio, Vector3 posOrigen , ShaderProgram sProgramParticles)
        {
            double tiempoAux = inicioExplosiones[0]; //se busca un lugar para la explosion en el arreglo o se remplaza el mas antiguo
            int masAntigua = 0;
            for (int i = 0; i < maxExplosiones; i++)
                if (inicioExplosiones[i] < tiempoAux)
                {
                    tiempoAux = inicioExplosiones[i];
                    masAntigua = i;
                }
            explosiones[masAntigua] = new ParticleEmitter(posOrigen);//, Vector3.UnitY * 0.25f, 500);
            explosiones[masAntigua].Build(sProgramParticles);
            inicioExplosiones[masAntigua] = tiempoInicio;
        }

        public void Actualizar(double timeSinceStartup)
         {
            for (int i = 0; i < maxExplosiones; i++)
                if (timeSinceStartup > inicioExplosiones[i] && timeSinceStartup < inicioExplosiones[i] + 2)
                    explosiones[i].Update();

         }

        public void Dibujar(double timeSinceStartup, ShaderProgram sProgramParticles)
        {
            for (int i = 0; i < maxExplosiones; i++)
                if (timeSinceStartup > inicioExplosiones[i] && timeSinceStartup < inicioExplosiones[i] + 2)
                    explosiones[i].Dibujar(sProgramParticles);

        }

        public void setCentro(Vector3 centro)
        {
            sphereCenters = centro;
        }

        public Vector3 getCentro() {
            return sphereCenters;
        }

        public void setRadio(float radio)
        {
            sphereRadius = radio;
        }

        public float getRadio()
        {
            return sphereRadius;
        }

    }
}
