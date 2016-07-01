using CGUNS.Cameras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace cg2016.CGUNS.Cameras
{
    class CamaraTanque : FreeCamera
    {
        private Quaternion cameraRot;

        public CamaraTanque(Vector3 start, Vector3 target, float speed = 0.005F, float sensitivity = 0.03F) : base(start, target, speed, sensitivity)
        {
            //Para que la cam mire al tanque en el inicio.
            cameraRot = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), 3.1415f) * Quaternion.FromAxisAngle(new Vector3(1, 0, 0), -3.1415f/4); 
        }

        /// <summary>
        /// W en FPS
        /// </summary>
        public override void Acercar()
        {
            
        }

        /// <summary>
        /// S en FPS
        /// </summary>
        public override void Alejar()
        {
            
        }

        /// <summary>
        /// A en FPS
        /// </summary>
        public override void Izquierda()
        {
            
        }

        /// <summary>
        /// D en FPS
        /// </summary>
        public override void Derecha()
        {
            
        }

        /// <summary>
        /// Returns view matrix.
        /// </summary>
        /// <returns></returns>
        public override Matrix4 ViewMatrix()
        {
            //Construimos la matriz y la devolvemos.
            Matrix4 posicion = Matrix4.CreateTranslation(eye);
            Matrix4 rotacion = Matrix4.CreateFromQuaternion(cameraRot);
            return Matrix4.Mult(posicion, rotacion);
        }

        /// <summary>
        /// Segun las coordenadas del mouse se cambia el frente de la camara.
        /// </summary>
        /// <param name="x">X del mouse</param>
        /// <param name="y">Y del mouse</param>
        public override void MouseCoords(float x, float y)
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

            //YAW
            cameraRot = Quaternion.Multiply(cameraRot, Quaternion.FromAxisAngle(new Vector3(0, 1, 0), dx));
            cameraRot.Normalize();
            //PITCH
            cameraRot = Quaternion.Multiply(Quaternion.FromAxisAngle(new Vector3(1, 0, 0), -dy), cameraRot);                        
            cameraRot.Normalize();
        }
    }
}
