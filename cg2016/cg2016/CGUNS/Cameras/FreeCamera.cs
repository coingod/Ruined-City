using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;


namespace CGUNS.Cameras
{
    /// <summary>
    /// Camara estilo FPS. 
    /// </summary>
    class FreeCamera : Camera
    {
        private Vector3 eye;
        private Vector3 front;
        private Vector3 up;
        private float speed;
        private float sensitivity;
        private float yaw;
        private float pitch;
        private bool firstMouse;
        private float lastX;
        private float lastY;

        private float fovReal;

        /// <summary>
        /// </summary>
        /// <param name="start"> Posicion inicial</param>
        /// <param name="target"> Posicion hacia donde mira la camara</param>
        /// <param name="speed"> 0.05 por defecto</param>
        public FreeCamera(Vector3 start, Vector3 target, float speed = 0.05f, float sensitivity = 0.3f)
            : base ()
        {
            eye = start;
            front = Vector3.Normalize(target - eye);
            up = Vector3.UnitY;
            this.speed = speed;
            this.sensitivity = sensitivity;
            firstMouse = true;
            yaw = -90;
            pitch = 0;

            fovReal = FieldOfView;
        }

        public override Vector3 Position()
        {
           return eye;         
        }   


        public override void setPosition(Vector3 pos)
        {
            eye = pos;
        }

        public Vector3 Front() {
            return front;
        }

        public Vector3 Side()
        {
            return Vector3.Cross(front, up);
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

            float dx = x - lastX;
            float dy = lastY - y;
            lastX = x;
            lastY = y;

            dx *= sensitivity;
            dy *= sensitivity;

            yaw += dx;
            pitch += dy;

            if (pitch > 89.0f) pitch = 89.0f;
            if (pitch < -89.0f) pitch = -89.0f;

            if (yaw > 179.0f) yaw = 179.0f;
            if (yaw < -179.0f) yaw = -179.0f;

            Vector3 aux = new Vector3();
            aux.X = (float) Math.Cos(MathHelper.DegreesToRadians(yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(pitch));
            aux.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            aux.Z = (float)Math.Sin(MathHelper.DegreesToRadians(yaw)) * (float)Math.Cos(MathHelper.DegreesToRadians(pitch));

            front = Vector3.Normalize(aux);
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
