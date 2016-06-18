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

        //Configuracion de la matriz de proyeccion
        private float _fieldOfView;
        private float _aspect;
        private float _nearClipPlane;
        private float _farClipPlane;

        //Valores necesarios para calcular la Matriz de Vista.
        private Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 target = new Vector3(0, 0, 0);
        private Vector3 up = Vector3.UnitY;

        private Quaternion cameraRot, qAux;
        private Vector3 cameraPos; 

        public QSphericalCamera(float radius = 5.0f, float theta = 45.0f, float phi = 30.0f,
            float zNear = 0.1f, float zFar = 100f, float fovy = 50 * DEG2RAD, float aspectRatio = 1)
        {
            //Posicion inicial de la camara.
            this.radius = radius;
            this.theta = theta;
            this.phi = phi;
            //Matriz
            _fieldOfView = fovy;
            _aspect = aspectRatio;
            _nearClipPlane = zNear;
            _farClipPlane = zFar;

            projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);

            cameraPos = new Vector3(0, 0, -radius);
            cameraRot = Quaternion.FromAxisAngle(new Vector3(0, 0, 0), 1.0f);   //Quat identidad

            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), -theta * DEG2RAD);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), phi * DEG2RAD);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
        }

        public Vector3 position
        {
            get
            {
                //Matris de Transformacion del Espacio del Ojo al espacio del Mundo
                Matrix4 viewToWorld = viewMatrix.Inverted();
                //Posicion de la camara en el espacio del ojo (Osea el origen)
                Vector4 eyePos = new Vector4(0, 0, 0, 1);
                //Transformo el origen de la camara en espacio del ojo al espacio del mundo
                return new Vector3(
                  Vector4.Dot(viewToWorld.Column0, eyePos),
                  Vector4.Dot(viewToWorld.Column1, eyePos),
                  Vector4.Dot(viewToWorld.Column2, eyePos)
                );
            }
        }
    
        /// <summary>
        /// Retorna la Matriz de Projeccion que esta utilizando esta camara.
        /// </summary>
        /// <returns></returns>
        public Matrix4 projectionMatrix
        {
            get { return projMatrix; }
            set { projMatrix = value; }
        }
        /// <summary>
        /// Retorna la Matriz de Vista que representa esta camara.
        /// </summary>
        /// <returns></returns>
        public Matrix4 viewMatrix
        {   
            get
            {
                //Construimos la matriz y la devolvemos.
                Matrix4 posicion = Matrix4.CreateTranslation(cameraPos);
                Matrix4 rotacion = Matrix4.CreateFromQuaternion(cameraRot);
                //return Matrix4.LookAt(eye, target, up);
                return Matrix4.Mult(rotacion, posicion);
            }
        }

        public float aspect
        {
            get { return _aspect; }
            set
            {
                _aspect = value;
                projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
            }
        }

        public float fieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
            }
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
        }

        public void Abajo()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(1, 0, 0), deltaPhi);
            cameraRot = Quaternion.Multiply(qAux, cameraRot);
        }

        public void Arriba2()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), -deltaPhi);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
        }

        public void Abajo2()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 0, 1), deltaPhi);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
        }

        public void Izquierda()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), -deltaTheta);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
        }

        public void Derecha()
        {
            qAux = Quaternion.FromAxisAngle(new Vector3(0, 1, 0), deltaTheta);
            cameraRot = Quaternion.Multiply(cameraRot, qAux);
        }


    }
}
