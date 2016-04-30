using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using CGUNS.Shaders;
using System.Linq;
using CGUNS.Parsers;

namespace CGUNS.Meshes {
  public class Transform {
    Matrix4 transformMatrix;

    public Transform()
    {
      transformMatrix = Matrix4.Identity;
    }

    //Matrix that transforms a point from local space into world space
    public Matrix4 LocalToWorld
    {
        get { return transformMatrix; }
    }

    //Matrix that transforms a point from world space into local space
    public Matrix4 WorldToLocal
    {
      get { return Matrix4.Invert(transformMatrix); }
    }

    //The position of the transform in world space.
    public Vector3 Position
    {
      get { return transformMatrix.ExtractTranslation(); }
      set { transformMatrix = Matrix4.CreateTranslation(value); }
    }

    //The rotation of the transform in world space stored as a Quaternion.
    public Quaternion Rotation
    {
      get { return transformMatrix.ExtractRotation(); }
      set { transformMatrix = Matrix4.CreateFromQuaternion(value); }
    }

    //The scale of the transform.
    public Vector3 Scale
    {
      get { return transformMatrix.ExtractScale(); }
      set { transformMatrix = Matrix4.CreateScale(value); }
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
      transformMatrix = Matrix4.Mult(localRotation, transformMatrix);
    }

  }
}
