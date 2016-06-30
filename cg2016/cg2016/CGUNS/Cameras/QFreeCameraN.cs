using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using CGUNS;
using CGUNS.Meshes;
using CGUNS.Cameras;

namespace cg2016.CGUNS.Cameras
{
    /// <summary>
    /// Camara estilo FPS. 
    /// </summary>
    class QFreeCameraN : Camera
    {
        private Vector3 eye;
        private Vector3 front;        
        private Vector3 up;

        private float speed;
        private float sensitivity;

        private bool firstMouse;
        private float lastX;
        private float lastY;

        private float pitch;
        private float yaw;

        private float fovReal;

        Quaternion frontRot;

        /// <summary>
        /// </summary>
        /// <param name="start"> Posicion inicial</param>
        /// <param name="target"> Posicion hacia donde mira la camara</param>
        /// <param name="speed"> 0.05 por defecto</param>
        public QFreeCameraN(Vector3 start, Vector3 target, float speed = 0.05f, float sensitivity = 0.3f) : base()
        {
            eye = new Vector3(0, 0, 20);
            target = new Vector3(0, 0, 0);
            front = Vector3.Normalize(target - eye);
            up = Vector3.UnitY;
            this.speed = speed;
            this.sensitivity = sensitivity;
            firstMouse = true;

            fovReal = FieldOfView;

            frontRot = Quaternion.FromAxisAngle(new Vector3(0, 0, 0), 1.0f);
        }

        public override Vector3 Position()
        {
            return eye;
        }

        /// <summary>
        /// Returns view matrix.
        /// </summary>
        /// <returns></returns>
        public override Matrix4 ViewMatrix()
        {
            return Matrix4.LookAt(eye, eye + front, up);
        }

        /// <summary>
        /// W en FPS
        /// </summary>
        public override void Acercar()
        {
            eye += speed * front;
        }

        /// <summary>
        /// S en FPS
        /// </summary>
        public override void Alejar()
        {
            eye -= speed * front;
        }

        /// <summary>
        /// A en FPS
        /// </summary>
        public override void Izquierda()
        {
            eye -= speed * Vector3.Normalize(Vector3.Cross(front, up));
        }

        /// <summary>
        /// D en FPS
        /// </summary>
        public override void Derecha()
        {
            eye += speed * Vector3.Normalize(Vector3.Cross(front, up));
        }

        /// <summary>
        /// Girar arriba con mouse.
        /// </summary>
        public override void Arriba()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Girar abajo con mouse.
        /// </summary>
        public override void Abajo()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Girar abajo con mouse.
        /// </summary>
        public override void GirarIzquierda()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Girar abajo con mouse.
        /// </summary>
        public override void GirarDerecha()
        {
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Segun las coordenadas del mouse se cambia el frente de la camara.
        /// </summary>
        /// <param name="x">X del mouse</param>
        /// <param name="y">Y del mouse</param>
        public void MouseCoords(float x, float y)
        {
            
            if (firstMouse)
            {
                lastX = x;
                lastY = y;
                firstMouse = false;
            }

            float dx = (x - lastX > 0) ? -0.001f : 0.001f;
            float dy = lastY - y;
            lastX = x;
            lastY = y;

            frontRot = Quaternion.FromAxisAngle(Vector3.UnitY, dx) * frontRot;
            frontRot.Normalize();
            //frontRot = Quaternion.FromAxisAngle(Vector3.UnitY, dy) * frontRot;
            frontRot.Normalize();
            front = Vector3.TransformVector(front, Matrix4.CreateFromQuaternion(frontRot).ClearTranslation());
            front.Normalize();
        }

        /// <summary>
        /// Simula zoom.
        /// </summary>
        /// <param name="delta">Delta que representa el scroll. 1 scroll up. -1 scroll down</param>
        public void MouseScroll(float delta)
        {
            delta *= 0.05f;
            if (delta > 0)
            {
                if (FieldOfView - delta > 0)
                    FieldOfView -= delta;
                else
                    FieldOfView = 0.01f;
            }
            else
            {
                if (FieldOfView - delta <= fovReal)
                    FieldOfView -= delta;
                else
                    FieldOfView = fovReal;
            }
        }
    }
}

