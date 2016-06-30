using System;
using OpenTK;

namespace CGUNS.Cameras
{
    public abstract class Camera
    {
        private const float DEG2RAD = (float)(Math.PI / 180.0); //Para pasar de grados a radianes

        private Matrix4 projMatrix; //Matriz de Proyeccion.

        //Configuracion de la matriz de proyeccion
        private float _fieldOfView;
        private float _aspect;
        private float _nearClipPlane;
        private float _farClipPlane;

        public Camera(float zNear = 0.1f, float zFar = 250f, float fovy = 50 * DEG2RAD, float aspectRatio = 1)
        {
            //Matriz
            _fieldOfView = fovy;
            _aspect = aspectRatio;
            _nearClipPlane = zNear;
            _farClipPlane = zFar;

            projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
        }

        public abstract Vector3 Position();

        public abstract void setPosition(Vector3 pos);
        /// <summary>
        /// Retorna la Matriz de Projeccion que esta utilizando esta camara.
        /// </summary>
        /// <returns></returns>
        public Matrix4 ProjectionMatrix()
        {
            return projMatrix;
        }

        /// <summary>
        /// Retorna la Matriz de Vista que representa esta camara.
        /// </summary>
        /// <returns></returns>
        public abstract Matrix4 ViewMatrix();

        public float Aspect
        {
            get { return _aspect; }
            set
            {
                _aspect = value;
                projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
            }
        }

        public float FieldOfView
        {
            get { return _fieldOfView; }
            set
            {
                _fieldOfView = value;
                projMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfView, _aspect, _nearClipPlane, _farClipPlane);
            }
        }

        public abstract void Acercar();

        public abstract void Alejar();

        public abstract void Arriba();

        public abstract void Abajo();

        public abstract void Izquierda();

        public abstract void Derecha();

        public abstract void GirarIzquierda();

        public abstract void GirarDerecha();
    }
}
