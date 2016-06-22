using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;

namespace CGUNS.Meshes
{
    public abstract class Mesh
    {
        protected string name;
        protected Transform transform;
        //Indices de las texturas (Diffuse, Normal, Specular)
        public List<int> textures;

        public Mesh(string name)
        {
            this.name = name;
            transform = new Transform();
            textures = new List<int>();
        }

        public Mesh()
        {
            name = "Mesh";
            transform = new Transform();
            textures = new List<int>();
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Transform Transform
        {
            get { return transform; }
            set { transform = value; }
        }

        public int GetTexture(int value)
        {
            return textures[value];
        }

        public int AddTexture(int textureId)
        {
            textures.Add(textureId);
            return textures.Count - 1;
        }

        public abstract void Dibujar(ShaderProgram sProgram);

        public abstract void DibujarNormales(ShaderProgram sProgram);

        public abstract void Build(ShaderProgram sProgram);

        public abstract Vector3[] getVertices();
    }
}
