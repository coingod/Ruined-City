using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Particles
{
    //Contenedor para renderizar un gran numero de particulas respawneandolas y actualizandolas en funcion del tiempo de vida
    //Basado en el Legacy Particle System de Unity3D
    //http://docs.unity3d.com/Manual/class-EllipsoidParticleEmitter.html
    //Referencias:
    //http://www.opengl-tutorial.org/intermediate-tutorials/billboards-particles/particles-instancing/
    //http://www.openglsuperbible.com/2013/08/20/is-order-independent-transparency-really-necessary/

    public class ParticleEmitter
    {
        //Representa el estado de una sola particula.
        public struct Particle
        {
            public Vector3 Position, Velocity;
            public Vector4 Color;
            public float Life;
            public float Size;

            public Particle(Vector3 position, Vector3 velocity, Vector4 color, float life, float size)
            {
                Position = position;
                Velocity = velocity;
                Color = color;
                Life = life;
                Size = size;
            }
        }
        //Textura para las particulas
        int texture;
        
        //Origen de las particulas
        public Vector3 Position;
        public bool enabled = false;                     //If enabled, the emitter will emit particles.
        public float minSize = 0.1f;             //The minimum size each particle can be at the time when it is spawned.
        public float maxSize = 0.1f;               //The maximum size each particle can be at the time when it is spawned.
        public int minEnergy = 3;                       //The minimum lifetime of each particle, measured in seconds.
        public int maxEnergy = 3;                       //The maximum lifetime of each particle, measured in seconds.
        //private int minEmission = 1;//50;                    //The minimum number of particles that will be spawned every second.
        protected int maxEmission = 50;                    //The maximum number of particles that will be spawned.
        public Vector3 worldVelocity = Vector3.UnitY;     //The starting speed of particles in world space, along X, Y, and Z.
        protected Vector3 localVelocity = Vector3.Zero;    //The starting speed of particles along X, Y, and Z, measured in the object’s orientation.
        protected Vector3 rndVelocity = Vector3.Zero;      //A random speed along X, Y, and Z that is added to the velocity.
        public float rndVelocityScale = 2;                ////The amount of random noise in the particles initial velocity.
        //public Vector3 tangentVelocity = Vector3.Zero;  //The starting speed of particles along X, Y, and Z, across the Emitter’s surface.
        //public float angularVelocity = 0;               //The angular velocity of new particles in degrees per second.
        //public bool rndRotation;                        //If enabled, the particles will be spawned with random rotations.
        //public bool oneShot;                            //If enabled, the particle numbers specified by min & max emission is spawned all at once. If disabled, the particles are generated in a long stream.
        public Vector3 ellipsoid = Vector3.One;         //Scale of the sphere along X, Y, and Z that the particles are spawned inside.
        public float minEmitterRange;                   //Determines an empty area in the center of the sphere - use this to make particles appear on the edge of the sphere.
        public float sizeGrow = 0;                      //Use this to make particles grow in size over their lifetime.
        public float fadeOut = 0.1f;                    //Values greater than zero will fadeout the aprticle over time.

        //Buffer de particulas.
        protected Particle[] particles;

        // The VBO containing the 4 vertices of the particles.
        // Thanks to instancing, they will be shared by all particles.
        protected float[] particle_quad =
        {
            -0.5f, -0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.0f,
            0.5f, 0.5f, 0.0f,
        };
        protected float[] particle_quad_texCoords =
        {
            0.0f, 0.0f,
            1.0f, 0.0f,
            0.0f, 1.0f,
            1.0f, 1.0f,
        };
        //VBO con las posiciones y tamaños de las particulas
        protected Vector4[] particle_positions_size;
        //VBO con los colores de las particulas
        protected Vector4[] particle_colors;

        //Diferencial de tiempo para la animacion
        protected float delta = 0.01f;
        //Indice a la ultima particula respawneada
        protected int lastUsedParticle = 0;

        //Intervalo de spawn de aprticula p.Life/maxEmission
        float emissionPeriod;
        float time = 0;
        float lastSpawnTime;

        protected Random rand;

        public ParticleEmitter(Vector3 position)
        {
            Position = position;          
        }

        public void Setup()
        {
            rand = new Random();
            //Configuracion compartida por todas las particulas
            //Position = position;
            //this.worldVelocity = worldVelocity;
            //this.maxEmission = maxEmission;
            //newParticles = rand.Next(minEmission, maxEmission);

            //Genero las particulas y las almaceno en el buffer.
            particles = new Particle[maxEmission];
            for (int i = 0; i < maxEmission; i++)
                particles[i] = new Particle();

            //Inicializo los buffers para los VBO.
            particle_positions_size = new Vector4[maxEmission];
            particle_colors = new Vector4[maxEmission];

            enabled = true;
            emissionPeriod = (float) maxEnergy / maxEmission;
            time = emissionPeriod;
        }

        protected float RandomRange(float MinValue, float MaxValue)
        {
            double range = MaxValue - MinValue;
            double sample = rand.NextDouble();
            double scaled = (sample * range) + MinValue;
            return (float) scaled;
        }

        //Configura cada particula
        protected void SpawnParticle(int particleIndex)
        {
            Particle p = particles[particleIndex];

            //Calculo un punto random dentro del ellipsoide
            float phi = RandomRange(0, 2 * (float)Math.PI);
            float theta = RandomRange(0, (float)Math.PI);
            float x = (float) (ellipsoid.X * Math.Sin(theta) * Math.Cos(phi));
            float y = (float) (ellipsoid.Y * Math.Sin(theta) * Math.Sin(phi));
            float z = (float) (ellipsoid.Z * Math.Cos(theta));
            //Asigno los valores
            p.Position = Position + new Vector3(x, y, z);
            p.Velocity = worldVelocity + localVelocity + rndVelocity * rndVelocityScale;
            //p.Color = new Vector4(RandomRange(0, 1), RandomRange(0, 1), RandomRange(0, 1), 1.0f);
            p.Color = new Vector4(1, 1, 1, 1.0f);
            p.Size = RandomRange(minSize, maxSize);
            p.Life = RandomRange(minEnergy, maxEnergy);
            particles[particleIndex] = p;
        }

        //Spawnea y actualiza particulas
        public void Update()
        {
            time += delta;

            if (enabled)
            {
                /*
                if( (time - lastSpawnTime) > emissionPeriod)
                {
                    //Respawnear particulas
                    for (int i = 0; i < minEmission; i++)
                    {
                        //Calculo un vector offset con componentes entre -1 y 1
                        rndVelocity = new Vector3(
                            2 * (float)rand.NextDouble() - 1,
                            2 * (float)rand.NextDouble() - 1,
                            2 * (float)rand.NextDouble() - 1
                        );
                        int unusedParticle = FirstUnusedParticle();
                        SpawnParticle(unusedParticle);
                    }
                    lastSpawnTime = time;
                }
                */
                //Actualizar las que existen
                for (int i = 0; i < maxEmission; i++)
                {
                    Particle p = particles[i];
                    if(p.Life > 0)
                    {
                        p.Position += p.Velocity * delta;
                        if (fadeOut > 0)
                            p.Color.W = p.Life / (maxEnergy * fadeOut);
                        p.Size += (maxEnergy - p.Life) * sizeGrow * 0.001f;
                        //Actualizar los buffers
                        particle_positions_size[i] = new Vector4(p.Position);
                        particle_positions_size[i].W = p.Size; //Size
                        particle_colors[i] = p.Color;
                        //Reducir tiempo de vida
                        p.Life -= delta;
                        particles[i] = p;
                    }
                    else
                    {
                        if ((time - lastSpawnTime) > emissionPeriod)
                        {
                            //Calculo un vector offset con componentes entre -1 y 1
                            rndVelocity = new Vector3(
                                2 * (float)rand.NextDouble() - 1,
                                2 * (float)rand.NextDouble() - 1,
                                2 * (float)rand.NextDouble() - 1
                            )*0.1f;
                            SpawnParticle(i);
                            lastSpawnTime = time;
                        }
                    }
                }
            }
        }

        //Busco una particula que haya muerto para respawnearla
        protected int FirstUnusedParticle()
        {
            //Buscar a partir de la ultima particula usada, deberia retornar al toque
            for (int i = lastUsedParticle; i < maxEmission; ++i)
            {
                if (particles[i].Life <= 0.0f)
                {
                    lastUsedParticle = i;
                    return i;
                }
            }
            //Sino, buscar desde el principio
            for (int i = 0; i < lastUsedParticle; ++i)
            {
                if (particles[i].Life <= 0.0f)
                {
                    lastUsedParticle = i;
                    return i;
                }
            }
            // Si estan todas vivas, resetear la ultima particula usada.
            lastUsedParticle = 0;
            return 0;
        }

        public int Texture
        {
            get { return texture; }
            set { texture = value; }
        }

        //Construye los Buffers correspondientes de OpenGL para dibujar este objeto.
        public void Build(ShaderProgram sProgram)
        {
            Setup();
            CrearVBOs();
            CrearVAO(sProgram);
        }

        //Cascara del metodo dibujar, para que las clases que heredan lo sobreescriban de ser necesario.
        public virtual void Dibujar(ShaderProgram sProgram)
        {
            DibujarParticulas(sProgram);
        }

        //Dibuja el contenido de los Buffers de este objeto.
        protected void DibujarParticulas(ShaderProgram sProgram)
        {
            //Actualizar los buffers de posicion, tamaño y color
            UpdateVBOs();

            //Usamos la textura de este efecto
            sProgram.SetUniformValue("ColorTex", texture);

            //gl.Disable(EnableCap.DepthTest);
            gl.DepthMask(false);            //Dejo habilitado el test de profundidad pero no permito que escriba en el buffer
            gl.Enable(EnableCap.Blend);     //Habilito el blending
            //Ecuacion de Blending: Cresult = Csource ∗ Fsource + Cdestination ∗ Fdestination
            // Blending aditivo: Csource ∗ Fsource + Cdestination ∗ (1 - Csource)
            gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);  //Utilizo una ecuacion de blending para Transparencia.
            // Blending multiplicativo: Cdestination * Csource
            //gl.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.SrcColor);

            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            PrimitiveType primitive; //Tipo de Primitiva a utilizar (triangulos, strip, fan, quads, ..)
            int offset; // A partir de cual indice dibujamos?
            int count;  // Cuantos?

            primitive = PrimitiveType.TriangleStrip;  //Usamos triangulos.
            offset = 0;  // A partir del primer indice.
            count = 4;//indices.Length; // Todos los indices.

            //Dibujo cada una de las particulas
            //foreach (Particle p in particles)
            {
                gl.BindVertexArray(h_VAO); //Seleccionamos el VAO a utilizar.
                gl.VertexAttribDivisor(0, 0); // particles vertices : always reuse the same 4 vertices -> 0
                gl.VertexAttribDivisor(1, 1); // positions : one per quad (its center) -> 1
                gl.VertexAttribDivisor(2, 1); // color : one per quad -> 1
                gl.VertexAttribDivisor(3, 0); // texturas : always reuse the same 4 texCoords -> 0
                //gl.DrawArrays(primitive, offset, count);
                gl.DrawArraysInstanced(primitive, offset, count, maxEmission);
                gl.BindVertexArray(0); //Deseleccionamos el VAO
            }
            // Reset Blending
            //gl.Enable(EnableCap.DepthTest);
            gl.DepthMask(true);             //Vuelvo a habilitar las escrituras al buffer de profundidad
            gl.Disable(EnableCap.Blend);    //Desabilito el blending
            gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);      //Reutilizo la ecuacion de blending por defecto

            //Reseteamos la textura por defecto
            sProgram.SetUniformValue("ColorTex", 0);
        }

        #region CONFIGURACION DE BUFFERS DE OPENGL

        private int h_VBO; //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int texturas_VBO; //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int position_size_VBO; //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int color_VBO; //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int h_VAO; //Handle del Vertex Array Object (Configuracion de los dos anteriores)

        private void CrearVBOs()
        {
            BufferTarget bufferType; //Tipo de buffer (Array: datos, Element: indices)
            IntPtr size;             //Tamanio (EN BYTES!) del buffer.
            //Hint para que OpenGl almacene el buffer en el lugar mas adecuado.
            //Por ahora, usamos siempre StaticDraw (buffer solo para dibujado, que no se modificara)
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            //VBO con el atributo "posicion" de los vertices.
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(particle_quad.Length * sizeof(float));
            h_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, h_VBO); //Lo selecciono como buffer de Datos actual.
            gl.BufferData<float>(bufferType, size, particle_quad, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "posicion y tamaño" de las particulas.
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(particle_positions_size.Length * Vector4.SizeInBytes);
            position_size_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, position_size_VBO); //Lo selecciono como buffer de Datos actual.
            gl.BufferData<Vector4>(bufferType, size, particle_positions_size, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "color" de las particulas.
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(particle_colors.Length * Vector4.SizeInBytes);
            color_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, color_VBO); //Lo selecciono como buffer de Datos actual.
            gl.BufferData<Vector4>(bufferType, size, particle_colors, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "texturas" de los vertices.
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(particle_quad_texCoords.Length * sizeof(float));
            texturas_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, texturas_VBO); //Lo selecciono como buffer de Datos actual.
            gl.BufferData<float>(bufferType, size, particle_quad_texCoords, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con otros atributos de los vertices (color, normal, textura, etc).
            //Se pueden hacer en distintos VBOs o en el mismo.
        }
        private void UpdateVBOs()
        {
            //Update VBOs!
            BufferTarget bufferType; //Tipo de buffer (Array: datos, Element: indices)
            IntPtr size;             //Tamanio (EN BYTES!) del buffer.
            //Hint para que OpenGl almacene el buffer en el lugar mas adecuado.
            //Por ahora, usamos siempre StaticDraw (buffer solo para dibujado, que no se modificara)
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            //Position and Size
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(particle_positions_size.Length * Vector4.SizeInBytes);
            gl.BindBuffer(bufferType, position_size_VBO);
            //gl.BufferData<Vector4>(bufferType, size, particle_positions_size, hint); //Lo lleno con la info.
            gl.BufferData(bufferType, size, IntPtr.Zero, hint); // Buffer orphaning, a common way to improve streaming perf.
            gl.BufferSubData(bufferType, IntPtr.Zero, size, particle_positions_size);

            //Color
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(particle_colors.Length * Vector4.SizeInBytes);
            gl.BindBuffer(bufferType, color_VBO);
            //gl.BufferData<Vector4>(bufferType, size, particle_colors, hint); //Lo lleno con la info.
            gl.BufferData(bufferType, size, IntPtr.Zero, hint); // Buffer orphaning, a common way to improve streaming perf.
            gl.BufferSubData(bufferType, IntPtr.Zero, size, particle_colors);
        }

        private void CrearVAO(ShaderProgram sProgram)
        {
            // Indice del atributo a utilizar. Este indice se puede obtener de tres maneras:
            // Supongamos que en nuestro shader tenemos un atributo: "in vec3 vPos";
            // 1. Dejar que OpenGL le asigne un indice cualquiera al atributo, y para consultarlo hacemos:
            //    attribIndex = gl.GetAttribLocation(programHandle, "vPos") DESPUES de haberlo linkeado.
            // 2. Nosotros le decimos que indice queremos que le asigne, utilizando:
            //    gl.BindAttribLocation(programHandle, desiredIndex, "vPos"); ANTES de linkearlo.
            // 3. Nosotros de decimos al preprocesador de shader que indice queremos que le asigne, utilizando
            //    layout(location = xx) in vec3 vPos;
            //    En el CODIGO FUENTE del shader (Solo para #version 330 o superior)      
            int attribIndex;
            int cantComponentes; //Cantidad de componentes de CADA dato.
            VertexAttribPointerType attribType; // Tipo de CADA una de las componentes del dato.
            int stride; //Cantidad de BYTES que hay que saltar para llegar al proximo dato. (0: Tightly Packed, uno a continuacion del otro)
            int offset; //Offset en BYTES del primer dato.
            BufferTarget bufferType; //Tipo de buffer.

            // 1. Creamos el VAO
            h_VAO = gl.GenVertexArray(); //Pedimos un identificador de VAO a OpenGL.
            gl.BindVertexArray(h_VAO);   //Lo seleccionamos para trabajar/configurar.

            //2. Configuramos el VBO de posiciones de vertices.
            attribIndex = sProgram.GetVertexAttribLocation("vertexPos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos se toman de a 4.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, h_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            //2. Configuramos el VBO de posiciones y tamaños de particulas.
            attribIndex = sProgram.GetVertexAttribLocation("particlePos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 4;   // 4 componentes (x, y, z) + Size
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos se toman de a 4.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, position_size_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            //2. Configuramos el VBO de colores de particula.
            attribIndex = sProgram.GetVertexAttribLocation("particleColor"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 4;   // 4 componentes (r, g, b, a)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos se toman de a 4.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, color_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            //2. Configuramos el VBO de texturas de vertices.
            attribIndex = sProgram.GetVertexAttribLocation("TexCoords"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 2;   // 2 componentes (x, y)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos se toman de a 4.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, texturas_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            // 2.a.El bloque anterior se repite para cada atributo del vertice (color, normal, textura..)

            // 4. Deseleccionamos el VAO.
            gl.BindVertexArray(0);
        }
        #endregion
    }
}
