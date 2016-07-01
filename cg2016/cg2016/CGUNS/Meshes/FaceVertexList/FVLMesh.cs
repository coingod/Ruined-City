using System;
using System.Collections.Generic;
using OpenTK;
using System.Text;
using CGUNS.Shaders;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;


namespace CGUNS.Meshes.FaceVertexList
{
    public class FVLMesh : Mesh
    {
        //Triangulos
        private List<FVLFace> faceList;
        //Vertices
        private List<Vector3> vertexList;
        //Coordenadas de textura
        private List<Vector2> texCordList;

        //Vectores Normales de a pares (Origen, Extremo)
        private List<Vector3> vertexNormalList;

        Vector3[] posiciones;

        public FVLMesh(string name) : base(name)
        {
            faceList = new List<FVLFace>();
            vertexList = new List<Vector3>();
            vertexNormalList = new List<Vector3>();
            texCordList = new List<Vector2>();

        }

        public FVLMesh()
        {
            faceList = new List<FVLFace>();
            vertexList = new List<Vector3>();
            vertexNormalList = new List<Vector3>();
            texCordList = new List<Vector2>();

        }

        public override List<int> IndicesDeMesh() {
            List<int> toReturn = new List<int>();
            foreach (FVLFace f in faceList) {
                toReturn.Add(f.VerticesDeCara()[0]);
                toReturn.Add(f.VerticesDeCara()[1]);
                toReturn.Add(f.VerticesDeCara()[2]);
            }
            return toReturn;
        }

        public List<Vector3> VertexList
        {
            get { return vertexList; }
        }

        public List<FVLFace> FaceList
        {
            get { return faceList; }
        }

        public List<Vector3> VertexNormalList
        {
            get { return vertexNormalList; }
        }

        public List<Vector2> TexCordList
        {
            get { return texCordList; }
        }

        public int VertexCount
        {
            get { return vertexList.Count; }
        }

        public int FaceCount
        {
            get { return faceList.Count; }
        }

        public int AddVertex(Vector3 vertex)
        {
            vertexList.Add(vertex);
            return vertexList.Count - 1;
        }

        public int AddVertexNormal(Vector3 normal)
        {
            vertexNormalList.Add(normal);
            return vertexNormalList.Count - 1;
        }

        public int AddTexCord(Vector2 texCord)
        {
            texCordList.Add(texCord);
            return texCordList.Count - 1;
        }

        public int AddFace(FVLFace face)
        {
            faceList.Add(face);
            return faceList.Count - 1;
        }

        public void PrintLists()
        {
            String sender = "FVLMesh.printLists: ";
            FVLFace face;
            List<int> faceVertexes;
            log(sender, "Vertex List has {0} items.", vertexList.Count);
            for (int i = 0; i < vertexList.Count; i++)
            {
                log("", "V[{0}] = ({1}, {2}, {3})", i, vertexList[i].X, vertexList[i].Y, vertexList[i].Z);
            }
            int cantFaces = faceList.Count;
            log(sender, "Face List has {0} items.", cantFaces);
            for (int i = 0; i < cantFaces; i++)
            {
                face = faceList[i];
                faceVertexes = face.VertexIndexes;
                String format = "F[{0}] = ";
                for (int j = 0; j < faceList[i].VertexCount; j++)
                {
                    format = format + " V[" + faceVertexes[j] + "],";
                }
                log("", format, i);
            }
            log(sender, "End!");
        }

        private void log(String sender, String format, params Object[] args)
        {
            Console.Out.WriteLine(sender + format, args);
        }

        public override void Build(ShaderProgram sProgram1, ShaderProgram sProgram2)
        {
            CrearVBOs();
            CrearVAO(sProgram1);
            CrearShadowVAO(sProgram2);
        }

        public override Vector3[] getVertices() {
            return vertexList.ToArray();
        }

        //Buffers para la Mesh
        private int h_VBO;                  //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int n_VBO;                  //Handle del Vertex Buffer Object (normales de los vertices)
        private int t_VBO;                  //Handle del Vertex Buffer Object (texturas de los vertices)
        private int tan_VBO;                //Handle del Vertex Buffer Object (tangentes de los vertices)
        private int bitan_VBO;              //Handle del Vertex Buffer Object (bitangentes de los vertices)
        private int h_EBO;                  //Handle del Elements Buffer Object (indices)
        private int h_VAO;                  //Handle del Vertex Array Object (Configuracion de los VBO anteriores)

        //Buffers para Normales Graficadas
        private int h_VBO_NormalVectors;    //Handle del Vertex Buffer Object (posiciones de los vertices)
        private int h_EBO_NormalVectors;    //Handle del Elements Buffer Object (indices)
        private int h_VAO_NormalVectors;    //Handle del Vertex Array Object (Configuracion de los VBO anteriores)

        private int h_ShadowVAO;

        private void CrearVBOs()
        {
            BufferTarget bufferType; //Tipo de buffer (Array: datos, Element: indices)
            IntPtr size;             //Tamanio (EN BYTES!) del buffer.
                                     //Hint para que OpenGl almacene el buffer en el lugar mas adecuado.
                                     //Por ahora, usamos siempre StaticDraw (buffer solo para dibujado, que no se modificara)
            BufferUsageHint hint = BufferUsageHint.StaticDraw;

            int[] indices;  //Los indices para formar las caras.
            int[] indicesNormalVectors;

            
            Vector2[] texturas;
            Vector3[] NormalVectors;
            Vector3[] normales;

            Vector4[] tangentes;
            Vector3[] bitangentes;

            reordenar(out posiciones, out texturas, out normales, out NormalVectors, out indices, out indicesNormalVectors);

            CalcularBaseTangente(posiciones, texturas, normales, out tangentes, out bitangentes);

            //VBO con el atributo "posicion" de los vertices.
            bufferType = BufferTarget.ArrayBuffer;

            size = new IntPtr(posiciones.Length * Vector3.SizeInBytes);
            h_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, h_VBO); //Lo selecciono como buffer de Datos actual.

            gl.BufferData<Vector3>(bufferType, size, posiciones, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "NormalVectors" de los vertices.
            size = new IntPtr(NormalVectors.Length * Vector3.SizeInBytes);
            h_VBO_NormalVectors = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, h_VBO_NormalVectors); //Lo selecciono como buffer de Datos actual.

            gl.BufferData<Vector3>(bufferType, size, NormalVectors, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "Normales" de los vertices.
            size = new IntPtr(normales.Length * Vector3.SizeInBytes);
            n_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, n_VBO); //Lo selecciono como buffer de Datos actual.

            gl.BufferData<Vector3>(bufferType, size, normales, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "texturas" de los vertices.
            size = new IntPtr(texturas.Length * Vector2.SizeInBytes);
            t_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, t_VBO); //Lo selecciono como buffer de Datos actual.

            gl.BufferData<Vector2>(bufferType, size, texturas, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "tangentes" de los vertices.
            size = new IntPtr(tangentes.Length * Vector4.SizeInBytes);
            tan_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, tan_VBO); //Lo selecciono como buffer de Datos actual.

            gl.BufferData<Vector4>(bufferType, size, tangentes, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con el atributo "bitangentes" de los vertices.
            size = new IntPtr(bitangentes.Length * Vector3.SizeInBytes);
            bitan_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, bitan_VBO); //Lo selecciono como buffer de Datos actual.

            gl.BufferData<Vector3>(bufferType, size, bitangentes, hint); //Lo lleno con la info.
            gl.BindBuffer(bufferType, 0); // Lo deselecciono (0: ninguno)

            //VBO con otros atributos de los vertices (color, normal, textura, etc).
            //Se pueden hacer en distintos VBOs o en el mismo.

            //EBO, buffer con los indices posiciones.
            bufferType = BufferTarget.ElementArrayBuffer;
            //size = new IntPtr(faceList.Count * sizeof(int));
            h_EBO = gl.GenBuffer();
            //indices = CarasToIndices();
            size = new IntPtr(indices.Length * sizeof(int));
            gl.BindBuffer(bufferType, h_EBO); //Lo selecciono como buffer de elementos actual.
            gl.BufferData<int>(bufferType, size, indices, hint);
            gl.BindBuffer(bufferType, 0);

            //EBO, buffer con los indices NormalVectors.
            bufferType = BufferTarget.ElementArrayBuffer;
            h_EBO_NormalVectors = gl.GenBuffer();
            size = new IntPtr(indicesNormalVectors.Length * sizeof(int));
            gl.BindBuffer(bufferType, h_EBO_NormalVectors); //Lo selecciono como buffer de elementos actual.
            gl.BufferData<int>(bufferType, size, indicesNormalVectors, hint);
            gl.BindBuffer(bufferType, 0);


        }

        private void reordenar(out Vector3[] vertices, out Vector2[] texturas, out Vector3[] normales, out Vector3[] NormalVectors, out int[] indices, out int[] indicesNormalVectors)
        {
            indices = new int[FaceCount * 3]; //OJO solo si  TODAS las caras son triágulos
            indicesNormalVectors = new int[FaceCount * 3 * 2];
            vertices = new Vector3[FaceCount * 3];
            texturas = new Vector2[FaceCount * 3];
            normales = new Vector3[FaceCount * 3];

            NormalVectors = new Vector3[FaceCount * 3 * 2];

            int i = 0; int k = 0;
            for (int f = 0; f < FaceCount; f++)
            {
                FVLFace cara = faceList[f];
                int[] verticesCara = cara.VerticesDeCara();
                int[] normalesCara = cara.NormalesDeCara();
                int[] texturasCara = cara.TexturasDeCara();
                int cuantosVertices = cara.VertexCount;

                for (int j = 0; j < cuantosVertices; j++)
                {
                    vertices[i] = vertexList[verticesCara[j]];

                    texturas[i] = texCordList[texturasCara[j]];

                    if (vertexNormalList.Count > 0)
                    {
                        //Normales reales
                        normales[i] = vertexNormalList[normalesCara[j]];

                        //Normales graficadas
                        indicesNormalVectors[k] = k;
                        indicesNormalVectors[k + 1] = k + 1;
                        NormalVectors[k] = vertices[i];
                        NormalVectors[k + 1] = (vertexNormalList[normalesCara[j]] * 0.1f + vertices[i]);
                        k += 2;
                    }

                    indices[i] = i;
                    i++;
                }
            }
        }

        private void CalcularBaseTangente(Vector3[] vertices, Vector2[] texturas, Vector3[] normales, out Vector4[] tangentes, out Vector3[] bitangentes)
        {
            int cantFaces = FaceCount;
            tangentes = new Vector4[cantFaces * 3];
            bitangentes = new Vector3[cantFaces * 3];

            //Para cada triangulo, se computa el deltaPos y deltaUV
            for (int i = 0; i < cantFaces * 3; i += 3)
            {
                //Vertices del triangulo
                Vector3 v0 = vertices[i + 0];
                Vector3 v1 = vertices[i + 1];
                Vector3 v2 = vertices[i + 2];

                //UVs del triangulo
                Vector2 uv0 = texturas[i + 0];
                Vector2 uv1 = texturas[i + 1];
                Vector2 uv2 = texturas[i + 2];

                //Edges del triangulo, deltaPos
                Vector3 deltaPos1 = v1 - v0;
                Vector3 deltaPos2 = v2 - v0;

                //deltaUV
                Vector2 deltaUV1 = uv1 - uv0;
                Vector2 deltaUV2 = uv2 - uv0;

                //Calculo el vector Tangente y Bitangente
                float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                Vector3 tangente = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;
                Vector3 bitangente = (deltaPos2 * deltaUV1.X - deltaPos1 * deltaUV2.X) * r;

                //Asigno la tangente del triangulo a cada uno de sus vertices
                tangentes[i + 0] = new Vector4(tangente);
                tangentes[i + 1] = new Vector4(tangente);
                tangentes[i + 2] = new Vector4(tangente);

                //Asigno la bitangente del triangulo a cada uno de sus vertices
                bitangentes[i + 0] = bitangente;
                bitangentes[i + 1] = bitangente;
                bitangentes[i + 2] = bitangente;
            }

            for (int i = 0; i < cantFaces * 3; i++)
            {
                Vector3 n = normales[i];
                Vector3 t = tangentes[i].Xyz;

                // Gram-Schmidt orthogonalize
                tangentes[i] = new Vector4((t - n * Vector3.Dot(n, t)).Normalized());

                // Calculate handedness
                tangentes[i].W = (Vector3.Dot(Vector3.Cross(n, t), bitangentes[i]) < 0.0f) ? -1.0f : 1.0f;
            }
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

            /* POSICIONES */

            // 1. Creamos el VAO
            h_VAO = gl.GenVertexArray(); //Pedimos un identificador de VAO a OpenGL.
            gl.BindVertexArray(h_VAO);   //Lo seleccionamos para trabajar/configurar.

            //2. Configuramos el VBO de posiciones.
            attribIndex = sProgram.GetVertexAttribLocation("vPos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, h_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            //2. Configuramos el VBO de texturas.
            attribIndex = sProgram.GetVertexAttribLocation("TexCoord"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 2;   // 2 componentes (s, t)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, t_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            //2. Configuramos el VBO de posiciones.
            attribIndex = sProgram.GetVertexAttribLocation("vNormal"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, n_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.
            
            //2. Configuramos el VBO de tangentes.
            attribIndex = sProgram.GetVertexAttribLocation("vTangente"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 4;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, tan_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.
            /*
            //2. Configuramos el VBO de bitangentes.
            attribIndex = sProgram.GetVertexAttribLocation("vBitangente"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, bitan_VBO); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.
            */
            // 2.a.El bloque anterior se repite para cada atributo del vertice (color, normal, textura..)

            // 3. Configuramos el EBO a utilizar. (como son indices, no necesitan info de layout)
            bufferType = BufferTarget.ElementArrayBuffer;
            gl.BindBuffer(bufferType, h_EBO);

            // 4. Deseleccionamos el VAO.
            gl.BindVertexArray(0);

            /* NormalVectors */

            // 1. Creamos el VAO
            h_VAO_NormalVectors = gl.GenVertexArray(); //Pedimos un identificador de VAO a OpenGL.
            gl.BindVertexArray(h_VAO_NormalVectors);   //Lo seleccionamos para trabajar/configurar.

            //2. Configuramos el VBO de posiciones.
            attribIndex = sProgram.GetVertexAttribLocation("vPos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            gl.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            gl.BindBuffer(bufferType, h_VBO_NormalVectors); //Seleccionamos el buffer a utilizar.
            gl.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            // 2.a.El bloque anterior se repite para cada atributo del vertice (color, normal, textura..)

            // 3. Configuramos el EBO a utilizar. (como son indices, no necesitan info de layout)
            bufferType = BufferTarget.ElementArrayBuffer;
            gl.BindBuffer(bufferType, h_EBO_NormalVectors);

            // 4. Deseleccionamos el VAO.
            gl.BindVertexArray(0);
        }

        protected void CrearShadowVAO(ShaderProgram sProgram)
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
            h_ShadowVAO = GL.GenVertexArray(); //Pedimos un identificador de VAO a OpenGL.
            GL.BindVertexArray(h_ShadowVAO);   //Lo seleccionamos para trabajar/configurar.

            //2. Configuramos el VBO de posiciones.
            attribIndex = sProgram.GetVertexAttribLocation("vPos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
            offset = 0;  //El primer dato esta al comienzo. (no hay offset).
            bufferType = BufferTarget.ArrayBuffer; //Buffer de Datos.

            GL.EnableVertexAttribArray(attribIndex); //Habilitamos el indice de atributo.
            GL.BindBuffer(bufferType, h_VBO); //Seleccionamos el buffer a utilizar.
            GL.VertexAttribPointer(attribIndex, cantComponentes, attribType, false, stride, offset);//Configuramos el layout (como estan organizados) los datos en el buffer.

            bufferType = BufferTarget.ElementArrayBuffer;
            GL.BindBuffer(bufferType, h_EBO);

            // 4. Deseleccionamos el VAO.
            GL.BindVertexArray(0);
        }

        public override void Dibujar(ShaderProgram sProgram)
        {
            sProgram.SetUniformValue("modelMatrix", transform.localToWorld);
            sProgram.SetUniformValue("material.Ka", material.Kambient);
            sProgram.SetUniformValue("material.Kd", material.Kdiffuse);
            sProgram.SetUniformValue("material.Shininess", material.Shininess);
            sProgram.SetUniformValue("material.Ks", material.Kspecular);
            //sProgram.SetUniformValue("normalMatrix", Matrix3.Transpose(Matrix3.Invert(new Matrix3(transform.localToWorld * viewMatrix))));

            //Si tiene una textura especial, asignarla
            if (textures.Count > 0)
                sProgram.SetUniformValue("ColorTex", textures[0]);
                        
            PrimitiveType primitive; //Tipo de Primitiva a utilizar (triangulos, strip, fan, quads, ..)
            int offset; // A partir de cual indice dibujamos?
            int count;  // Cuantos?
            DrawElementsType indexType; //Tipo de los indices.

            primitive = PrimitiveType.Triangles;  //Usamos trianglos.
            offset = 0;  // A partir del primer indice.
            count = FaceCount * 3; // Todos los indices.
            indexType = DrawElementsType.UnsignedInt; //Los indices son enteros sin signo.

            gl.BindVertexArray(h_VAO); //Seleccionamos el VAO a utilizar.
            gl.DrawElements(primitive, count, indexType, offset); //Dibujamos utilizando los indices del VAO.
            gl.BindVertexArray(0); //Deseleccionamos el VAO

            //Reseteamos la textura por defecto
            sProgram.SetUniformValue("ColorTex", 0);
        }

        public override void DibujarShadows(ShaderProgram sProgram)
        {
            PrimitiveType primitive; //Tipo de Primitiva a utilizar (triangulos, strip, fan, quads, ..)
            int offset; // A partir de cual indice dibujamos?
            int count;  // Cuantos?
            DrawElementsType indexType; //Tipo de los indices.

            primitive = PrimitiveType.Triangles;  //Usamos trianglos.
            offset = 0;  // A partir del primer indice.
            count = faceList.Count * 3; // Todos los indices.
            indexType = DrawElementsType.UnsignedInt; //Los indices son enteros sin signo.

            gl.BindVertexArray(h_ShadowVAO); //Seleccionamos el VAO a utilizar.
            gl.DrawElements(primitive, count, indexType, offset); //Dibujamos utilizando los indices del VAO.
            gl.BindVertexArray(0); //Deseleccionamos el VAO
        }

        public override void DibujarNormales(ShaderProgram sProgram)
        {
            sProgram.SetUniformValue("modelMatrix", transform.localToWorld);
            //sProgram.SetUniformValue("normalMatrix", Matrix3.Transpose(Matrix3.Invert(new Matrix3(transform.localToWorld * viewMatrix))));
            PrimitiveType primitive; //Tipo de Primitiva a utilizar (triangulos, strip, fan, quads, ..)
            int offset; // A partir de cual indice dibujamos?
            int count;  // Cuantos?
            DrawElementsType indexType; //Tipo de los indices.

            primitive = PrimitiveType.Lines;  //Usamos trianglos.
            offset = 0;  // A partir del primer indice.
            count = FaceCount * 3 * 2; // Todos los indices.
            indexType = DrawElementsType.UnsignedInt; //Los indices son enteros sin signo.

            gl.BindVertexArray(h_VAO_NormalVectors); //Seleccionamos el VAO a utilizar.
            gl.DrawElements(primitive, count, indexType, offset); //Dibujamos utilizando los indices del VAO.
            gl.BindVertexArray(0); //Deseleccionamos el VAO
        }

    }
}
