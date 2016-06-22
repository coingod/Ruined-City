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
        private SequentialImpulseConstraintSolver solver;
        private DynamicsWorld dynamicsWor;
        public RigidBody tank; //para modificar desde mainwindow
        public RigidBody map;  //para modificar desde mainwindow
        ConvexHullShape tankShape;
        ConvexHullShape mapShape;
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
            dispatcher = new CollisionDispatcher(collisionConfiguration);
            solver = new SequentialImpulseConstraintSolver();
            //mundo
            dynamicsWor = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfiguration);
            dynamicsWor.Gravity = new Vector3(0, -10, 0);
            //aca se realizan las acciones
            tankShape = new ConvexHullShape();
            DefaultMotionState myMotionState = new DefaultMotionState(Matrix4.CreateTranslation(0, 3, 0));
            Vector3 localInertia2 = tankShape.CalculateLocalInertia(10f);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(10f, myMotionState, tankShape, localInertia2);

            tank = new RigidBody(rbInfo);
            dynamicsWor.AddRigidBody(tank);

            myMotionState = new DefaultMotionState(Matrix4.CreateTranslation(0, 0, 0));
            mapShape = new ConvexHullShape();
            rbInfo = new RigidBodyConstructionInfo(0f, myMotionState, mapShape, new Vector3(0, 0, 0));

            map = new RigidBody(rbInfo);
            dynamicsWor.AddRigidBody(map);


        }
        
        public void addMeshMap( List<Vector3> lista){
            int i = 0;
            for (i = 0; i < lista.Count; i++) {
                mapShape.AddPoint(lista[i]);                
            }
        }

        public void addMeshTank(List<Vector3> lista) {
            int i = 0;
            for (i = 0; i < lista.Count; i++)
            {
                tankShape.AddPoint(lista[i]);
            }
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
