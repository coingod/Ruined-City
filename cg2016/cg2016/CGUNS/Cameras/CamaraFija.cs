using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace CGUNS.Cameras
{

    /// <summary>
    /// Representa una Camara fija. En el constructor se le indica la posicion y target y luego no pueden modificarse
    /// </summary>
    class CamaraFija: Camera
    {
        private const float DEG2RAD = (float)(Math.PI / 180.0); //Para pasar de grados a radianes
        private Matrix4 projMatrix; //Matriz de Proyeccion.

        //Valores necesarios para calcular la Matriz de Vista.
        private Vector3 eye = new Vector3(0.0f, 0.0f, 0.0f);
        private Vector3 target = new Vector3(0, 0, 0);
        private Vector3 up = Vector3.UnitY;


        
        /// <summary>
        /// Constructor con la posicion. Target se asume en el origen y up como el versor en Y
        /// </summary>
        /// <returns></returns>
        public CamaraFija(Vector3 posicion)
        {
            CrearProjMatrix();
            eye = posicion;

        }

        /// <summary>
        /// Constructor con la posicion y target. El vector up se asume como el versor en Y
        /// </summary>
        /// <returns></returns>
        public CamaraFija(Vector3 posicion, Vector3 objetivo)
        {
            CrearProjMatrix();
            eye = posicion;
            target = objetivo;

        }

        
        /// <summary>
        /// Constructor con la posicion y target. El vector up se asume como el versor en Y
        /// </summary>
        /// <returns></returns>
        public CamaraFija(Vector3 posicion, Vector3 objetivo, Vector3 Up)
        {
            CrearProjMatrix();
            eye = posicion;
            target = objetivo;
            up = Up;

        }

        private void CrearProjMatrix() {

            float fovy = 50 * DEG2RAD; //50 grados de angulo.
            float aspectRadio = 1; //Cuadrado
            float zNear = 0.1f; //Plano Near
            float zFar = 100f;  //Plano Far
            projMatrix = Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRadio, zNear, zFar);

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
        public override Matrix4 ViewMatrix()
        {
            //Construimos la matriz y la devolvemos.
            return Matrix4.LookAt(eye, target, up);
        }

        public override void setPosition(Vector3 pos)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Retorna la Posicion de la camara.
        /// </summary>
        /// <returns></returns>
        public override Vector3 Position()
        {
            return eye;
        }


        //Los metodos de movimiento no se van a usar. Solo se agregan para respetar la interfaz Camera y facilitar el manejo de camaras en MainGameWindows, a traves de una sola variable
        public override void Acercar()
        {

        }

        public override void Alejar()
        {

        }

        public override void Arriba()
        {

        }

        public override void Abajo()
        {

        }

        public override void Izquierda()
        {

        }

        public override void Derecha()
        {

        }

        public override void GirarIzquierda()
        {
            throw new NotImplementedException("No tiene sentido girar izquierda con esta camara, es para mouse");
        }

        public override void GirarDerecha()
        {
            throw new NotImplementedException("No tiene sentido girar derecha con esta camara, es para mouse");
        }

        public override void MouseCoords(float x, float y)
        {

        }

    }
}
