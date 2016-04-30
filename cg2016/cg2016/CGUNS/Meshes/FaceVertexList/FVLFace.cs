using System;
using System.Collections.Generic;
using System.Text;

namespace CGUNS.Meshes.FaceVertexList {
  public class FVLFace {
    private List<int> vertexIndexes;
    private List<int> normalIndexes;
    private List<int> texCordIndexes;


    public FVLFace() {
      vertexIndexes = new List<int>();
      normalIndexes = new List<int>();
      texCordIndexes = new List<int>();

    }
    public int VertexCount {
      get { return vertexIndexes.Count; }
    }
    public List<int> VertexIndexes {
      get { return vertexIndexes; }
      set { vertexIndexes = value; }
    }
 
    public List<int> NormalIndexes
    {
        get { return normalIndexes; }
        set { normalIndexes = value; }
    }
    public List<int> TexCordIndexes
    {
        get { return texCordIndexes; }
        set { texCordIndexes = value; }
    }


    public void AddVertex(int vertexIndex) {
      this.vertexIndexes.Add(vertexIndex);
    }

    public void AddNormal(int normalIndex)
    {
        this.normalIndexes.Add(normalIndex);
    }

    public void AddTexCord(int texCordIndex)
    {
        this.texCordIndexes.Add(texCordIndex);
    }


    public int[] VerticesDeCara()
    {
        return vertexIndexes.ToArray();
    }

    public int[] NormalesDeCara()
    {
        return normalIndexes.ToArray();
    }

    public int[] TexturasDeCara()
    {
        return texCordIndexes.ToArray();
    }
 
  }
}
