using CGUNS.Shaders;
using OpenTK;
using System;
using OpenTK.Graphics.OpenGL;

namespace CGUNS.Primitives
{
    class ViewportQuad
    {
        private Vector2[] vPos; //Las posiciones de los vertices.
        private uint[] indices;  //Los indices para formar las caras.

        private int h_VBO; //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int h_EBO; //Handle del Elements Buffer Object (indices)
        private int h_VAO; //Handle del Vertex Array Object (Configuracion de los dos anteriores)


        public ViewportQuad()
        {
            vPos = new Vector2[]
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1.0f, 0.0f),
                new Vector2(1.0f, 1.0f),
                new Vector2(0.0f, 1.0f)
            };

            indices = new uint[]{
                0, 1, 2,
                0, 2, 3,
              };
        }

        /// <summary>
        /// Construye los Buffers correspondientes de OpenGL para dibujar este objeto.
        /// </summary>
        /// <param name="sProgram"></param>
        public void Build(ShaderProgram sProgram)
        {
            CrearVBOs();
            CrearVAO(sProgram);
        }

        /// <summary>
        /// Dibuja el contenido de los Buffers de este objeto.
        /// </summary>
        /// <param name="sProgram"></param>
        public void Dibujar(ShaderProgram sProgram)
        {
            PrimitiveType primitive; //Tipo de Primitiva a utilizar (triangulos, strip, fan, quads, ..)
            int offset; // A partir de cual indice dibujamos?
            int count;  // Cuantos?
            DrawElementsType indexType; //Tipo de los indices.

            primitive = PrimitiveType.Triangles;  //Usamos triangulos.
            offset = 0;  // A partir del primer indice.
            count = indices.Length; // Todos los indices.
            indexType = DrawElementsType.UnsignedInt; //Los indices son enteros sin signo.

            GL.BindVertexArray(h_VAO); //Seleccionamos el VAO a utilizar.
            GL.DrawElements(primitive, count, indexType, offset); //Dibujamos utilizando los indices del VAO.
            GL.BindVertexArray(0); //Deseleccionamos el VAO
        }

        
        private void CrearVBOs()
        {
            BufferTarget bufferType; //Tipo de buffer (Array: datos, Element: indices)
            IntPtr size;             //Tamanio (EN BYTES!) del buffer.
            //Hint para que OpenGl almacene el buffer en el lugar mas adecuado.
            //Por ahora, usamos siempre StaticDraw (buffer solo para dibujado, que no se modificara)
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            //VBO con el atributo "posicion" de los vertices.
            bufferType = BufferTarget.ArrayBuffer;
            size = new IntPtr(vPos.Length * Vector2.SizeInBytes);
            h_VBO = GL.GenBuffer();  //Le pido un Id de buffer a OpenGL
            GL.BindBuffer(bufferType, h_VBO); //Lo selecciono como buffer de Datos actual.
            GL.BufferData<Vector2>(bufferType, size, vPos, hint); //Lo lleno con la info.
            GL.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //EBO, buffer con los indices.
            bufferType = BufferTarget.ElementArrayBuffer;
            size = new IntPtr(indices.Length * sizeof(int));
            h_EBO = GL.GenBuffer();
            GL.BindBuffer(bufferType, h_EBO); //Lo selecciono como buffer de elementos actual.
            GL.BufferData<uint>(bufferType, size, indices, hint);
            GL.BindBuffer(bufferType, 0);
        }

        private void CrearVAO(ShaderProgram sProgram)
        {
            // Indice del atributo a utilizar. Este indice se puede obtener de tres maneras:
            // Supongamos que en nuestro shader tenemos un atributo: "in vec3 vPos";
            // 1. Dejar que OpenGL le asigne un indice cualquiera al atributo, y para consultarlo hacemos:
            //    attribIndex = GL.GetAttribLocation(programHandle, "vPos") DESPUES de haberlo linkeado.
            // 2. Nosotros le decimos que indice queremos que le asigne, utilizando:
            //    GL.BindAttribLocation(programHandle, desiredIndex, "vPos"); ANTES de linkearlo.
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
            h_VAO = GL.GenVertexArray(); //Pedimos un identificador de VAO a OpenGL.
            GL.BindVertexArray(h_VAO);   //Lo seleccionamos para trabajar/configurar.

            //2. Configuramos el VBO de posiciones.
            attribIndex = sProgram.GetVertexAttribLocation("vPos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 2;   // 2 componentes (x, y)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            GL.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            GL.BindBuffer(bufferType, h_VBO); //Seleccionamos el buffer a utilizar.
            GL.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            // 3. Configuramos el EBO a utilizar. (como son indices, no necesitan info de layout)
            bufferType = BufferTarget.ElementArrayBuffer;
            GL.BindBuffer(bufferType, h_EBO);

            // 4. Deseleccionamos el VAO.
            GL.BindVertexArray(0);
        }
    }
}
