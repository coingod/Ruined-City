using System;
using OpenTK;
using System.Text;

namespace CGUNS.Meshes.HalfEdge {
  struct HEVertex {
    private Vector3 coord;
    private int outHEdge;

    /// <summary>
    /// Gets or Sets the 3D coordinates of this Vertex.
    /// </summary>
    public Vector3 Coord {
      get { return coord; }
      set { coord = value; }
    }
    /// <summary>
    /// Gets or Sets the index of one of the Half-Edges with origin on this Vertex.
    /// </summary>
    public int OutHEdge {
      get { return outHEdge; }
      set { outHEdge = value; }
    }
  }
}
