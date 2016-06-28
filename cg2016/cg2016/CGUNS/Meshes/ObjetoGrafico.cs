using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Meshes
{
    public class ObjetoGrafico
    {
        List<Mesh> meshes;
        List<Vector3> meshesColor;
        Transform rootTransform;

        public ObjetoGrafico()
        {
            meshes = new List<Mesh>();
            transform = new Transform();
        }

        public ObjetoGrafico(string path)
        {
            meshes = CGUNS.Parsers.ObjFileParser.parseFile(path).Cast<Mesh>().ToList();
            transform = new Transform();

            //Setup random colors
            meshesColor = new List<Vector3>();
            Random rand2 = new Random();
            foreach (Mesh m in meshes)
            {
                meshesColor.Add(new Vector3((float)rand2.NextDouble(), (float)rand2.NextDouble(), (float)rand2.NextDouble()));
            }
        }

        public Transform transform
        {
            get { return rootTransform; }
            set
            {
                rootTransform = value;
                UpdateAllTransforms();
            }
        }

        private void UpdateAllTransforms()
        {
            foreach (Mesh m in meshes)
            {
                m.Transform = rootTransform;
            }
        }

        public List<Mesh> Meshes
        {
            get { return meshes; }
            set { meshes = value; }
        }

        public void AddMesh(Mesh m)
        {
            meshes.Add(m);
        }

        public void RemoveMesh(Mesh m)
        {
            meshes.Remove(m);
        }

        public void AddTextureToAllMeshes(int id)
        {
            foreach (Mesh m in Meshes)
                m.AddTexture(id);
        }

        public void ClearMeshes()
        {
            meshes.Clear();
        }

        public void Dibujar(ShaderProgram sProgram)
        {
            foreach (Mesh m in meshes)
            {
                m.Dibujar(sProgram);
            }
        }

        public void DibujarShadows(ShaderProgram sProgram)
        {
            foreach (Mesh m in meshes)
            {
                m.DibujarShadows(sProgram);
            }
        }

        public void DibujarNormales(ShaderProgram sProgram)
        {
            foreach (Mesh m in meshes)
            {
                m.DibujarNormales(sProgram);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sProgram1">Shader para dibujar objeto</param>
        /// <param name="sProgram2">Corresponde al shader de sombras</param>
        public void Build(ShaderProgram sProgram1, ShaderProgram sProgram2)
        {
            foreach (Mesh m in meshes)
                m.Build(sProgram1, sProgram2);
        }

        public List<Vector3> getMeshVertices(String name){
            List<Vector3> aux = new List<Vector3>();
            foreach (Mesh m in meshes)
            {
                if (m.Name.Equals(name))
                    foreach (Vector3 vertex in m.getVertices())
                    {
                        aux.Add(vertex);
                    }
            }
            return aux;
        }

        public List<Vector3> getAllMeshVertices() {
            List<Vector3> aux = new List<Vector3>();
            Vector3[] verticesDeM;
            foreach (Mesh m in meshes)
            {
                verticesDeM = m.getVertices();
                foreach (Vector3 vertex in verticesDeM)
                {
                    aux.Add(vertex);
                }
            }
            return aux;
        }

        public List<int> getIndicesDeMesh(String name) {
            foreach (Mesh m in meshes)
            {
                if (m.Name.Equals(name))
                    return m.IndicesDeMesh();
            }
            return new List<int>();
        }

        public List<int> getIndicesDeMeshes()
        {
            List<int> toReturn = new List<int>();
            foreach (Mesh m in meshes)
            {
                toReturn.Concat(m.IndicesDeMesh());
            }
            return toReturn;
        }
    }
}
