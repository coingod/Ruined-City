using System;
using System.Drawing;
using OpenTK; //La matematica
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using CGUNS;
using CGUNS.Shaders;
using CGUNS.Cameras;
using CGUNS.Primitives;
using CGUNS.Meshes;
using CGUNS.Meshes.FaceVertexList;
using System.Drawing.Imaging;
using CGUNS.Particles;
using IrrKlang;
using BulletSharp;
using System.Collections.Generic;

namespace cg2016
{
    public class MainGameWindow : GameWindow
    {
        #region Variables de clase
        //Camaras
        private Camera myCamera;
        private List<Camera> camaras;
        private Rectangle viewport; //Viewport a utilizar.
        private List<CamaraFija> camarasFijas;
        bool freeOn;
        int indiceFija=0; //

        //Shaders
        private ShaderProgram sProgram; //Nuestro programa de shaders.
        private ShaderProgram sProgramAnimated;
        private ShaderProgram sProgramUnlit; //Nuestro programa de shaders.
        private ShaderProgram sProgramParticles; //Nuestro programa de shaders.
        private ShaderProgram sProgramTerrain; //Nuestro programa de shaders.
        private ShaderProgram mSkyBoxProgram;

        //Modelos
        private Ejes ejes_globales; // Ejes de referencia globales
        private Ejes ejes_locales; // Ejes de referencia locales al objeto
        private ObjetoGrafico tanque; //Nuestro objeto a dibujar.
        private ObjetoGrafico tanque_col; //Nuestro objeto a dibujar.
        private ObjetoGrafico orugas;
        private ObjetoGrafico mapa; //Nuestro objeto a dibujar.
        private ObjetoGrafico mapa_col;

        //Iluminacion
        private Light[] luces;

        //Materiales
        private Material[] materiales = new Material[] { Material.Default, Material.WhiteRubber, Material.Obsidian, Material.Bronze, Material.Gold, Material.Jade, Material.Brass };
        private Material material;
        private int materialIndex = 0;

        //Efectos de Particulas
        //private ParticleEmitter particles;
        private Smoke smokeParticles;
        private Fire fireParticles;

        private Cube cubo;
        Explosiones explosiones;
        private Aviones aviones;

        //BulletSharp
        private fisica fisica;
        private int tanksleeping = 0;
        private int tankDirection = 0;

        //Irrklang. Para audio
        ISoundEngine engine;
        ISound sonidoAmbiente;
        ISound sonidoTanque;

        //Opciones
        private bool toggleNormals = false;
        private bool toggleWires = false;
        private bool drawGizmos = true;
        private bool toggleParticles = true;
        private bool toggleFullScreen = false;

        //Debug y helpers
        private int loaded = 0;
        private double timeSinceStartup = 0; //Tiempo total desde el inicio del programa (En segundos)
        private int fps = 0; //FramesPorSegundo
        int FrameCount = 0;
        DateTime NextFPSUpdate = DateTime.Now.AddSeconds(1);

        //Pressed/released para movimientos suaves.
        bool[] keys = new bool[1024];

        //Skybox
        private Skybox mSkyBox;
        private int mSkyBoxTextureUnit = 12;
        private int mSkyboxTextureId;

        //Sombras
        private bool showShadowMap;
        private int fbo;
        private int depthTexture;        
        private bool shadowsOn = false;
        private Rectangle mShadowViewport; //Viewport a utilizar para el shadow mapping.
        private ShaderProgram mShadowProgram; //Nuestro programa de shaders.   
        private int mShadowTextureUnit = 17;

        private ShaderProgram mShadowViewportProgram; //Nuestro programa de shaders.
        private ViewportQuad mShadowViewportQuad;

        #endregion

        #region Funciones de GameWindow (por eventos)
        /// <summary>
        /// Game window initialisation
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            logContextInfo(); //Mostramos info de contexto.

            //Configuracion de Audio
            engine = new ISoundEngine();
            sonidoAmbiente = engine.Play2D("files/audio/ambience.ogg", true);
            sonidoAmbiente.Volume = 0.5f;

            sonidoTanque = engine.Play2D("files/audio/bell.wav", false);
            sonidoTanque.Stop();

            loaded += 10;

            //Creamos los shaders y el programa de shader
            SetupShaders("vunlit.glsl", "funlit.glsl", out sProgramUnlit);
            SetupShaders("vbumpedspecularphong.glsl", "fbumpedspecularphong.glsl", out sProgram);
            //SetupShaders("vmultiplesluces.glsl", "fmultiplesluces.glsl", out sProgram);
            SetupShaders("vanimated.glsl", "fanimated.glsl", out sProgramAnimated);
            SetupShaders("vterrain.glsl", "fterrain.glsl", out sProgramTerrain);
            SetupShaders("vparticles.glsl", "fparticles.glsl", out sProgramParticles);
            SetupShaders("vSkyBox.glsl", "fSkyBox.glsl", out mSkyBoxProgram);
            SetupShaders("vShadow.glsl", "fShadow.glsl", out mShadowProgram);
            SetupShaders("vViewport.glsl", "fViewport.glsl", out mShadowViewportProgram);

            CrearShadowTextures();

            loaded += 50;

            //Configuracion de los sistemas de particulas
            SetupParticles();
            cubo = new Cube(0.1f, 0.1f, 0.1f);
            cubo.Build(sProgramUnlit);
            aviones = new Aviones(sProgram, mShadowProgram, engine, sProgramUnlit);
            explosiones = new Explosiones(5);

            loaded += 39;

            //Carga y configuracion de Objetos
            SetupObjects();

            //Arrancamos la clase fisica
            fisica = new fisica();
            //Meshes Convex Fisica 
            fisica.addMeshMap(mapa.getMeshVertices("Ground_Plane"), mapa.getIndicesDeMesh("Ground_Plane"));
            
            for (int i=0; i<mapa.Meshes.Count; i++)
                if (mapa.Meshes[i].Name!="Ground_Plane")
                    fisica.addMesh(mapa.getMeshVertices(i), mapa.getIndicesDeMesh(i), 0);

            fisica.addMeshTank(tanque_col.getMeshVertices(0), tanque_col.getIndicesDeMesh(0));

            //Configuracion de la Camara
            camaras = new List<Camera>();
            camaras.Add(new QSphericalCamera(5, 45, 30, 0.01f, 250));
            camaras.Add(new FreeCamera(camaras[0].Position(), new Vector3(0, 0, 0), 0.025f));
            camaras.Add(new QSphericalCamera(5, 45, 30, 0.1f, 250));
            camaras.Add(new FreeCamera(new Vector3(-5, 5, 0), new Vector3(-20, 0, 0), 0.025f));
            myCamera = camaras[0]; //Creo una camara.


            CrearCamarasFijas();

            gl.ClearColor(Color.Black); //Configuro el Color de borrado.

            // Setup OpenGL capabilities
            gl.Enable(EnableCap.DepthTest);
            //gl.Enable(EnableCap.CullFace);

            SetupEjes();

            //Configuracion de las Luces
            SetupLights();

            //Configuracion de Materiales
            material = materiales[materialIndex];

            loaded += 1;
        }


        void CrearCamarasFijas()
        {
            camarasFijas = new List<CamaraFija>();

            //Camara en la ventana del edificio de la calle del tanque
            Vector3 pos = new Vector3(6.5f, 1, 0.5f);
            camarasFijas.Add(new CamaraFija(pos));

            //Camara en una de las esquinas
            pos = new Vector3(2.9f, 1, 5.3f);
            camarasFijas.Add(new CamaraFija(pos));

            //en la esquina de al lado a la anterior
            pos = new Vector3(-3.7f, 1.3f, 7f);
            Vector3 tar = new Vector3(1f, 0f, 0);
            camarasFijas.Add(new CamaraFija(pos, tar));


            //
            //desde el edificio de la otra esquina
            pos = new Vector3(-4.62f, 2f, -6.65f);
            camarasFijas.Add(new CamaraFija(pos));

            //Cupula del edificio
            pos = new Vector3(0f, 2.15f, -11.6f);
            camarasFijas.Add(new CamaraFija(pos));

            //en la puerta del edificio, dando a la calle que tiene en frente
            pos = new Vector3(3.5f, 0.38f, -3.75f);
            tar = new Vector3(0f,0f, -3.75f);
            camarasFijas.Add(new CamaraFija(pos,tar));

            //Desde el piso, cerca del origen, mirando hacia los aviones que llegan
            pos = new Vector3(-0.5f, 0.2f, 0f);
            tar = new Vector3(-5f, 1.8f, 0);
            camarasFijas.Add(new CamaraFija(pos, tar));


        }


        //El THREAD de la pantalla de carga consulta aca cuanto se ha cargado.
        public int UpdateLoadScreen()
        {
            Console.WriteLine("##########     LOADING: " + loaded);

            //Aqui dibujaria la pantalla de carga en esta ventana
            //El thread de carga solo se encargaria de llamar a esta funcion.

            //Actualizo el viewport
            SwapBuffers();

            return loaded;
        }

        /// <summary>
        /// Game window sizing and 3D projection setup
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            int w = Width;
            int h = Height;
            float aspect = 1;

            // Calculate aspect ratio, checking for divide by zero
            if (h > 0)
            {
                aspect = (float)w / (float)h;
            }
            //Configuro la camara principal para este aspect ratio.
            myCamera.Aspect = aspect;
            
            //Configuro el tamaño del viewport
            viewport.X = 0;
            viewport.Y = 0;
            viewport.Width = w;
            viewport.Height = h;
        }
        /// <summary>
        /// Game window update loop
        /// </summary>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            //Incremento el tiempo transcurrido
            timeSinceStartup += this.RenderTime;
            //Console.WriteLine(timeSinceStartup);

            MoverCamara();

            //Simular la fisica
            fisica.dynamicsWorld.StepSimulation(10f);

            //para que el giro sea más manejable, sería un efecto de rozamiento con el aire.
            fisica.tank.AngularVelocity = fisica.tank.AngularVelocity / 10;
            
            //Animacion de una luz
            float blend = ((float)Math.Sin(timeSinceStartup / 2) + 1) / 2;
            //float blend = (float)timeSinceStartup % 1 ;
            Vector3 pos = Vector3.Lerp(new Vector3(-4f, 1f, 0.0f), new Vector3(4f, 1f, 0.0f), blend);
            luces[0].Position = new Vector4(pos, 1.0f);

            //Actualizo los sistemas de particulas
            smokeParticles.Update();
            fireParticles.Update();
            aviones.Actualizar(timeSinceStartup, sProgramParticles);
            explosiones.Actualizar(timeSinceStartup);

            //Actualizo el audio
            Vector3 tankPos = tanque.transform.position;
            sonidoTanque.Position = new Vector3D(tankPos.X, tankPos.Y, tankPos.Z);

            //Actualizamos la informacion de debugeo
            updateDebugInfo();
        }
        /// <summary>
        /// Draw a single 3D frame
        /// </summary>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            // Calculo la matrix MVP desde el punto de vista de la luz.
            // Ojo con el up, debe estar mirando correctamente al target.
            Vector3 lightEye = -luces[1].Position.Xyz;
            //Console.WriteLine();
            //Vector3 lightEye = new Vector3(7.0f, 10f, 7.0f);
            Vector3 lightUp = new Vector3(0, 1, 0);
            Vector3 lightTarget = new Vector3(0, 0, 0);

            // --- VIEW MATRIX ---
            Matrix4 lightViewMatrix = Matrix4.LookAt(lightEye, lightTarget, lightUp);

            // --- PROJECTION MATRIX ---
            // La matrix de proyeccion es una matrix ortografica que abarca toda la escena.
            // Estos valores son seleccionados de forma que toda la escena visible es incluida.
            Matrix4 lightProjMatrix = Matrix4.CreateOrthographicOffCenter(
                -10,
                 10,
                -10,
                 10,
                 1f,
                 10f);

            // --- VIEW PROJECTION ---
            // La matrix de modelado es la identidad.
            Matrix4 lightSpaceMatrix = lightViewMatrix * lightProjMatrix;

            // --- RENDER ---
            // 1. Se renderiza la escena desde el punto de vista de la luz
            if (shadowsOn)
                GenerarShadowMap(lightSpaceMatrix);          

            // Clear the screen
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (toggleWires)
                gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else
                gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            if (toggleFullScreen)
            {
                WindowBorder = WindowBorder.Hidden;
                WindowState = WindowState.Fullscreen;
            }
            else
            {
                WindowBorder = WindowBorder.Resizable;
                WindowState = WindowState.Maximized;//WindowState.Normal;
            }

            gl.Viewport(viewport); //Especificamos en que parte del glControl queremos dibujar.            

            DibujarSkyBox();
            
            //audio
            Vector3D posOyente = new Vector3D(myCamera.Position().X, myCamera.Position().Y, myCamera.Position().Z);
            engine.SetListenerPosition(posOyente, new Vector3D(0, 0, 0));

            DibujarEscena(lightSpaceMatrix, toggleNormals);

            DibujarParticles();                                   

            if (drawGizmos)
                DibujarGizmos();

            if (shadowsOn & showShadowMap)            
                DibujarShadowMap();            

            //Actualizamos la informacion de debugeo
            updateDebugInfo();

            // Display the new frame
            SwapBuffers(); //Intercambiamos buffers frontal y trasero, para evitar flickering
        }        

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Exit();
            }
            if (e.Control && (int)e.Key >= (int)Key.Number0 && (int)e.Key <= (int)Key.Number9)
            {
                ModificarCono(-2.0f, (int)e.Key - (int)Key.Number0);
            }
            else
            if (e.Shift && (int)e.Key >= (int)Key.Number0 && (int)e.Key <= (int)Key.Number9)
            {
                ModificarCono(2.0f, (int)e.Key - (int)Key.Number0);
            }
            else
            if ((int)e.Key >= (int)Key.Number0 && (int)e.Key <= (int)Key.Number9)
            {
                ToggleLight((int)e.Key - (int)Key.Number0);
            }
            else
            {
                switch (e.Key)
                {
                    case Key.Down:
                        fisica.tank.LinearVelocity = -new Vector3(tanque.transform.forward.X, 0, tanque.transform.forward.Z) * 0.6f;
                        tankDirection = -1;
                        if (!engine.IsCurrentlyPlaying("files/audio/tiger_moving.ogg"))
                            sonidoTanque = engine.Play3D("files/audio/tiger_moving.ogg", tanque.transform.position.X, tanque.transform.position.Y, tanque.transform.position.Z, true);
                        break;
                    case Key.Up:
                        fisica.tank.LinearVelocity = new Vector3(tanque.transform.forward.X, 0, tanque.transform.forward.Z) * 0.6f;
                        tankDirection = 1;
                        if (!engine.IsCurrentlyPlaying("files/audio/tiger_moving.ogg"))
                            sonidoTanque = engine.Play3D("files/audio/tiger_moving.ogg", tanque.transform.position.X, tanque.transform.position.Y, tanque.transform.position.Z, true);
                        break;
                    case Key.Right:
                        fisica.tank.AngularVelocity= (new Vector3(0, -1f, 0)) * 1.5f;
                        if(!engine.IsCurrentlyPlaying("files/audio/tiger_moving.ogg"))
                            sonidoTanque = engine.Play3D("files/audio/tiger_moving.ogg", tanque.transform.position.X, tanque.transform.position.Y, tanque.transform.position.Z, true);
                        break;
                    case Key.Left:
                        fisica.tank.AngularVelocity = (new Vector3(0, 1f, 0)) * 1.5f;
                        if (!engine.IsCurrentlyPlaying("files/audio/tiger_moving.ogg"))
                            sonidoTanque = engine.Play3D("files/audio/tiger_moving.ogg", tanque.transform.position.X, tanque.transform.position.Y, tanque.transform.position.Z, true);
                        break;
                    case Key.S:
                        keys[(int)Key.S] = true;
                        break;
                    case Key.W:
                        keys[(int)Key.W] = true;
                        break;
                    case Key.D:
                        keys[(int)Key.D] = true;
                        break;
                    case Key.A:
                        keys[(int)Key.A] = true;
                        break;
                    case Key.KeypadAdd:
                    case Key.I:
                        myCamera.Acercar();
                        break;
                    case Key.KeypadMinus:
                    case Key.O:
                        myCamera.Alejar();
                        break;
                    //Teclas para activar/desactivar funciones
                    case Key.F1:
                        toggleFullScreen = !toggleFullScreen;
                        break;
                    case Key.F3:
                        toggleWires = !toggleWires;
                        break;
                    case Key.F2:
                        toggleNormals = !toggleNormals;
                        break;
                    case Key.G:
                        drawGizmos = !drawGizmos;
                        break;
                    //CAMBIO DE MATERIAL
                    case Key.Z:
                        materialIndex = (materiales.Length + materialIndex - 1) % materiales.Length;
                        material = materiales[materialIndex];
                        break;
                    case Key.X:
                        materialIndex = (materialIndex + 1) % materiales.Length;
                        material = materiales[materialIndex];
                        break;
                    case Key.P:
                        toggleParticles = !toggleParticles;
                        break;
                    case Key.N:
                         {
                             myCamera = camaras[3];
                             OnResize(null);
                         }
                         break;
                    case Key.M:
                        {
                            myCamera = camaras[2];
                            OnResize(null);
                        }
                        break;

                    case Key.J:
                        {
                            freeOn = false;
                            indiceFija = (indiceFija + 1) % camarasFijas.Count; //la primera vez se muestra la 0 y luego va cambiando
                            myCamera = camarasFijas[indiceFija];                            
                            OnResize(null);
                        }
                        break;
                    case Key.K:
                        {
                            freeOn = false;
                            if (indiceFija > 0)
                                indiceFija = indiceFija - 1;
                            else indiceFija = camarasFijas.Count - 1; //si antes era 0, ahora va a la ultima de la lista                        
                            myCamera = camarasFijas[indiceFija];                          
                            
                            OnResize(null);
                        }
                        break;
                    /*case Key.N:
                        Vector3D pos = musicAmbiente.Position + new Vector3D(0, 1, 0) ;
                        musicAmbiente.Position = pos;
                        break;
                    case Key.M:
                        pos = musicAmbiente.Position + new Vector3D(0, -1, 0);
                        musicAmbiente.Position = pos;
                        break;*/
                    case Key.C:
                        {
                            if (myCamera==camaras[0])
                            {
                                freeOn = true;                                
                                myCamera = camaras[1];
                                OnResize(null);
                            }
                            else
                            {
                                freeOn = false;
                                myCamera = camaras[0];//Inicial.
                                OnResize(null);
                            }                            
                        }
                        break;
                    case Key.V:
                        showShadowMap = !showShadowMap;
                        break;
                    case Key.B:
                        shadowsOn = !shadowsOn;
                        break;
                }
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            CursorVisible = false;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            CursorVisible = true;
        }

        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            tankDirection = 0;
            sonidoTanque.Stop();
            switch (e.Key)
            {
                case Key.S:
                    keys[(int)Key.S] = false;
                    break;
                case Key.W:
                    keys[(int)Key.W] = false;
                    break;
                case Key.D:
                    keys[(int)Key.D] = false;
                    break;
                case Key.A:
                    keys[(int)Key.A] = false;
                    break;
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (freeOn)
            {
                FreeCamera aux = (FreeCamera)myCamera;
                aux.MouseCoords(e.X, e.Y);
            }            
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (freeOn)
            {
                FreeCamera aux = (FreeCamera)myCamera;
                aux.MouseScroll(e.Delta);
            }
        }

        private void MoverCamara()
        {
            if (freeOn)
            {
                if (keys[(int)Key.S]) myCamera.Alejar();
                if (keys[(int)Key.W]) myCamera.Acercar();
                if (keys[(int)Key.D]) myCamera.Derecha();
                if (keys[(int)Key.A]) myCamera.Izquierda();                
            }
            else
            {
                if (keys[(int)Key.S]) myCamera.Abajo();
                if (keys[(int)Key.W]) myCamera.Arriba();
                if (keys[(int)Key.D]) myCamera.Izquierda();
                if (keys[(int)Key.A]) myCamera.Derecha();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs mouse)
        {
            int Xopengl = mouse.X;
            int Yopengl = this.Height - mouse.Y;
            if (viewport.Contains(Xopengl, Yopengl))
            { //Inside viewport?
                int Xviewport = Xopengl - viewport.X;
                int Yviewport = Yopengl - viewport.Y;
                Vector3 ray_wor = getRayFromMouse(Xviewport, Yviewport);

                ISound sound;
                sound = engine.Play3D("files/audio/NearExplosionA.ogg", tanque.transform.position.X, tanque.transform.position.Y, tanque.transform.position.Z, false);
                
                if (sound != null)
                    sound.MaxDistance = 10.0f;
                

               float rToSphere = rayToSphere(myCamera.Position(), ray_wor, explosiones.getCentro(), explosiones.getRadio());
                if (rToSphere != -1.0f)
                {
                    Vector3 origenParticulas = proyeccion(myCamera.Position(), ray_wor * 10);
                    explosiones.CrearExplosion(timeSinceStartup, origenParticulas, sProgramParticles);
                }

            }
        }

        private Vector3 getRayFromMouse(int x_viewport, int y_viewport)
        {
            // mouse_x, mouse_y are on screen space (viewport coordinates)
            float x = (2.0f * x_viewport) / viewport.Width - 1.0f;
            float y = (2.0f * y_viewport) / viewport.Height - 1.0f;
            float z = 1.0f;
            // normalised device space
            Vector3 ray_nds = new Vector3(x, y, z);
            // clip space
            Vector4 ray_clip = new Vector4(ray_nds.X, ray_nds.Y, -1.0f, 0.0f);
            // eye space
            Vector4 ray_eye = Vector4.Transform(ray_clip, Matrix4.Invert(myCamera.ProjectionMatrix())); //inverse(projMat) * ray_clip
            ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);
            // world space
            Vector4 aux = Vector4.Transform(ray_eye, Matrix4.Invert(myCamera.ViewMatrix())); //inverse(viewMat) * ray_eye
            Vector3 ray_wor = new Vector3(aux.X, aux.Y, aux.Z);
            ray_wor = Vector3.Normalize(ray_wor);
            return ray_wor;
        }

        private Vector3 proyeccion(Vector3 rayOrigin, Vector3 rayDir)
        {
            Vector3 pendiente = rayDir - rayOrigin;
            //recta se expresa como origen+ t*pendiente. Calculo para que valor de t va a dar y=0 (q punto esta dentro del plano)
            // 0 = rayOrigin.Y + t * pendiente.Y            
            float t = -rayOrigin.Y / pendiente.Y;
            float x = rayOrigin.X + t * pendiente.X;
            float z = rayOrigin.Z + t * pendiente.Z;


            //return new Vector3(x,0, z);
            return rayOrigin + t * pendiente;
        }

        /*
        check if a ray and a sphere intersect. if not hit, returns -1. it rejects
        intersections behind the ray caster's origin, and sets intersection_distance to
        the closest intersection (ALL PARAMETERS ARE ON WORLD SPACE!) */
        private float rayToSphere(Vector3 rayOrigin, Vector3 rayDir, Vector3 sphereCenter, float sphereRadius)
        {
            // work out components of quadratic
            Vector3 distToSphere = rayOrigin - sphereCenter;
            float b = Vector3.Dot(rayDir, distToSphere);
            float c = Vector3.Dot(distToSphere, distToSphere) - sphereRadius * sphereRadius;
            float b_2_minus_c = b * b - c;
            // check for "imaginary" answer. == ray completely misses sphere
            if (b_2_minus_c < 0.0f)
            {
                return -1;
            }
            // check for ray hitting twice (in and out of the sphere)
            if (b_2_minus_c > 0.0f)
            {
                // get the 2 intersection distances along ray
                float t_a = -b + (float)Math.Sqrt(b_2_minus_c);
                float t_b = -b - (float)Math.Sqrt(b_2_minus_c);
                float result = t_b;
                // if behind viewer, throw one or both away
                if (t_a < 0.0)
                {
                    if (t_b < 0.0)
                    {
                        return -1;
                    }
                }
                else if (t_b < 0.0)
                {
                    result = t_a;
                }
                return result;
            }
            // check for ray hitting once (skimming the surface)
            if (0.0f == b_2_minus_c)
            {
                // if behind viewer, throw away
                float t = -b + (float)Math.Sqrt(b_2_minus_c);
                if (t < 0.0f)
                {
                    return -1;
                }
                return t;
            }
            return -1;
        }

        //OnClosed debe finalizar la simulacion de BulletSharp
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            fisica.dynamicsWorld.Dispose();
        }
        #endregion

        #region Dibujado

        /// <summary>
        /// Dibuja el contenido de la textura utilizada para el shadow map en un viewport en el extremo inferior izquierdo.
        /// </summary>
        private void DibujarShadowMap()
        {
            // --- SETEO EL ESTADO ---
            GL.Disable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Rectangle shadowmap = new Rectangle(0, 0, viewport.Width / 2, viewport.Height / 2);
            GL.Viewport(shadowmap);

            mShadowViewportProgram.Activate();

            Matrix4 projectionMatrix = Matrix4.CreateOrthographicOffCenter(
                shadowmap.Left,
                shadowmap.Right,
                shadowmap.Top,
                shadowmap.Bottom,
                0.0f,
                1.0f);

            Vector2 viewportSize = new Vector2(shadowmap.Width, shadowmap.Height);

            // --- SETEO UNIFORMS ---
            mShadowViewportProgram.SetUniformValue("uViewportOrthographic", projectionMatrix);
            mShadowViewportProgram.SetUniformValue("uViewportSize", viewportSize);
            mShadowViewportProgram.SetUniformValue("uShadowSampler", mShadowTextureUnit);

            // --- DIBUJO ---
            mShadowViewportQuad.Dibujar(mShadowViewportProgram);

            mShadowViewportProgram.Deactivate();
        }

        /// <summary>
        /// Se renderiza la escena desde el punto de vista de la luz.
        /// Almacena en una textura la profundidad de los objetos partiendo desde la posicion de la luz.
        /// </summary>
        /// <param name="lightSpaceMatrix">MVP respecto a la luz.</param>
        private void GenerarShadowMap(Matrix4 lightSpaceMatrix)
        {
            // --- SETEO EL ESTADO ---
            GL.Enable(EnableCap.DepthTest);
            GL.Viewport(mShadowViewport);

            // Limpio el framebuffer el contenido de la pasada anterior.
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            mShadowProgram.Activate();

            // La matriz es uniforme a todos los objetos renderizados.
            mShadowProgram.SetUniformValue("uLightSpaceMatrix", lightSpaceMatrix);

            // --- TANQUE -----            
            tanque.transform.localToWorld = fisica.tank.MotionState.WorldTransform;
            mShadowProgram.SetUniformValue("uModelMatrix", tanque.transform.localToWorld);
            tanque.DibujarShadows(mShadowProgram);

            // --- MAPA ---
            mapa.transform.localToWorld = fisica.map.MotionState.WorldTransform;
            mShadowProgram.SetUniformValue("uModelMatrix", mapa.transform.localToWorld);
            foreach (Mesh m in mapa.Meshes)
                //if (m.Name != "Ground_Plane")
                    m.DibujarShadows(mShadowProgram);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            mShadowProgram.Deactivate();
        }

        private void DibujarSkyBox()
        {
            //gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            // --- SETEO EL ESTADO ---
            // No necesito el depth test.
            GL.Disable(EnableCap.DepthTest);

            GL.BindTexture(TextureTarget.TextureCubeMap, mSkyboxTextureId);

            // TRUCO: Remuevo los componentes de traslacion asi parece un skybox infinito.
            Matrix4 viewMatrix = myCamera.ViewMatrix();
            viewMatrix = viewMatrix.ClearTranslation();
            Matrix4 projMatrix = myCamera.ProjectionMatrix();

            //Activamos el programa de shaders
            mSkyBoxProgram.Activate();

            mSkyBoxProgram.SetUniformValue("projMat", projMatrix);
            mSkyBoxProgram.SetUniformValue("vMat", viewMatrix);
            mSkyBoxProgram.SetUniformValue("uSamplerSkybox", mSkyBoxTextureUnit);
            mSkyBox.Dibujar(sProgram);

            mSkyBoxProgram.Deactivate();

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        private void DibujarEscena(Matrix4 lightSpaceMatrix, bool normals)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            // Si multiplicamos la posicion del vertice por la matrix de MVP de la luz
            // nos da coordenadas homogeneas [-1,1] pero el sampleo de textura debe hacerse 
            // en [0, 1].
            // Para eso usamos la matrix de bias que nos mapea de [-1,1] a [0,1]
            Matrix4 biasMatrix = new Matrix4(
                0.5f, 0.0f, 0.0f, 0.0f,
                0.0f, 0.5f, 0.0f, 0.0f,
                0.0f, 0.0f, 0.5f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f);
            Matrix4 lightBiasMatrix = lightSpaceMatrix * biasMatrix;

            Matrix4 modelMatrix = Matrix4.Identity; //Por ahora usamos la identidad.
            Matrix4 mvMatrix = Matrix4.Mult(myCamera.ViewMatrix(), modelMatrix);
            //Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(mvMatrix)));
            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(modelMatrix)));
            Matrix4 MVP = Matrix4.Mult(mvMatrix, myCamera.ProjectionMatrix());

            //FIRST SHADER (Para dibujar objetos)
            sProgram.Activate(); //Activamos el programa de shaders

            #region Configuracion de Uniforms
             
            /// BUMPED SCPECULAR PHONG
            
            //Configuracion de los valores uniformes del shader
            sProgram.SetUniformValue("projMatrix", myCamera.ProjectionMatrix());
            sProgram.SetUniformValue("modelMatrix", modelMatrix);
            //sProgram.SetUniformValue("normalMatrix", normalMatrix);
            sProgram.SetUniformValue("viewMatrix", myCamera.ViewMatrix());
            //sProgram.SetUniformValue("cameraPosition", myCamera.Position());
            sProgram.SetUniformValue("A", 0.3f);
            sProgram.SetUniformValue("B", 0.007f);
            sProgram.SetUniformValue("C", 0.00008f);
            sProgram.SetUniformValue("material.Ka", material.Kambient);
            sProgram.SetUniformValue("material.Kd", material.Kdiffuse);
            //sProgram.SetUniformValue("material.Ks", material.Kspecular);
            sProgram.SetUniformValue("material.Shininess", material.Shininess);
            sProgram.SetUniformValue("ColorTex", 0);
            sProgram.SetUniformValue("NormalMapTex", 1);
            //sProgram.SetUniformValue("SpecularMapTex", 0);
            
            sProgram.SetUniformValue("numLights", luces.Length);
            for (int i = 0; i < luces.Length; i++)
            {
                sProgram.SetUniformValue("allLights[" + i + "].position", luces[i].Position);
                sProgram.SetUniformValue("allLights[" + i + "].Ia", luces[i].Iambient);
                sProgram.SetUniformValue("allLights[" + i + "].Ip", luces[i].Ipuntual);
                sProgram.SetUniformValue("allLights[" + i + "].coneAngle", luces[i].ConeAngle);
                sProgram.SetUniformValue("allLights[" + i + "].coneDirection", luces[i].ConeDirection);
                sProgram.SetUniformValue("allLights[" + i + "].enabled", luces[i].Enabled);
            }
            /*
            // MULTIPLES LUCES
            //Configuracion de los valores uniformes del shader
            sProgram.SetUniformValue("projMatrix", myCamera.ProjectionMatrix());
            sProgram.SetUniformValue("modelMatrix", modelMatrix);
            sProgram.SetUniformValue("normalMatrix", normalMatrix);
            sProgram.SetUniformValue("viewMatrix", myCamera.ViewMatrix());
            sProgram.SetUniformValue("cameraPosition", myCamera.Position());
            sProgram.SetUniformValue("A", 0.3f);
            sProgram.SetUniformValue("B", 0.007f);
            sProgram.SetUniformValue("C", 0.00008f);
            sProgram.SetUniformValue("material.Ka", material.Kambient);
            sProgram.SetUniformValue("material.Kd", material.Kdiffuse);
            sProgram.SetUniformValue("material.Ks", material.Kspecular);
            sProgram.SetUniformValue("material.Shininess", material.Shininess);
            sProgram.SetUniformValue("ColorTex", 0);

            sProgram.SetUniformValue("numLights", luces.Length);
            for (int i = 0; i < luces.Length; i++)
            {
                sProgram.SetUniformValue("allLights[" + i + "].position", luces[i].Position);
                sProgram.SetUniformValue("allLights[" + i + "].Ia", luces[i].Iambient);
                sProgram.SetUniformValue("allLights[" + i + "].Ip", luces[i].Ipuntual);
                sProgram.SetUniformValue("allLights[" + i + "].coneAngle", luces[i].ConeAngle);
                sProgram.SetUniformValue("allLights[" + i + "].coneDirection", luces[i].ConeDirection);
                sProgram.SetUniformValue("allLights[" + i + "].enabled", luces[i].Enabled);
            }
            */
            //Para sombras
            int iShadowsOn = shadowsOn ? 1 : 0;
            sProgram.SetUniformValue("shadowsOn", iShadowsOn);
            sProgram.SetUniformValue("uLightBiasMatrix", lightBiasMatrix);
            sProgram.SetUniformValue("uShadowSampler", mShadowTextureUnit);
            #endregion


            //Dibujamos el Objeto
            sProgram.SetUniformValue("NormalMapTex", 15);
            tanque.transform.localToWorld = fisica.tank.MotionState.WorldTransform;
            tanque_col.transform.localToWorld = fisica.tank.MotionState.WorldTransform;
            //Cambio la escala de los objetos para evitar el bug de serruchos.
            //objeto.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
            tanque.Dibujar(sProgram);
            sProgram.SetUniformValue("NormalMapTex", 1);

            aviones.Dibujar(sProgram, sProgramParticles, timeSinceStartup);
            //if (toggleNormals) objeto.DibujarNormales(sProgram, viewMatrix);

            if (toggleNormals)
            {
                tanque_col.Dibujar(sProgram);//tanque.DibujarNormales(sProgram);
                mapa_col.Dibujar(sProgram);
            }
            //Dibujamos el Mapa
            mapa.transform.localToWorld = fisica.map.MotionState.WorldTransform;
            //Cambio la escala de los objetos para evitar el bug de serruchos.
            //mapa.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
            //mapa.Dibujar(sProgram);
            foreach (Mesh m in mapa.Meshes)
                if (m.Name != "Ground_Plane")
                    m.Dibujar(sProgram);
            if (toggleNormals) mapa.DibujarNormales(sProgram);
            sProgram.Deactivate(); //Desactivamos el programa de shader.

            //SHADER ANIMADO (Para dibujar texturas animadas)
            sProgramAnimated.Activate(); //Activamos el programa de shaders
            //Configuracion de las transformaciones del objeto en espacio de mundo
            sProgramAnimated.SetUniformValue("projMatrix", myCamera.ProjectionMatrix());
            sProgramAnimated.SetUniformValue("modelMatrix", modelMatrix);
            sProgramAnimated.SetUniformValue("viewMatrix", myCamera.ViewMatrix());
            sProgramAnimated.SetUniformValue("speed", -(float)timeSinceStartup * tankDirection);
            sProgramAnimated.SetUniformValue("ColorTex", 1);
            //Dibujamos las orugas del tanque
            orugas.transform.localToWorld = fisica.tank.MotionState.WorldTransform;
            orugas.Dibujar(sProgramAnimated);
            sProgramAnimated.Deactivate(); //Desactivamos el programa de shaders

            //SHADER PARA EL TERRENO
            sProgramTerrain.Activate();
            // TERRAIN MULTIPLES LUCES
            //Configuracion de los valores uniformes del shader
            sProgramTerrain.SetUniformValue("projMatrix", myCamera.ProjectionMatrix());
            sProgramTerrain.SetUniformValue("modelMatrix", modelMatrix);
            sProgramTerrain.SetUniformValue("normalMatrix", normalMatrix);
            sProgramTerrain.SetUniformValue("viewMatrix", myCamera.ViewMatrix());
            sProgramTerrain.SetUniformValue("cameraPosition", myCamera.Position());
            sProgramTerrain.SetUniformValue("A", 0.3f);
            sProgramTerrain.SetUniformValue("B", 0.007f);
            sProgramTerrain.SetUniformValue("C", 0.00008f);
            sProgramTerrain.SetUniformValue("material.Ka", material.Kambient);
            sProgramTerrain.SetUniformValue("material.Kd", material.Kdiffuse);
            sProgramTerrain.SetUniformValue("material.Ks", material.Kspecular);
            sProgramTerrain.SetUniformValue("material.Shininess", material.Shininess);

            //SplatMap (Para indicar que porcentaje de cada textura utilizar por fragmento)
            sProgramTerrain.SetUniformValue("ColorTex", 9);

            //Texturas
            sProgramTerrain.SetUniformValue("Texture1", 8);
            sProgramTerrain.SetUniformValue("Texture2", 11);
            sProgramTerrain.SetUniformValue("Texture3", 10);

            sProgramTerrain.SetUniformValue("numLights", luces.Length);
            for (int i = 0; i < luces.Length; i++)
            {
                sProgramTerrain.SetUniformValue("allLights[" + i + "].position", luces[i].Position);
                sProgramTerrain.SetUniformValue("allLights[" + i + "].Ia", luces[i].Iambient);
                sProgramTerrain.SetUniformValue("allLights[" + i + "].Ip", luces[i].Ipuntual);
                sProgramTerrain.SetUniformValue("allLights[" + i + "].coneAngle", luces[i].ConeAngle);
                sProgramTerrain.SetUniformValue("allLights[" + i + "].coneDirection", luces[i].ConeDirection);
                sProgramTerrain.SetUniformValue("allLights[" + i + "].enabled", luces[i].Enabled);
                //sProgram.SetUniformValue("allLights[" + i + "].direccional", luces[i].Direccional);
            }
            //Para sombras
            sProgramTerrain.SetUniformValue("shadowsOn", iShadowsOn);
            sProgramTerrain.SetUniformValue("uLightBiasMatrix", lightBiasMatrix);
            sProgramTerrain.SetUniformValue("uShadowSampler", mShadowTextureUnit);

            //Dibujo el terreno
            foreach (Mesh m in mapa.Meshes)
                if (m.Name == "Ground_Plane")
                    m.Dibujar(sProgramTerrain);
            sProgramTerrain.Deactivate();
        } 

        private void DibujarParticles()
        {
            //SECOND SHADER (Para dibujar las particulas)
            sProgramParticles.Activate(); //Activamos el programa de shaders
            sProgramParticles.SetUniformValue("projMatrix", myCamera.ProjectionMatrix());
            sProgramParticles.SetUniformValue("modelMatrix", Matrix4.Identity);  //Ajustar con la escala
            sProgramParticles.SetUniformValue("viewMatrix", myCamera.ViewMatrix());
            sProgramParticles.SetUniformValue("uvOffset", new Vector2(1f, 1f));
            sProgramParticles.SetUniformValue("time", (float)timeSinceStartup);
            sProgramParticles.SetUniformValue("animated", 0);
            sProgramParticles.SetUniformValue("ColorTex", 0);
            //Dibujamos el sistema de particulas
            explosiones.Dibujar(timeSinceStartup, sProgramParticles);
            if (toggleParticles)
            {
                //Humo
                sProgramParticles.SetUniformValue("ColorTex", 3);
                smokeParticles.Dibujar(sProgramParticles);
                aviones.DibujarDisparos(sProgramParticles);
                //Fuego animado
                sProgramParticles.SetUniformValue("uvOffset", new Vector2(0.5f, 0.5f));
                sProgramParticles.SetUniformValue("ColorTex", 7);
                sProgramParticles.SetUniformValue("animated", 1);
                fireParticles.Dibujar(sProgramParticles);
            }
            sProgramParticles.Deactivate(); //Desactivamos el programa de shaders
        }

        private void DibujarGizmos()
        {
            sProgramUnlit.Activate(); //Activamos el programa de shaders
            sProgramUnlit.SetUniformValue("projMatrix", myCamera.ProjectionMatrix());
            sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.Identity);
            sProgramUnlit.SetUniformValue("viewMatrix", myCamera.ViewMatrix());
            //Dibujamos los ejes de referencia.
            ejes_globales.Dibujar(sProgramUnlit);
            ejes_locales.Dibujar(sProgramUnlit);
            //Dibujamos la representacion visual de la luz.
            for (int i = 0; i < luces.Length; i++)
                luces[i].gizmo.Dibujar(sProgramUnlit);

            //Area de clickeo para explosion
            sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.CreateTranslation(explosiones.getCentro()));
            aviones.DibujarCuadradosDisparos(sProgramUnlit);
            cubo.Dibujar(sProgramUnlit);
            sProgramUnlit.Deactivate(); //Desactivamos el programa de shaders
        }
        #endregion

        #region Configuraciones (Setups)

        protected void SetupObjects()
        {
            //Carga de Texturas
            gl.ActiveTexture(TextureUnit.Texture0);
            CargarTextura("files/Texturas/Helper/no_s.jpg");
            gl.ActiveTexture(TextureUnit.Texture1);
            CargarTextura("files/Texturas/Helper/no_n.jpg");
            gl.ActiveTexture(TextureUnit.Texture2);
            CargarTextura("files/Texturas/Helper/no_s.jpg");

            gl.ActiveTexture(TextureUnit.Texture3);
            CargarTextura("files/Texturas/FX/smoke.png");

            gl.ActiveTexture(TextureUnit.Texture4);
            CargarTextura("files/Texturas/Map/ambientruins.png");
            gl.ActiveTexture(TextureUnit.Texture5);
            CargarTextura("files/Texturas/Map/distantbuilding.png");
            gl.ActiveTexture(TextureUnit.Texture6);
            CargarTextura("files/Texturas/Map/distantroof.png");

            gl.ActiveTexture(TextureUnit.Texture7);
            CargarTextura("files/Texturas/FX/fire.png");

            gl.ActiveTexture(TextureUnit.Texture8);
            CargarTextura("files/Texturas/Map/ground.png");

            gl.ActiveTexture(TextureUnit.Texture9);
            CargarTextura("files/Texturas/Map/splatmap.png");
            gl.ActiveTexture(TextureUnit.Texture10);
            CargarTextura("files/Texturas/Map/ladrillos.png");
            gl.ActiveTexture(TextureUnit.Texture11);
            CargarTextura("files/Texturas/Map/ruble_d.png");

            //TextureUnit.Texture12 -> Skybox

            GL.ActiveTexture(TextureUnit.Texture13);
            CargarTextura("files/Texturas/Vehicles/tiger_d.png");
            GL.ActiveTexture(TextureUnit.Texture14);
            CargarTextura("files/Texturas/Vehicles/track_d.png");
            GL.ActiveTexture(TextureUnit.Texture15);
            CargarTextura("files/Texturas/Vehicles/tiger_n.png");
            GL.ActiveTexture(TextureUnit.Texture16);
            CargarTextura("files/Texturas/Vehicles/tiger_s.png");

            //TextureUnit.Texture17 -> Sombras

            //Construimos los objetos que vamos a dibujar.
            //TODO Separar el ground del mapa para evitar esto de los builds
            mapa = new ObjetoGrafico("CGUNS/ModelosOBJ/Map/maptest.obj");
            mapa_col = new ObjetoGrafico("CGUNS/ModelosOBJ/Colisiones/mapcoll.obj");
            mapa_col.Build(sProgram, mShadowProgram);
            foreach(Mesh m in mapa.Meshes)
            {
                Char[] separator = { '.' };
                string prefijo = m.Name.Split(separator)[0];
                switch (prefijo)
                {
                    case "Background_Cube":
                        m.AddTexture(4);
                        m.Build(sProgram, mShadowProgram);
                        break;
                    case "Facade":
                    case "Window":
                    case "Chimney":
                        m.AddTexture(5);
                        m.Build(sProgram, mShadowProgram);
                        break;
                    case "Roof":
                        m.AddTexture(6);
                        m.Build(sProgram, mShadowProgram);
                        break;
                    case "Ground_Plane":
                        m.AddTexture(9);
                        m.Build(sProgramTerrain, mShadowProgram); //El terreno usa un shader especial
                        break;
                    default:
                        m.AddTexture(0);
                        m.Build(sProgram, mShadowProgram);
                        break;
                }
            }
            //mapa.Build(sProgram); //Construyo los buffers OpenGL que voy a usar.
            tanque = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/tiger.obj");
            tanque.AddTextureToAllMeshes(13);
            tanque.Build(sProgram, mShadowProgram); //Construyo los buffers OpenGL que voy a usar.
            orugas = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/tracks.obj");
            orugas.AddTextureToAllMeshes(14);
            orugas.Build(sProgram, mShadowProgram); //Construyo los buffers OpenGL que voy a usar.

            tanque_col = new ObjetoGrafico("CGUNS/ModelosOBJ/Colisiones/tanktest.obj");
            tanque_col.Build(sProgram, mShadowProgram); //Construyo los buffers OpenGL que voy a usar.

            mShadowViewportQuad = new ViewportQuad();
            mShadowViewportQuad.Build(mShadowViewportProgram);

            mSkyBox = new Skybox();
            mSkyBox.Build(mSkyBoxProgram);
            //Importa el orden, ver crearTexturaSkybox
            mSkyboxTextureId = CrearTexturaSkybox(
                new string[]
                {
                    "files/Texturas/Skybox/right.png",
                    "files/Texturas/Skybox/left.png",
                    "files/Texturas/Skybox/top.png",
                    "files/Texturas/Skybox/bottom.png",
                    "files/Texturas/Skybox/back.png",
                    "files/Texturas/Skybox/front.png",
                },
                mSkyBoxTextureUnit);
        }

        private int CrearTexturaSkybox(string[] paths, int unit)
        {
            int textId = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0 + unit);

            GL.BindTexture(TextureTarget.TextureCubeMap, textId);

            for (int i = 0; i < paths.Length; i++)
            {
                Bitmap bitmap = new Bitmap(Image.FromFile(paths[i]));

                BitmapData data = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(
                   TextureTarget.TextureCubeMapPositiveX + i,
                   0,
                   PixelInternalFormat.Rgba,
                   data.Width,
                   data.Height,
                   0,
                   OpenTK.Graphics.OpenGL.PixelFormat.Bgra,
                   PixelType.UnsignedByte,
                   data.Scan0);
            }

            GL.TexParameter(
                TextureTarget.TextureCubeMap,
                TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);

            GL.TexParameter(
                TextureTarget.TextureCubeMap,
                TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            GL.TexParameter(
                TextureTarget.TextureCubeMap,
                TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(
                TextureTarget.TextureCubeMap,
                TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.ClampToEdge);

            GL.TexParameter(
                TextureTarget.TextureCubeMap,
                TextureParameterName.TextureWrapR,
                (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);

            return textId;
        }

        private void SetupEjes()
        {
            ejes_globales = new Ejes();
            ejes_locales = new Ejes(0.5f, tanque);
            ejes_globales.Build(sProgramUnlit);
            ejes_locales.Build(sProgramUnlit);
        }

        protected void SetupLights()
        {
            luces = new Light[4];
            //Roja
            luces[0] = new Light();
            luces[0].Position = new Vector4(0.0f, 0.0f, 10.0f, 1.0f);
            luces[0].Iambient = new Vector3(0.0f, 0.0f, 0.0f);
            luces[0].Ipuntual = new Vector3(1.0f, 0.0f, 0.0f);
            luces[0].ConeAngle = 180.0f;
            luces[0].ConeDirection = new Vector3(0.0f, 0.0f, -1.0f);
            luces[0].Enabled = 0;
            luces[0].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
            //Direccional blanca
            luces[1] = new Light();
            //luces[1].Position = new Vector4(1.0f, -2.0f, -1.0f, 0.0f);
            luces[1].Position = new Vector4(3.5f, -5.0f, -2.5f, 0.0f);
            luces[1].Iambient = new Vector3(0.1f, 0.1f, 0.1f);
            luces[1].Ipuntual = new Vector3(1f, 1f, 1f);
            luces[1].ConeAngle = 180.0f;
            luces[1].ConeDirection = new Vector3(0.0f, -1.0f, 0.0f);
            luces[1].Enabled = 1;
            luces[1].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
            //Amarilla
            luces[2] = new Light();
            luces[2].Position = new Vector4(0.0f, 10.0f, 0.0f, 1.0f);
            luces[2].Iambient = new Vector3(0.1f, 0.1f, 0.1f);
            luces[2].Ipuntual = new Vector3(1f, 1f, 0.0f);
            luces[2].ConeAngle = 10.0f;
            luces[2].ConeDirection = new Vector3(0.0f, -1.0f, 0.0f);
            luces[2].Enabled = 0;
            luces[2].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
            //Azul
            luces[3] = new Light();
            luces[3].Position = new Vector4(0.0f, 0.0f, -3.0f, 1.0f);
            luces[3].Iambient = new Vector3(0.1f, 0.1f, 0.1f);
            luces[3].Ipuntual = new Vector3(0.0f, 0.0f, 0.5f);
            luces[3].ConeAngle = 20.0f;
            luces[3].ConeDirection = new Vector3(0.0f, 0.0f, 1.0f);
            luces[3].Enabled = 0;
            luces[3].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
        }

        private void SetupShaders(String vShaderName, String fShaderName, out ShaderProgram sProgram)
        {
            //1. Creamos los shaders, a partir de archivos.
            String vShaderFile = "files/shaders/" + vShaderName;
            String fShaderFile = "files/shaders/" + fShaderName;
            Shader vShader = new Shader(ShaderType.VertexShader, vShaderFile);
            Shader fShader = new Shader(ShaderType.FragmentShader, fShaderFile);
            //2. Los compilamos
            vShader.Compile();
            fShader.Compile();
            //3. Creamos el Programa de shader con ambos.
            sProgram = new ShaderProgram();
            sProgram.AddShader(vShader);
            sProgram.AddShader(fShader);
            //4. Construimos (linkeamos) el programa.
            sProgram.Build();
            //5. Ya podemos eliminar los shaders compilados. (Si no los vamos a usar en otro programa)
            vShader.Delete();
            fShader.Delete();
        }

        private void SetupParticles()
        {
            smokeParticles = new Smoke(new Vector3(5, 0, 5));
            smokeParticles.Build(sProgramParticles);
            fireParticles = new Fire(new Vector3(7.5f, 2.5f, 2));
            fireParticles.Build(sProgramParticles);
        }

        private int CargarTextura(String imagenTex)
        {
            int texId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texId);


            Bitmap bitmap = new Bitmap(Image.FromFile(imagenTex));

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                             ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);
            return texId;

        }

        /// <summary>
        /// Creo el framebuffer y la textura necesaria para generar el shadow map.
        /// </summary>
        private void CrearShadowTextures()
        {
            // Necesito generar un framebuffer y una textura 2D para almacenar el depth buffer.
            TextureTarget textureTarget = TextureTarget.Texture2D;
            FramebufferTarget framebufferTarget = FramebufferTarget.Framebuffer;

            mShadowViewport = new Rectangle(0, 0, 4096, 4096);//2048, 2048);

            // 1. Genero un framebuffer.
            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(framebufferTarget, fbo);


            // 2. Genero una textura para vincular al framebuffer.
            depthTexture = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0 + mShadowTextureUnit);
            GL.BindTexture(textureTarget, depthTexture);
            GL.TexImage2D(
                textureTarget,
                0,
                PixelInternalFormat.DepthComponent16, // Solo voy a utilizar el componente de profundidad.
                mShadowViewport.Width,
                mShadowViewport.Height,
                0,
                OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                PixelType.Float,
                IntPtr.Zero);

            GL.TexParameter(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(textureTarget, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(textureTarget, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);

            // 3. Seteo que cuando salgo de los limites de la textura sampleo el color blanco.
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            // 4. Vinculo la textura al framebuffer.
            GL.FramebufferTexture2D(
                framebufferTarget,
                FramebufferAttachment.DepthAttachment,
                TextureTarget.Texture2D,
                depthTexture,
                0);

            // 5. IMPORTANTE: Chequeo si el framebuffer esta completo.
            if (GL.CheckFramebufferStatus(framebufferTarget) != FramebufferErrorCode.FramebufferComplete)
            {   
                if (shadowsOn)             
                    throw new InvalidOperationException("El framebuffer no fue completamente creado.");
            }
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void ToggleLight(int i)
        {
            if (i < luces.Length)
            {
                luces[i].Toggle();
            }
        }

        private void ModificarCono(float deltaGrados, int luz)
        {
            if (luces.Length >= luz && luces.Length >= 1 && luces[luz].Enabled == 1 && luces[luz].Position.W != 0)
            {
                float coneAngle = luces[luz].ConeAngle;
                coneAngle = coneAngle + deltaGrados;
                if (coneAngle < 2.0f)
                {
                    coneAngle = 2.0f;
                }
                if (coneAngle > 45.0f)
                {
                    coneAngle = 45.0f;
                }
                luces[luz].ConeAngle = coneAngle;
            }
        }

        #endregion

        #region Funciones de Debugeo

        private void updateDebugInfo()
        {
            //Muestro informacion de Debugeo en el titulo de la ventana
            int vertCount = 0, faceCount = 0, normalCount = 0, objCount = tanque.Meshes.Count;
            foreach (FVLMesh m in tanque.Meshes)
            {
                vertCount += m.VertexCount;
                faceCount += m.FaceCount;
                normalCount += m.VertexNormalList.Count;
            }

            DisplayFPS();

            String title = "CGProy2016 [FPS:" + fps + "] [Verts:" + vertCount + " - Normals:" + normalCount + " - Faces:" + faceCount + " - Objects:" + objCount + " - Material:" + materialIndex +
                "] [DebugNormals: " + toggleNormals + " - Wireframe: " + toggleWires + " - DrawGizmos: " + drawGizmos +
                "] [Lights: ";

            for (int i = 0; i < luces.Length; i++)
            {
                bool onoff = luces[i].Enabled == 1 ? true : false;
                title += "L" + i + ": " + onoff;
                if (i != luces.Length - 1)
                    title += " - ";
            }
            title += "]";

            this.Title = title;

        }
        private void DisplayFPS()
        {
            if (DateTime.Now >= NextFPSUpdate)
            {
                // Display the number of frames in the last second
                //this.Title = String.Format("First Person GL (fps={0}) Faces: {1}", FrameCount, facecount);
                fps = FrameCount;

                // Calculate the time of the next update
                NextFPSUpdate = DateTime.Now.AddSeconds(1);

                // Reset the frame count
                FrameCount = 0;
            }

            // Increment the frame count
            FrameCount++;
        }
        private void logContextInfo()
        {
            String version, renderer, shaderVer, vendor;//, extensions;
            version = gl.GetString(StringName.Version);
            renderer = gl.GetString(StringName.Renderer);
            shaderVer = gl.GetString(StringName.ShadingLanguageVersion);
            vendor = gl.GetString(StringName.Vendor);
            //extensions = gl.GetString(StringName.Extensions);
            log("========= CONTEXT INFORMATION =========");
            log("Renderer:       {0}", renderer);
            log("Vendor:         {0}", vendor);
            log("OpenGL version: {0}", version);
            log("GLSL version:   {0}", shaderVer);
            //log("Extensions:" + extensions);
            log("===== END OF CONTEXT INFORMATION =====");

        }
        private void log(String format, params Object[] args)
        {
            System.Diagnostics.Debug.WriteLine(String.Format(format, args), "[CGUNS]");
        }
        #endregion
    }
}