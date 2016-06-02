using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Meshes
{
    public class Transform
    {
        Matrix4 transformMatrix;

        public Transform()
        {
            transformMatrix = Matrix4.Identity;
        }

        #region Variables

        /*
        //The rotation as Euler angles in degrees.
        public Vector3 eulerAngles
        {
            get { return Rotation.ToAxisAngle; }
        }
        */

        //The blue axis of the transform in world space.
        public Vector3 forward
        {
            get
            {
                Vector3 fw = Vector3.UnitZ;
                Vector3.TransformVector(fw, Matrix4.CreateFromQuaternion(rotation));
                return fw;
            }
        }

        //The red axis of the transform in world space.
        public Vector3 right
        {
            get
            {
                Vector3 rt = Vector3.UnitX;
                Vector3.TransformVector(rt, Matrix4.CreateFromQuaternion(rotation));
                return rt;
            }
        }

        //The green axis of the transform in world space.
        public Vector3 up
        {
            get
            {
                Vector3 u = Vector3.UnitY;
                Vector3.TransformVector(u, Matrix4.CreateFromQuaternion(rotation));
                return u;
            }
        }

        //Matrix that transforms a point from local space into world space
        public Matrix4 localToWorld
        {
            get { return transformMatrix; }
        }

        //Matrix that transforms a point from world space into local space
        public Matrix4 worldToLocal
        {
            get { return Matrix4.Invert(transformMatrix); }
        }

        //The position of the transform in world space.
        public Vector3 position
        {
            get { return transformMatrix.ExtractTranslation(); }
            set { transformMatrix = Matrix4.CreateTranslation(value); }
        }

        //The rotation of the transform in world space stored as a Quaternion.
        public Quaternion rotation
        {
            get { return transformMatrix.ExtractRotation(); }
            set { transformMatrix = Matrix4.CreateFromQuaternion(value); }
        }

        //The scale of the transform.
        public Vector3 scale
        {
            get { return transformMatrix.ExtractScale(); }
            set { transformMatrix = Matrix4.CreateScale(value); }
        }

        #endregion

        #region Public Functions
        //Rotates the transform so the forward vector points at /target/'s current position.
        public void LookAt(Vector3 target)
        {
            //Vector de la posicion de este Transform al target
            Vector3 direccion = target - position;
            //El eje de rotacion perpendicular al plano de Up local y la direccion
            Vector3 axis = Vector3.Cross(up, direccion);
            //El angulo que hay que rotar
            float angle = Vector3.CalculateAngle(up, direccion);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(axis, angle));
            //Aplico la rotacion al objeto y luego la transformacion que tenia
            transformMatrix = Matrix4.Mult(rotation, transformMatrix);
        }

        //Moves the transform in the direction and distance of translation (world space).
        public void Translate(Vector3 translation)
        {
            transformMatrix = Matrix4.Mult(transformMatrix, Matrix4.CreateTranslation(translation));
        }

        //Applies a rotation (local) around the z axis, around the x axis and around the y axis (in that order).
        public void Rotate(Vector3 eulerAngles)
        {
            Matrix4 localRotation = Matrix4.CreateRotationZ(eulerAngles.Z);
            localRotation = Matrix4.Mult(localRotation, Matrix4.CreateRotationX(eulerAngles.X));
            localRotation = Matrix4.Mult(localRotation, Matrix4.CreateRotationY(eulerAngles.Y));
            //Primero aplico las rotaciones luego el resto de la transformacion
            transformMatrix = Matrix4.Mult(transformMatrix, localRotation);
        }
        //Rotates the transform about axis passing through point in world coordinates by angle degrees.
        public void RotateAround(Vector3 point, Vector3 axis, float angle)
        {
            Matrix4 t1 = Matrix4.CreateTranslation(-point);
            Matrix4 t2 = Matrix4.CreateTranslation(point);
            Matrix4 rot = Matrix4.CreateFromQuaternion(Quaternion.FromAxisAngle(axis, angle));            
            transformMatrix = transformMatrix * t1 * rot * t2;
        }
        #endregion

    }
}
