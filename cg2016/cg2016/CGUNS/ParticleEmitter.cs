using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Meshes
{
    //Contenedor para renderizar un gran numero de particulas respawneandolas y actualizandolas en funcion del tiempo de vida
    public class ParticleEmitter
    {
        //Representa el estado de una sola particula.
        public struct Particle
        {
            public Vector3 Position, Velocity;
            public Vector4 Color;
            public float Life;

            public Particle(Vector3 position, Vector3 velocity, Vector4 color, float life)
            {
                Position = position;
                Velocity = velocity;
                Color = color;
                Life = life;
            }
        }

        //Representacion de una particula como cubo
        private Vector3[] quad; //Las posiciones de los vertices.
        private uint[] indices;  //Los indices para formar las caras.

        //Origen de las particulas
        private Vector3 Position;
        //Velocidad inicial de las particulas
        private Vector3 Velocity;

        //Buffer de particulas.
        private Particle[] particles;
        private int amount;

        //Cuantas particulas se respawnean por frame
        private int newParticles = 1;
        //Diferencial de tiempo para la animacion
        private float dt = 0.01f;
        //Indice a la ultima particula respawneada
        private int lastUsedParticle = 0;

        private Random rand;

        public ParticleEmitter(Vector3 position, Vector3 velocity, int amount)
        {
            Position = position; Velocity = velocity; rand = new Random(); this.amount = amount;
            //Genero las particulas y las almaceno en el buffer.
            particles = new Particle[amount];
            for (int i = 0; i < amount; i++)
                particles[i] = new Particle();

            //Por ahora cada particula es un pequeño cubo
            quad = new Vector3[8]; float s = 0.01f;
            quad[0] = new Vector3(0.5f, -0.5f, -0.5f) * s;
            quad[1] = new Vector3(0.5f, -0.5f, 0.5f) * s;
            quad[2] = new Vector3(-0.5f, -0.5f, 0.5f) * s;
            quad[3] = new Vector3(-0.5f, -0.5f, -0.5f) * s;
            quad[4] = new Vector3(0.5f, 0.5f, -0.5f) * s;
            quad[5] = new Vector3(0.5f, 0.5f, 0.5f) * s;
            quad[6] = new Vector3(-0.5f, 0.5f, 0.5f) * s;
            quad[7] = new Vector3(-0.5f, 0.5f, -0.5f) * s;

            indices = new uint[]{
                1, 3, 0,
                7, 5, 4,
                4, 1, 0,
                5, 2, 1,
                2, 7, 3,
                0, 7, 4,
                1, 2, 3,
                7, 6, 5,
                4, 5, 1,
                5, 6, 2,
                2, 6, 7,
                0, 3, 7
            };
        }

        //Configura cada particula
        private void RespawnParticle(int particleIndex, Vector3 offset)
        {
            //float random = ((rand.Next() % 100) - 50) / 10.0f;
            float rColor = 0.5f + ((rand.Next() % 100) / 100.0f);
            particles[particleIndex].Position = Position + offset;// * random;
            particles[particleIndex].Color = new Vector4(rColor, rColor, rColor, 1.0f);
            particles[particleIndex].Life = 3.0f;
            particles[particleIndex].Velocity = Velocity + offset;// * 0.1f;
        }

        //Spawnea y actualiza particulas
        public void Update()
        {
            //Respawnear particulas
            for (int i = 0; i < newParticles; i++)
            {
                //Calculo un vector offset con componentes entre -0.1 y 0.1
                Vector3 offset = new Vector3(
                    2*(float)rand.NextDouble()-1, 
                    2*(float)rand.NextDouble()-1, 
                    2*(float)rand.NextDouble()-1
                ) * 0.1f;
                //Vector3 offset = Vector3.Zero;
                int unusedParticle = FirstUnusedParticle();
                RespawnParticle(unusedParticle, offset);
            }
            //Actualizar las que existen
            for (int i = 0; i < amount; i++)
            {
                Particle p = particles[i];
                //Reducir tiempo de vida
                p.Life -= dt;
                //Si la particula vive, actualizarla
                //if (p.Life > 0.0f)
                {
                    p.Position += p.Velocity * dt;
                    p.Color.W -= dt * 2.5f;
                }
                particles[i] = p;
            }
        }

        //Busco una particula que haya muerto para respawnearla
        private int FirstUnusedParticle()
        {
            //Buscar a partir de la ultima particula usada, deberia retornar al toque
            for (int i = lastUsedParticle; i < amount; ++i)
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

        //Construye los Buffers correspondientes de OpenGL para dibujar este objeto.
        public void Build(ShaderProgram sProgram)
        {
            CrearVBOs();
            CrearVAO(sProgram);
        }

        //Dibuja el contenido de los Buffers de este objeto.
        public void Dibujar(ShaderProgram sProgram)
        {
            // Use additive blending to give it a 'glow' effect
            gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            PrimitiveType primitive; //Tipo de Primitiva a utilizar (triangulos, strip, fan, quads, ..)
            int offset; // A partir de cual indice dibujamos?
            int count;  // Cuantos?
            DrawElementsType indexType; //Tipo de los indices.

            primitive = PrimitiveType.Triangles;  //Usamos triangulos.
            offset = 0;  // A partir del primer indice.
            count = indices.Length; // Todos los indices.
            indexType = DrawElementsType.UnsignedInt; //Los indices son enteros sin signo.

            //Dibujo cada una de las particulas
            foreach (Particle p in particles)
            {
                //En el shader especifico la posicion de esta particula y su color
                sProgram.SetUniformValue("offset", p.Position);
                sProgram.SetUniformValue("color", p.Color);
                sProgram.SetUniformValue("life", p.Life);
                gl.BindVertexArray(h_VAO); //Seleccionamos el VAO a utilizar.
                gl.DrawElements(primitive, count, indexType, offset); //Dibujamos utilizando los indices del VAO.
                gl.BindVertexArray(0); //Deseleccionamos el VAO
            }
        }

        #region CONFIGURACION DE BUFFERS DE OPENGL

        private int h_VBO; //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int h_EBO; //Handle del Elements Buffer Object (indices)
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
            size = new IntPtr(quad.Length * Vector3.SizeInBytes);
            h_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, h_VBO); //Lo selecciono como buffer de Datos actual.
            gl.BufferData<Vector3>(bufferType, size, quad, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con otros atributos de los vertices (color, normal, textura, etc).
            //Se pueden hacer en distintos VBOs o en el mismo.

            //EBO, buffer con los indices.
            bufferType = BufferTarget.ElementArrayBuffer;
            size = new IntPtr(indices.Length * sizeof(int));
            h_EBO = gl.GenBuffer();
            gl.BindBuffer(bufferType, h_EBO); //Lo selecciono como buffer de elementos actual.
            gl.BufferData<uint>(bufferType, size, indices, hint);
            gl.BindBuffer(bufferType, 0);
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

            //2. Configuramos el VBO de posiciones.
            attribIndex = sProgram.GetVertexAttribLocation("quad"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos se toman de a 4.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, h_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            // 2.a.El bloque anterior se repite para cada atributo del vertice (color, normal, textura..)

            // 3. Configuramos el EBO a utilizar. (como son indices, no necesitan info de layout)
            bufferType = BufferTarget.ElementArrayBuffer;
            gl.BindBuffer(bufferType, h_EBO);

            // 4. Deseleccionamos el VAO.
            gl.BindVertexArray(0);
        }
        #endregion
    }
}
