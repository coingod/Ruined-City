using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK; //La matematica

using BulletSharp;

namespace cg2016
{
   
    class fisica
    {
        private BroadphaseInterface broadphase;
        private DefaultCollisionConfiguration collisionConfiguration;
        private CollisionDispatcher dispatcher;
        private ConstraintSolver solver;
        private DynamicsWorld dynamicsWor;
        public RigidBody tank; //para modificar desde mainwindow
        public RigidBody map;  //para modificar desde mainwindow
        public RigidBody FPSCamera;
        CollisionShape tankShape;
        CollisionShape mapShape;
        public DynamicsWorld dynamicsWorld{
            get{return dynamicsWor;}
            set{dynamicsWor = value;}
        }

        //CONSTRUCTOR

        public fisica() {
            inicializarMundo();
        }

        private void inicializarMundo()
        {
            broadphase = new DbvtBroadphase();
            collisionConfiguration = new DefaultCollisionConfiguration();
            ConvexPenetrationDepthSolver asd = new MinkowskiPenetrationDepthSolver();
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            solver = new SequentialImpulseConstraintSolver();
            //mundo
            dynamicsWor = new DiscreteDynamicsWorld(dispatcher, broadphase, null, collisionConfiguration);
            dynamicsWor.Gravity = new Vector3(0, -10, 0);
            
        }

        public void addFPSCamera(Vector3 translation) {
            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateTranslation(translation));
            CollisionShape cameraShape = new BoxShape(0.1f);
            Vector3 inertia = cameraShape.CalculateLocalInertia(100f);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(100f, myMotionState, cameraShape, inertia);
            FPSCamera = new RigidBody(rbInfo);
            FPSCamera.Friction = 1;
            FPSCamera.RollingFriction = 1;
            dynamicsWor.AddRigidBody(FPSCamera);
        }

        public void addMeshMap( List<Vector3> listaVertices, List<int> listaIndices){
            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateTranslation(0, 0, 0));
            TriangleMesh aux = new TriangleMesh();            
            int i = 0;
            for (i = 0; i < listaIndices.Count; i=i+3) {
                aux.AddTriangle(listaVertices[listaIndices[i]], listaVertices[listaIndices[i+1]], listaVertices[listaIndices[i+2]]);
            }
            mapShape = new BvhTriangleMeshShape(aux, true);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0f, myMotionState, mapShape);
            map = new RigidBody(rbInfo);
            map.Friction = 1;
            map.RollingFriction = 1;
            dynamicsWor.AddRigidBody(map);
        }

        public void addMesh(List<Vector3> listaVertices, List<int> listaIndices) {
            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateTranslation(0, 0, 0));
            TriangleMesh aux = new TriangleMesh();
            int i = 0;
            for (i = 0; i < listaIndices.Count; i = i + 3)
            {
                aux.AddTriangle(listaVertices[listaIndices[i]], listaVertices[listaIndices[i + 1]], listaVertices[listaIndices[i + 2]]);
            }
            CollisionShape meshShape;
            meshShape = new BvhTriangleMeshShape(aux, true);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0f, myMotionState, meshShape);
            RigidBody meshRB = new RigidBody(rbInfo);
            dynamicsWor.AddRigidBody(meshRB);
        }

        public void addMeshTank(List<Vector3> listaVertices, List<int> listaIndices) {
            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateTranslation(0, 0.1f, 0));
            TriangleMesh aux = new TriangleMesh();
            int i = 0;
            for (i = 0; i < listaIndices.Count; i = i + 3)
            {
               aux.AddTriangle(listaVertices[listaIndices[i]], listaVertices[listaIndices[i + 1]], listaVertices[listaIndices[i + 2]]);
            }
            tankShape = new ConvexTriangleMeshShape(aux, true);
            Vector3 localInertia = tankShape.CalculateLocalInertia(60000f);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(60000f, myMotionState, tankShape, localInertia);
            tank = new RigidBody(rbInfo);
            tank.Friction = 1; //El tanque tracciona, no se mueve si no se le indica.
            tank.RollingFriction = 1;
            tank.ForceActivationState(ActivationState.DisableDeactivation); //Siempre activo!
            tank.Restitution = 1;
            dynamicsWor.AddRigidBody(tank);
          }

        void myTickCallback()
        {
            int numManifolds = dynamicsWor.Dispatcher.NumManifolds;

        }


        void eliminar()
        {
            //aca se elimina
            int i;
            for (i = dynamicsWor.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = dynamicsWor.GetConstraint(i);
                dynamicsWor.RemoveConstraint(constraint);
                constraint.Dispose();
            }
            for (i = dynamicsWor.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = dynamicsWor.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                dynamicsWor.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            dynamicsWor.Dispose();
            broadphase.Dispose();
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
            collisionConfiguration.Dispose();

        }
        public RigidBody LocalCreateRigidBody(float mass, Matrix4 startTransform, CollisionShape shape)
        {
            bool isDynamic = (mass != 0.0f);

            Vector3 localInertia = Vector3.Zero;
            if (isDynamic)
                shape.CalculateLocalInertia(mass, out localInertia);

            DefaultMotionState myMotionState = new DefaultMotionState(startTransform);

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);
            RigidBody body = new RigidBody(rbInfo);

            dynamicsWorld.AddRigidBody(body);

            return body;
        }
        public Matrix4 getMatrixModelMap() {
            return map.MotionState.WorldTransform;
        }

        public Matrix4 getMatrixModelTank() {
            return tank.MotionState.WorldTransform;
        }
                        
    }


}
