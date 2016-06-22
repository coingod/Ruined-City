using System;
using System.Collections.Generic;
using System.Text;
using OpenTK; //La matematica
using CGUNS.Primitives;
using CGUNS.Shaders;

namespace CGUNS
{
    class Light
    {
        Vector4 position;
        Vector3 iambient;
        Vector3 ipuntual;
        float coneAngle;
        Vector3 coneDirection;
        int enabled;

        public LightGizmo gizmo;
        /*
        public Light()
        {
            gizmo = new LightGizmo(this);
        }
        */
        public void updateGizmo(ShaderProgram sProgram)
        {
            gizmo = new LightGizmo(this);
            gizmo.Build(sProgram);
        }

        public Vector4 Position
        {
            get
            {
                return position;
            }
            set
            {
                this.position = value;
            }
        }
        public Vector3 Iambient
        {
            get
            {
                return iambient;
            }
            set
            {
                this.iambient = value;
            }
        }
        public Vector3 Ipuntual
        {
            get
            {
                return ipuntual;
            }
            set
            {
                this.ipuntual = value;
            }
        }        
        public Vector3 ConeDirection
        {
            get
            {
                return coneDirection;
            }
            set
            {
                this.coneDirection = value;
            }
        }
        /// <summary>
        /// Cone Angle in degrees.
        /// </summary>
        public float ConeAngle
        {
            get
            {
                return coneAngle;
            }
            set
            {
                this.coneAngle = value;
            }
        }
        public int Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                this.enabled = value;
            }
        }
        public void Toggle()
        {
            if (Enabled == 0)
            {
                Enabled = 1;
            }
            else
            {
                Enabled = 0;
            }
        }
    }
}
