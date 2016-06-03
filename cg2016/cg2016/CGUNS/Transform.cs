using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Meshes
{
    /// <summary>
    /// Position, rotation and scale of an object. Based on UnityEngine's Transform Class.
    /// </summary>
    public class Transform
    {
        Matrix4 modelMatrix;

        public Transform()
        {
            modelMatrix = Matrix4.Identity;
        }

        #region Variables

        /*
        //The rotation as Euler angles in degrees.
        public Vector3 eulerAngles
        {
            get { return Rotation.ToAxisAngle; }
        }
        */

        /// <summary>
        /// The blue axis of the transform in world space.
        /// </summary>
        public Vector3 forward
        {
            get { return Vector3.TransformVector(Vector3.UnitZ, Matrix4.CreateFromQuaternion(rotation)); }
        }

        /// <summary>
        /// The red axis of the transform in world space.
        /// </summary>
        public Vector3 left
        {
            get { return Vector3.TransformVector(Vector3.UnitX, Matrix4.CreateFromQuaternion(rotation)); }
        }

        /// <summary>
        /// The green axis of the transform in world space.
        /// </summary>
        public Vector3 up
        {
            get { return Vector3.TransformVector(Vector3.UnitY, Matrix4.CreateFromQuaternion(rotation)); }
        }

        /// <summary>
        /// Matrix that transforms a point from local space into world space
        /// </summary>
        public Matrix4 localToWorld
        {
            get { return modelMatrix; }
        }

        /// <summary>
        /// Matrix that transforms a point from world space into local space
        /// </summary>
        public Matrix4 worldToLocal
        {
            get { return Matrix4.Invert(modelMatrix); }
        }

        /// <summary>
        /// The position of the transform in world space.
        /// </summary>
        public Vector3 position
        {
            get { return modelMatrix.ExtractTranslation(); }
            set { modelMatrix = modelMatrix.ClearTranslation() * Matrix4.CreateTranslation(value); }
        }

        /// <summary>
        /// The rotation of the transform in world space stored as a Quaternion.
        /// </summary>
        public Quaternion rotation
        {
            get { return modelMatrix.ExtractRotation(); }
            //Hago la rotacion localmente, primero aplicando la rotacion y luego las transformaciones existentes.
            set { modelMatrix = Matrix4.CreateFromQuaternion(value) * modelMatrix.ClearRotation(); }
        }

        /// <summary>
        /// The scale of the transform.
        /// </summary>
        public Vector3 scale
        {
            get { return modelMatrix.ExtractScale(); }
            //Hago el escalado localmente, primero aplicando el escalado y luego las transformaciones existentes.
            set { modelMatrix = Matrix4.CreateScale(value) * modelMatrix.ClearScale(); }
        }

        #endregion

        #region Public Functions
        /// <summary>
        /// Rotates the transform so the forward vector points at /target/'s current position.
        /// </summary>
        /// <param name="target"></param>
        public void LookAt(Vector3 target)
        {
            //Direccion hacia arriba en el mundo
            Vector3 worldUp = Vector3.UnitY;
            //Vector direccion de este Transform al target
            Vector3 direccion = target - position;
            //El eje de rotacion perpendicular al plano de WorldUp y la direccion
            Vector3 axis = Vector3.Cross(worldUp, direccion);
            //El angulo que hay que rotar
            float angle = Vector3.CalculateAngle(worldUp, direccion);
            //Construyo la matris de rotacion para mirar al target
            Matrix4 lookRotation = Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(axis, angle));
            //Aplico la rotacion al objeto y luego la transformacion existente
            modelMatrix = lookRotation * modelMatrix.ClearRotation();
        }

        /// <summary>
        /// Moves the transform in the direction and distance of translation (world space).
        /// </summary>
        /// <param name="translation"></param>
        public void Translate(Vector3 translation)
        {
            modelMatrix = modelMatrix * Matrix4.CreateTranslation(translation);
        }

        /// <summary>
        /// Applies a rotation (local) around the z axis, around the x axis and around the y axis (in that order).
        /// </summary>
        /// <param name="eulerAngles"></param>
        public void Rotate(Vector3 eulerAngles)
        {
            Matrix4 Rz = Matrix4.CreateRotationZ(eulerAngles.Z);
            Matrix4 Rx = Matrix4.CreateRotationX(eulerAngles.X);
            Matrix4 Ry = Matrix4.CreateRotationY(eulerAngles.Y);
            //Primero aplico las rotaciones luego las transformaciones existentes
            modelMatrix = Rz * Rx * Ry * modelMatrix;
        }
        /// <summary>
        /// Rotates the transform about axis passing through point in world coordinates by angle degrees.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            Matrix4 t1 = Matrix4.CreateTranslation(-point);
            Matrix4 rot = Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(axis, angle));
            Matrix4 t2 = Matrix4.CreateTranslation(point);
            modelMatrix = modelMatrix * t1 * rot * t2;
        }

        /// <summary>
        /// Transforms position from local space to world space. The returned position is affected by scale.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Vector3 TransformPoint(Vector3 position)
        {
            return Vector3.TransformPosition(position, modelMatrix);
        }

        /// <summary>
        /// Transforms vector from local space to world space. This operation is not affected by position of the transform, but it is affected by scale.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public Vector3 TransformVector(Vector3 vector)
        {
            return Vector3.TransformVector(vector, modelMatrix.ClearTranslation());
        }

        /// <summary>
        /// Transforms direction from local space to world space. This operation is not affected by scale or position of the transform.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Vector3 TransformDirection(Vector3 direction)
        {
            return Vector3.TransformVector(direction, modelMatrix.ClearScale().ClearTranslation());
        }
        #endregion

    }
}
