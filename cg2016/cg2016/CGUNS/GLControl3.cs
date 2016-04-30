using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics;


namespace CGUNS
{
    /// <summary>
    /// Componente para Windows Forms.
    /// Hereda de GLControl y su unica funcion es setear propiedades para trabajar con
    /// un contexto de OpenGL 3.0 o superior. (Pipeline Programable)
    /// </summary>
    class GLControl3 : OpenTK.GLControl
    {
        public GLControl3()
            : base(GraphicsMode.Default, 3, 2, GraphicsContextFlags.ForwardCompatible)
        {
            /**
             * GLControl(GraphicsMode, major, minor, GraphicsContextFlags)
             * 
             * | OpenGL Version | GLSL Version | Shader Preprocessor |
             * -------------------------------------------------------
             * |      3.0       |     1.30     |    #version 130     |
             * |      3.1       |     1.40     |    #version 140     |
             * |      3.2       |     1.50     |    #version 150     |
             * |      3.3       |     3.30     |    #version 330     |
             * |      4.0       |     4.00     |    #version 400     |
             * |      4.1       |     4.10     |    #version 410     |
             * |      4.2       |     4.20     |    #version 420     |
             * |      4.3       |     4.30     |    #version 430     |
             * |      4.4       |     4.40     |    #version 440     |
             * */

        }

        private void InitializeComponent()
        {
      this.SuspendLayout();
      // 
      // GLControl3
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.Name = "GLControl3";
      this.Size = new System.Drawing.Size(1500, 1500);
      this.Load += new System.EventHandler(this.GLControl3_Load);
      this.ResumeLayout(false);

        }

        private void GLControl3_Load(object sender, EventArgs e)
        {

        }
    }
}
