using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;

namespace CGUNS.Cameras
{
    /// <summary>
    /// Representa una Camara en coordenadas esfericas.
    /// La camara apunta y orbita alrededor del origen de coordenadas (0,0,0).
    /// El vector "up" de la camara es esl eje "Y" (0,1,0).
    /// La posicion de la camara esta dada por 3 valores: Radio, Theta, Phi.
    /// </summary>
    class QSphericalCamera
    {
        private const float DEG2RAD = (float)(Math.PI / 180.0); //Para pasar de grados a radianes

        private Matrix4 projMatrix; //Matriz de Proyeccion.

        private float radius; //Distancia al origen.
        private float theta; //Angulo en el plano horizontal (XZ) desde el eje X+ (grados)
        private float phi; //Angulo desde el eje Y+. (0, 180)  menos un epsilon. (grados)

        //Valores necesarios para calcular la Matriz de Vista.
        private Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 target = new Vector3(0, 0, 0);
        private Vector3 up = Vector3.UnitY;

        private Quaternion cameraRot, qAux;
        private Vector3 cameraPos; 

        public QSphericalCamera()
        {
            //Por ahora la matriz de proyeccion queda fija. :)
            float fovy = 50 * DEG2RAD; //50 grados de angulo.
            float aspectRadio = 1; //Cuadrado
            float zNear = 0.1f; //Plano Near
            float zFar = 100f;  //Plano Far
            projMatrix = Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRadio, zNear, zFar);

            //Posicion inicial de la camara.
            radius = 5.0f;
            theta = 45.0f;
            phi = 30.0f;

            cameraPos = new Vector3(0, 0, -radius);
            cameraRot = Quaternion.FromAxisAngle(new Vector3(0, 0, 0), 1.0f);   //Quat identidad

            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), -theta * DEG2RAD);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), phi * DEG2RAD);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
        }

        public Vector3 getPosition()
        {
            //Matris de Transformacion del Espacio del Ojo al espacio del Mundo
            Matrix4 viewToWorld = getViewMatrix().Inverted();
            //Posicion de la camara en el espacio del ojo (Osea el origen)
            Vector4 eyePos = new Vector4(0,0,0,1);
            //Transformo el origen de la camara en espacio del ojo al espacio del mundo
            return new Vector3(
              Vector4.Dot(viewToWorld.Column0, eyePos),
              Vector4.Dot(viewToWorld.Column1, eyePos),
              Vector4.Dot(viewToWorld.Column2, eyePos)
              );
        }
    
        /// <summary>
        /// Retorna la Matriz de Projeccion que esta utilizando esta camara.
        /// </summary>
        /// <returns></returns>
        public Matrix4 getProjectionMatrix()
        {
            return projMatrix;
        }
        /// <summary>
        /// Retorna la Matriz de Vista que representa esta camara.
        /// </summary>
        /// <returns></returns>
        public Matrix4 getViewMatrix()
        {   
            //Pasamos de sistema esferico, a sistema cartesiano
            //eye.Y = (float)(radius * Math.Cos(phi * DEG2RAD));
            //eye.X = (float)(radius * Math.Sin(phi * DEG2RAD) * Math.Cos(theta * DEG2RAD));
            //eye.Z = (float)(radius * Math.Sin(phi * DEG2RAD) * Math.Sin(theta * DEG2RAD));
            //Construimos la matriz y la devolvemos.
            Matrix4 posicion = Matrix4.CreateTranslation(cameraPos);
            Matrix4 rotacion = Matrix4.CreateFromQuaternion(cameraRot);
            //return Matrix4.LookAt(eye, target, up);
            return Matrix4.Mult(rotacion, posicion);
        }


        public void Acercar(float distance)
        {
            if ((distance > 0) && (distance < radius))
            {
                radius = radius - distance;
                cameraPos.Z = -radius;
            }
        }

        public void Alejar(float distance)
        {
            if (distance > 0)
            {
                radius = radius + distance;
                cameraPos.Z = -radius;
            }
        }

        private float deltaTheta = 0.1f;
        private float deltaPhi = 0.1f;

        public void Arriba()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), -deltaPhi);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
            /*
            phi = phi - deltaPhi;
            if (phi < 10)
            {
                phi = 10;
            }
             */
        }

        public void Abajo()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), deltaPhi);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
            /*
            phi = phi + deltaPhi;
            if (phi > 170)
            {
                phi = 170;
            }
             */
        }

        public void Arriba2()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), -deltaPhi);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            /*
            phi = phi - deltaPhi;
            if (phi < 10)
            {
                phi = 10;
            }
             */
        }

        public void Abajo2()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), deltaPhi);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            /*
            phi = phi + deltaPhi;
            if (phi > 170)
            {
                phi = 170;
            }
             */
        }

        public void Izquierda()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), -deltaTheta);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            /*
            theta = theta + deltaTheta;
             */
        }

        public void Derecha()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), deltaTheta);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            /*
            theta = theta - deltaTheta;
             */
        }


    }
}
