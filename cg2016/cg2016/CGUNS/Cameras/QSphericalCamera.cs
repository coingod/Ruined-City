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
    class QSphericalCamera : Camera
    {
        private const float DEG2RAD = (float)(Math.PI / 180.0); //Para pasar de grados a radianes

        private float radius; //Distancia al origen.
        private float theta; //Angulo en el plano horizontal (XZ) desde el eje X+ (grados)
        private float phi; //Angulo desde el eje Y+. (0, 180)  menos un epsilon. (grados)

        //Valores necesarios para calcular la Matriz de Vista.
        private Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 target = new Vector3(0, 0, 0);
        private Vector3 up = Vector3.UnitY;

        private Quaternion cameraRot, qAux;
        private Vector3 cameraPos;

        private float deltaTheta = 0.05f;
        private float deltaPhi = 0.05f;
        private float distance = 5f;

        public QSphericalCamera(float radius = 5.0f, float theta = 45.0f, float phi = 30.0f,
            float zNear = 0.1f, float zFar = 250f, float fovy = 50 * DEG2RAD, float aspectRatio = 1) : base(zNear, zFar, fovy, aspectRatio)
        {
            //Posicion inicial de la camara.
            this.radius = radius;
            this.theta = theta;
            this.phi = phi;

            cameraPos = new Vector3(0, 0, -radius);
            cameraRot = Quaternion.FromAxisAngle(new Vector3(0, 0, 0), 1.0f);   //Quat identidad

            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), -theta * DEG2RAD);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), phi * DEG2RAD);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
        }

        public override Vector3 Position()
        { 
            //Matris de Transformacion del Espacio del Ojo al espacio del Mundo
            Matrix4 viewToWorld = ViewMatrix().Inverted();
            //Posicion de la camara en el espacio del ojo (Osea el origen)
            Vector4 eyePos = new Vector4(0, 0, 0, 1);
            //Transformo el origen de la camara en espacio del ojo al espacio del mundo
            return new Vector3(
                  Vector4.Dot(viewToWorld.Column0, eyePos),
                  Vector4.Dot(viewToWorld.Column1, eyePos),
                  Vector4.Dot(viewToWorld.Column2, eyePos)
                );
        }
    
        /// <summary>
        /// Retorna la Matriz de Vista que representa esta camara.
        /// </summary>
        /// <returns></returns>
        public override Matrix4 ViewMatrix()
        {   
            //Construimos la matriz y la devolvemos.
            Matrix4 posicion = Matrix4.CreateTranslation(cameraPos);
            Matrix4 rotacion = Matrix4.CreateFromQuaternion(cameraRot);
            //return Matrix4.LookAt(eye, target, up);
            return Matrix4.Mult(rotacion, posicion);            
        }
        
        public override void Acercar()
        {
            if ((distance > 0) && (distance < radius))
            {
                radius = radius - distance;
                cameraPos.Z = -radius;
            }
        }

        public override void Alejar()
        {
            if (distance > 0)
            {
                radius = radius + distance;
                cameraPos.Z = -radius;
            }
        }        

        public override void Arriba()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), -deltaPhi);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
        }

        public override void Abajo()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), deltaPhi);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
        }

        public override void Izquierda()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), -deltaTheta);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
        }

        public override void Derecha()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), deltaTheta);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
        }

        public override void GirarIzquierda()
        {
            throw new NotImplementedException("No tiene sentido girar izquierda con esta camara, es para mouse");
        }

        public override void GirarDerecha()
        {
            throw new NotImplementedException("No tiene sentido girar derecha con esta camara, es para mouse");
        }
    }
}
