using System;
using System.Collections.Generic;
using System.Text;

namespace CGUNS.Meshes.HalfEdge {
  struct HEHalfEdge {
    private int srcVertex;
    private int twinHEdge;
    private int adjFace;
    private int nextHEdge;
    private int prevHEdge;

    /// <summary>
    /// Gets or Sets the index of the starting Vertex of this Half-Edge.
    /// </summary>
    public int SrcVertex {
      get { return srcVertex; }
      set { srcVertex = value; }
    }
    /// <summary>
    /// Gets or Sets the index of the oppositely oriented adjacent Half-Edge.
    /// </summary>
    public int TwinHEdge {
      get { return twinHEdge; }
      set { twinHEdge = value; }
    }
    /// <summary>
    /// Gets or Sets the index of the unique Face that this Half-Edge belongs to.
    /// </summary>
    public int AdjFace {
      get { return adjFace; }
      set { adjFace = value; }
    }
    /// <summary>
    /// Gets or Sets the index of the next Half-Edge around the Face.
    /// </summary>
    public int NextHEdge {
      get { return nextHEdge; }
      set { nextHEdge = value; }
    }
    /// <summary>
    /// Gets or Sets the index of the previous Half-Edge around the Face.
    /// </summary>
    public int PrevHEdge {
      get { return prevHEdge; }
      set { prevHEdge = value; }
    }

  }
}
