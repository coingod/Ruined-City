using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CGUNS.Meshes.HalfEdge {
  struct HEFace {
    //Atributos
    private int anyHEdge;

    /// <summary>
    /// Gets or Sets the index of any Half-Edge bordering this Face.
    /// </summary>
    public int AnyHEdge {
      get { return anyHEdge; }
      set { anyHEdge = value; }
    }


  }
}
