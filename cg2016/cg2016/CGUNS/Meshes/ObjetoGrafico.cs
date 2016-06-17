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

        public void ClearMeshes()
        {
            meshes.Clear();
        }

        public void Dibujar(ShaderProgram sProgram, Matrix4 mvMatrix)
        {
            int i = 0;
            foreach (Mesh m in meshes)
            {
                //sProgram.SetUniformValue("figureColor", new Vector4(meshesColor[i++]));
                //sProgram.SetUniformValue("kd", meshesColor[i] * 0.6f);
                //sProgram.SetUniformValue("ka", meshesColor[i++] * 0.1f);
                m.Dibujar(sProgram, mvMatrix);
            }
        }

        public void setModelsMatrix(Matrix4 modelMat) {
            foreach (Mesh m in meshes)
                m.SetModelMatrix(modelMat);
        }

        public void DibujarNormales(ShaderProgram sProgram, Matrix4 mvMatrix)
        {
            int i = 0;
            foreach (Mesh m in meshes)
            {
                //sProgram.SetUniformValue("figureColor", new Vector4(1.0f, 0.0f, 0.0f, 1.0f));
                //sProgram.SetUniformValue("ka", new Vector3(1.0f, 0.0f, 0.0f));
                //sProgram.SetUniformValue("kd", new Vector3(1.0f, 0.0f, 0.0f));
                //sProgram.SetUniformValue("ks", new Vector3(0.0f, 0.0f, 0.0f));
                m.DibujarNormales(sProgram, mvMatrix);
            }
        }

        public void Build(ShaderProgram sProgram)
        {
            foreach (Mesh m in meshes)
                m.Build(sProgram);
        }

    }
}
