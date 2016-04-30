using System;
using System.Collections.Generic;
using System.Text;
using OpenTK; //La matematica


namespace CGUNS
{
    class Light
    {
        Vector4 position;
        Vector3 iambient;
        Vector3 idiffuse;
        Vector3 ispecular;
        float coneAngle;
        Vector3 coneDirection;
        int enabled;

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
        public Vector3 Idiffuse
        {
            get
            {
                return idiffuse;
            }
            set
            {
                this.idiffuse = value;
            }
        }
        public Vector3 Ispecular
        {
            get
            {
                return ispecular;
            }
            set
            {
                this.ispecular = value;
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
