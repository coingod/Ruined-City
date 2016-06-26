using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Particles
{
    //Ejemplo de configuracion de un Sistema de Particulas
    public class Smoke : ParticleEmitter 
    {
        public Smoke(Vector3 position) : base(position)
        {
            //The minimum size each particle can be at the time when it is spawned.
            minSize = 0.5f;
            //The maximum size each particle can be at the time when it is spawned.
            maxSize = 1.0f;
            //The minimum lifetime of each particle, measured in seconds.
            minEnergy = 10;
            //The maximum lifetime of each particle, measured in seconds.
            maxEnergy = 15;
            //The maximum number of particles that will be spawned.
            maxEmission = 50;
            //Use this to make particles grow in size over their lifetime.
            sizeGrow = 1;
            //Values greater than zero will fadeout the aprticle over time.
            fadeOut = 1.0f;
            //The amount of random noise in the particles initial velocity.
            rndVelocityScale = 1f;
            //The starting speed of particles in world space, along X, Y, and Z.
            worldVelocity = new Vector3(-0.2f, 0.5f, -0.15f);
            //Scale of the sphere along X, Y, and Z that the particles are spawned inside.
            ellipsoid = new Vector3(0.4f, 0.1f, 0.4f);
        }

        public Smoke(Vector3 position, float vel) : base(position)
        {
            //The minimum size each particle can be at the time when it is spawned.
            minSize = 0.5f;
            //The maximum size each particle can be at the time when it is spawned.
            maxSize = 1.0f;
            //The minimum lifetime of each particle, measured in seconds.
            minEnergy = 10;
            //The maximum lifetime of each particle, measured in seconds.
            maxEnergy = 15;
            //The maximum number of particles that will be spawned.
            maxEmission = 80;
            //Use this to make particles grow in size over their lifetime.
            sizeGrow = 1;
            //Values greater than zero will fadeout the aprticle over time.
            fadeOut = 2.0f;
            //The amount of random noise in the particles initial velocity.
            rndVelocityScale = 2f;
            //The starting speed of particles in world space, along X, Y, and Z.
            worldVelocity = new Vector3(-0.2f, 5, -0.15f);
            //Scale of the sphere along X, Y, and Z that the particles are spawned inside.
            ellipsoid = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }
}
