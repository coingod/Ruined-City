﻿using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using CGUNS.Shaders;

namespace CGUNS.Primitives
{
    class LightGizmo
    {
        private Vector3[] vPos; //Las posiciones de los vertices.
        private uint[] indices;  //Los indices para formar las caras.
        private Light light;

        public LightGizmo( Light l,  float s = 0.075f )
        {
            light = l;

            if(light.Direccional == 1)
            {
                //El gizmo para las luces direccionales es una flecha.
                vPos = new Vector3[13];
                //Base de la flecha.
                vPos[0] = new Vector3(0.5f, -1f - 0.5f, -0.5f) * s * 0.75f;
                vPos[1] = new Vector3(0.5f, -1f - 0.5f, 0.5f) * s * 0.75f;
                vPos[2] = new Vector3(-0.5f, -1f - 0.5f, 0.5f) * s * 0.75f;
                vPos[3] = new Vector3(-0.5f, -1f - 0.5f, -0.5f) * s * 0.75f;
                vPos[4] = new Vector3(0.5f, 1f - 0.5f, -0.5f) * s * 0.75f;
                vPos[5] = new Vector3(0.5f, 1f - 0.5f, 0.5f) * s * 0.75f;
                vPos[6] = new Vector3(-0.5f, 1f - 0.5f, 0.5f) * s * 0.75f;
                vPos[7] = new Vector3(-0.5f, 1f - 0.5f, -0.5f) * s * 0.75f;
                //Punta de la flecha.
                vPos[8] = new Vector3(-1f, -1f + 0.5f, -1f) * s * 0.75f;
                vPos[9] = new Vector3(1f, -1f + 0.5f, -1f) * s * 0.75f;
                vPos[10] = new Vector3(0f, 1f + 0.5f, 0f) * s * 0.75f;
                vPos[11] = new Vector3(1f, -1f + 0.5f, 1f) * s * 0.75f;
                vPos[12] = new Vector3(-1f, -1f + 0.5f, 1f) * s * 0.75f;
                
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
                    0, 3, 7,
                    ////////
                    8, 10, 9,
                    9, 10, 11,
                    11, 10, 12,
                    12, 10, 8,
                    9, 12, 8,
                    9, 11, 12
                };
            }
            else if(light.ConeAngle < 180.0f)
            {
                //El gizmo para las luces spot es un cono.
                vPos = new Vector3[5];
                vPos[0] = new Vector3(-1f, -1f, -1f) * s;
                vPos[1] = new Vector3(1f, -1f, -1f) * s;
                vPos[2] = new Vector3(0f, 1f, 0f) * s;
                vPos[3] = new Vector3(1f, -1f, 1f) * s;
                vPos[4] = new Vector3(-1f, -1f, 1f) * s;

                indices = new uint[]{
                    0, 2, 1,
                    1, 2, 3,
                    3, 2, 4,
                    4, 2, 0,
                    1, 4, 0,
                    1, 3, 4
                };
            }
            else
            {
                //El gizmo para las luces puntuales es un cubo.
                vPos = new Vector3[8];
                vPos[0] = new Vector3(0.5f, -0.5f, -0.5f) * s;
                vPos[1] = new Vector3(0.5f, -0.5f, 0.5f) * s;
                vPos[2] = new Vector3(-0.5f, -0.5f, 0.5f) * s;
                vPos[3] = new Vector3(-0.5f, -0.5f, -0.5f) * s;
                vPos[4] = new Vector3(0.5f, 0.5f, -0.5f) * s;
                vPos[5] = new Vector3(0.5f, 0.5f, 0.5f) * s;
                vPos[6] = new Vector3(-0.5f, 0.5f, 0.5f) * s;
                vPos[7] = new Vector3(-0.5f, 0.5f, -0.5f) * s;

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

            primitive = PrimitiveType.Triangles;  //Usamos quads.
            offset = 0;  // A partir del primer indice.
            count = indices.Length; // Todos los indices.
            indexType = DrawElementsType.UnsignedInt; //Los indices son enteros sin signo.
            
            Vector4 figColor; //Color que usaremos para cada eje;

            gl.BindVertexArray(h_VAO); //Seleccionamos el VAO a utilizar.

            if(light.Enabled == 1)
            {
                //figColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                figColor = new Vector4(light.Ipuntual, 1.0f);
                sProgram.SetUniformValue("figureColor", figColor);
            }
            else
            {
                figColor = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                sProgram.SetUniformValue("figureColor", figColor);
            }

            Matrix4 modelMatrix;

            //Si es Direccional, la roto para que apunte en la direccion correcta
            if (light.Direccional == 1 || light.ConeAngle < 180)
            {
                //Direccion a la cual debe apuntar el nuevo eje Y (UP) local
                Vector3 upDir = new Vector3(light.Position.X, light.Position.Y, light.Position.Z).Normalized();
                //Calculo el eje de rotacion para rotar el versor Y hacia el versor direccion nuevo
                Vector3 eje = Vector3.Cross(Vector3.UnitY, upDir);
                double angle;
                //Si la direccion coincide con -Y, roto 180° en el eje X .
                if ((eje == Vector3.Zero) && (upDir.Y < 0))
                {
                    angle = Math.PI;
                    eje = Vector3.UnitX;
                }
                else
                {
                    //El angulo que debo rotar, es el angulo entre el eje Y y la direccion
                    angle = Math.Acos(Vector3.Dot(Vector3.UnitY, upDir));
                }
                //Calculo la matriz de rotacion
                Matrix4 rotation = Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(eje, (float)angle));
                //Traslado el gizmo para que su posicion en el mundo sea la correcta
                Matrix4 translation = Matrix4.CreateTranslation(new Vector3(light.Position));
                //Primero aplico la rotacion, luego lo traslado
                modelMatrix = Matrix4.Mult(rotation, translation);
            }
            else
            {
                //Posiciono la luz
                modelMatrix = Matrix4.CreateTranslation(new Vector3(light.Position));
            }

            sProgram.SetUniformValue("modelMatrix", modelMatrix);

            gl.DrawElements(primitive, count, indexType, offset); //Dibujamos utilizando los indices del VAO.
            gl.BindVertexArray(0); //Deseleccionamos el VAO

        }

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
            size = new IntPtr(vPos.Length * Vector3.SizeInBytes);
            h_VBO = gl.GenBuffer();  //Le pido un Id de buffer a OpenGL
            gl.BindBuffer(bufferType, h_VBO); //Lo selecciono como buffer de Datos actual.
            gl.BufferData<Vector3>(bufferType, size, vPos, hint); //Lo lleno con la info.
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
            attribIndex = sProgram.GetVertexAttribLocation("vPos"); //Yo lo saco de mi clase ProgramShader.
            cantComponentes = 3;   // 3 componentes (x, y, z)
            attribType = VertexAttribPointerType.Float; //Cada componente es un Float.
            stride = 0;  //Los datos estan uno a continuacion del otro.
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
    }
}
