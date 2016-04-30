using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;

namespace CGUNS.Meshes {
  public abstract class Mesh {
      //protected Matrix4 transform;
      protected Transform transform;

    public Mesh() {
      //transform = Matrix4.Identity;
      transform = new Transform();
    }

    public Transform Transform {
        get { return transform; }
        set { transform = value; }
    }

    public abstract void Dibujar(ShaderProgram sProgram, Matrix4 mvMatrix);

    public abstract void DibujarNormales(ShaderProgram sProgram, Matrix4 mvMatrix);

    public abstract void Build(ShaderProgram sProgram);
 
  }
}
