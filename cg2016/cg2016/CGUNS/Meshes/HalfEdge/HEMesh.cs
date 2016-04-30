using System;
using System.Collections.Generic;
using OpenTK;
using CGUNS.Meshes.FaceVertexList;
using CGUNS.Parsers;

namespace CGUNS.Meshes.HalfEdge {
  class HEMesh {
    private static int UNDEFINED = -1;
    private List<HEVertex> vertexList;
    private List<HEFace> faceList;
    private List<HEHalfEdge> halfedgeList;
    public HEMesh() {
      vertexList = new List<HEVertex>();
      faceList = new List<HEFace>();
      halfedgeList = new List<HEHalfEdge>();
    //  throw new NotImplementedException("Falta implementar. No usar!");
    }
    
    public int VertexCount {
      get { return vertexList.Count; }
    }
    public int FaceCount {
      get { return faceList.Count; }
    }
    public int HEdgeCount {
      get { return halfedgeList.Count; }
    }

    public Vector3 VCoord(int vertexIndex) {
      return vertexList[vertexIndex].Coord;
    }
    public int VOutHEdge(int vertexIndex) {
      return vertexList[vertexIndex].OutHEdge;
    }

    public int FBoundaryHEdge(int faceIndex) {
      return faceList[faceIndex].AnyHEdge;
    }

    public int ESrcVertex(int hedgeIndex) {
      return halfedgeList[hedgeIndex].SrcVertex;
    }
    public int ETwinHEdge(int hedgeIndex) {
      return halfedgeList[hedgeIndex].TwinHEdge;
    }
    public int EAdjFace(int hedgeIndex) {
      return halfedgeList[hedgeIndex].AdjFace;
    }
    public int ENextHEdge(int hedgeIndex) {
      return halfedgeList[hedgeIndex].NextHEdge;
    }
    public int EPrevHEdge(int hedgeIndex) {
      return halfedgeList[hedgeIndex].PrevHEdge;
    }


    /// <summary>
    /// Adds a new Vertex to this mesh.
    /// </summary>
    /// <param name="vertexCoord">3D Coordinates of the Vertex.</param>
    /// <returns>The index of the newly created Vertex</returns>
    public int AddVertex(Vector3 vertexCoord) {
      HEVertex vertex = new HEVertex();
      vertex.Coord = vertexCoord;
      vertex.OutHEdge = UNDEFINED;
      vertexList.Add(vertex);
      return vertexList.Count - 1;
    }
    /// <summary>
    /// Adds a new Face to this mesh.
    /// </summary>
    /// <returns>The index of the newly created Face</returns>
    public int AddFace() {
      HEFace face = new HEFace();
      face.AnyHEdge = UNDEFINED;
      faceList.Add(face);
      return faceList.Count - 1;
    }
    /// <summary>
    /// If Half-Edge (src->dst) exists, returns its index.
    /// Otherwise, creates both Half-Edges (src->dst, dst->src) and returns the index of the first.
    /// </summary>
    /// <param name="srcVertexIndex"></param>
    /// <param name="destVertexIndex"></param>
    /// <returns></returns>
    public int AddEdge(int srcVertexIndex, int destVertexIndex) {
      if (VOutHEdge(srcVertexIndex) == UNDEFINED) { //El vertice origen todavia no tiene ningun edge saliente
        HEHalfEdge newHEdge1 = new HEHalfEdge();
        HEHalfEdge newHEdge2 = new HEHalfEdge();

      }
      return -1;
    }

    //Metodos de Consulta de Adyacencias
    public int[] GetVerticesFromHEdge(int hedgeIndex) {
      int[] resul = new int[2];
      resul[0] = ESrcVertex(hedgeIndex);
      resul[1] = ESrcVertex(ETwinHEdge(hedgeIndex));
      return resul;
    }
    public int[] GetFacesFromHEdge(int hedgeIndex) {
      int[] resul = new int[2];
      resul[0] = EAdjFace(hedgeIndex);
      resul[1] = EAdjFace(ETwinHEdge(hedgeIndex));
      return resul;
    }
    public List<int> GetHEdgesFromFace(int faceIndex){
      List<int> resul = new List<int>();
      int startHEdge = FBoundaryHEdge(faceIndex);
      int currHEdge = startHEdge;
      do{
        resul.Add(currHEdge);
        currHEdge = ENextHEdge(currHEdge);
      }while(currHEdge != startHEdge);
      return resul;
    }
    //Metodo Avanzado
    public int AddFace(List<int> vertexIndices) {
      int initHEdgeIndex = halfedgeList.Count;
      //Inserto todos los Half-Edge correspondientes a esta cara.
      int cant = vertexIndices.Count;
      for (int i = 0; i < cant; i++) {

      }
      return 0;
    }

  }
}
