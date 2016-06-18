using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK; //La matematica
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using CGUNS;
using CGUNS.Shaders;
using CGUNS.Cameras;
using CGUNS.Primitives;
using CGUNS.Meshes;
using CGUNS.Meshes.FaceVertexList;
using System.Drawing.Imaging;
using System.Diagnostics;
using BulletSharp;

namespace cg2016
{
    public partial class MapTestScene : Form
    {
        public MapTestScene()
        {
            InitializeComponent();
        }
        private int jkl=0;
        private System.Timers.Timer timer; // From System.Timers

        private double timeSinceStartup = 0; //Tiempo total desde el inicio del programa (En segundos)
        private double deltaTime = 0;   //Tiempo que tomo completar el ultimo frame (En segundos)
        private int frameCount = 0; //Cantidad de frames en el ultimo segundo
        //private long fps_startTime = 0;
        private int fps = 0;    //FramesPorSegundo
        private Stopwatch stopWatch = new Stopwatch();
        private double fps_timeInterval = 0;

        private ShaderProgram sProgram; //Nuestro programa de shaders.
        private ShaderProgram sProgramUnlit; //Nuestro programa de shaders.
        private ShaderProgram sProgramParticles; //Nuestro programa de shaders.

        private ParticleEmitter particles;
        private ObjetoGrafico mapa; //Nuestro objeto a dibujar.
        private ObjetoGrafico objeto; //Nuestro objeto a dibujar.
        private Light[] luces;
        private Material[] materiales = new Material[] { Material.Default, Material.WhiteRubber, Material.Obsidian, Material.Bronze, Material.Gold, Material.Jade, Material.Brass };
        private Material material;
        private int materialIndex = 0;
        private QSphericalCamera myCamera;  //Camara
        private Rectangle viewport; //Viewport a utilizar (Porcion del glControl en donde voy a dibujar).
        private Ejes ejes_globales; // Ejes de referencia globales
        private Ejes ejes_locales; // Ejes de referencia locales al objeto
        private fisica fisica;

        private bool toggleNormals = false;
        private bool toggleWires = false;
        private bool drawGizmos = true;
        private bool toggleParticles = false;

        private int transformaciones = 0;
        private int tex1, tex2, tex3;

        private Cube cubo;
        Matrix4 viewMatrix;
        Matrix4 projMatrix;
        private Vector3 sphereCenters = new Vector3(2.0f, 0.0f, 0.0f);
        private float sphereRadius = 3f;
        private double[] inicioExplosiones;
        static int maxExplosiones = 5;
        private ParticleEmitter[] explosiones = new ParticleEmitter[maxExplosiones];

        private void glControl3_Load(object sender, EventArgs e)
        {
            logContextInfo(); //Mostramos info de contexto.

            //Arrancamos la clase fisica (Mirá como está esa modularización papa)
            fisica = new fisica();
            
            //Creamos los shaders y el programa de shader
            SetupShaders("vunlit.glsl", "funlit.glsl", out sProgramUnlit);
            //SetupShaders("vbumpedspecularphong.glsl", "fbumpedspecularphong.glsl", out sProgram);
            SetupShaders("vmultiplesluces.glsl", "fmultiplesluces.glsl", out sProgram);
            SetupShaders("vparticles.glsl", "fparticles.glsl", out sProgramParticles);
                        
            //Configuracion de los sistemas de particulas
            particles = new ParticleEmitter(Vector3.Zero, Vector3.UnitY * 0.25f, 500);
            particles.Build(sProgramParticles);

            cubo = new Cube(0.1f, 0.1f, 0.1f);
            cubo.Build(sProgramUnlit);

            inicioExplosiones = new double[maxExplosiones];
            for (int i = 0; i < maxExplosiones; i++)
                inicioExplosiones[i] = -3;

            //Carga y configuracion de Objetos
            mapa = new ObjetoGrafico("CGUNS/ModelosOBJ/Map/maptest.obj"); //Construimos los objetos que voy a dibujar.
            mapa.Build(sProgram); //Construyo los buffers OpenGL que voy a usar.
            objeto = new ObjetoGrafico("CGUNS/ModelosOBJ/Vehicles/tanktest.obj"); //Construimos los objetos que voy a dibujar.
            objeto.Build(sProgram); //Construyo los buffers OpenGL que voy a usar.

            //Carga de Texturas
            GL.ActiveTexture(TextureUnit.Texture0);
			//tex1 = CargarTextura("files/Texturas/BrickWallHD_d.png");
            tex1 = CargarTextura("files/Texturas/Helper/no_s.jpg");
            GL.ActiveTexture(TextureUnit.Texture1);
            //tex2 = CargarTextura("files/Texturas/BrickWallHD_n.png");
            tex2 = CargarTextura("files/Texturas/Helper/no_n.jpg");
            GL.ActiveTexture(TextureUnit.Texture2);
            //tex3 = CargarTextura("files/Texturas/BrickWallHD_s.png");
            tex3 = CargarTextura("files/Texturas/Helper/no_s.jpg");

            //Configuracion de la Camara
            myCamera = new QSphericalCamera(50, 45, 30, 0.1f, 250); //Creo una camara.
            gl.ClearColor(Color.Black); //Configuro el Color de borrado.
            gl.Enable(EnableCap.DepthTest);
            //gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //Configuracion de Ejes
            ejes_globales = new Ejes();
            ejes_locales = new Ejes(0.5f, objeto);
            ejes_globales.Build(sProgramUnlit);
            ejes_locales.Build(sProgramUnlit);

            //Configuracion de las Luces
            luces = new Light[4];
            //Roja
            luces[0] = new Light();
            luces[0].Position = new Vector4(0.0f, 0.0f, 10.0f, 1.0f);
            luces[0].Iambient = new Vector3(0.0f, 0.0f, 0.0f);
            luces[0].Ipuntual = new Vector3(1.0f, 0.0f, 0.0f);
            luces[0].ConeAngle = 180.0f;
            luces[0].ConeDirection = new Vector3(0.0f, 0.0f, -1.0f);
            luces[0].Enabled = 0;
            luces[0].Direccional = 0;
            luces[0].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
            //Direccional blanca
            luces[1] = new Light();
            luces[1].Position = new Vector4(1.0f, -2.0f, -1.0f, 0.0f);
            luces[1].Iambient = new Vector3(0.1f, 0.1f, 0.1f);
            luces[1].Ipuntual = new Vector3(0.75f, 0.75f, 0.75f);
            luces[1].ConeAngle = 180.0f;
            luces[1].ConeDirection = new Vector3(0.0f, -1.0f, 0.0f);
            luces[1].Enabled = 1;
            luces[1].Direccional = 1;
            luces[1].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
            //Amarilla
            luces[2] = new Light();
            luces[2].Position = new Vector4(0.0f, 10.0f, 0.0f, 1.0f); 
            luces[2].Iambient = new Vector3(0.1f, 0.1f, 0.1f);
            luces[2].Ipuntual = new Vector3(1f, 1f, 0.0f);
            luces[2].ConeAngle = 10.0f;
            luces[2].ConeDirection = new Vector3(0.0f, -1.0f, 0.0f);
            luces[2].Enabled = 0;
            luces[2].Direccional = 0;
            luces[2].updateGizmo(sProgramUnlit);    //Representacion visual de la luz
            //Azul
            luces[3] = new Light();
            luces[3].Position = new Vector4(0.0f, 0.0f, -3.0f, 1.0f);
            luces[3].Iambient = new Vector3(0.1f, 0.1f, 0.1f);
            luces[3].Ipuntual = new Vector3(0.0f, 0.0f, 0.5f);
            luces[3].ConeAngle = 20.0f;
            luces[3].ConeDirection = new Vector3(0.0f, 0.0f, 1.0f);
            luces[3].Enabled = 0;
            luces[3].Direccional = 0;
            luces[3].updateGizmo(sProgramUnlit);    //Representacion visual de la luz

            //Configuracion de Materiales
            material = materiales[materialIndex];
            
            updateDebugInfo();
            
            //Configuracion del Timer para redibujar la escena cada 10ms
            timer = new System.Timers.Timer(10);
            timer.Elapsed += Update;
            timer.AutoReset = true;
            timer.Enabled = true;
            stopWatch.Start();
            
        }

        //Se ejecuta cada tick del Timer, actualiza a las entidades dinamicas y redibuja la escena.
        private void Update(Object source, System.Timers.ElapsedEventArgs e)
        {
            fisica.dynamicsWorld.StepSimulation(10);
        

        //Console.WriteLine("Timer");
        //Incremento el tiempo transcurrido
        timeSinceStartup += deltaTime;

            //Animacion de una luz
            float blend = ((float)Math.Sin(timeSinceStartup/4) + 1)/2;
            Vector3 pos = Vector3.Lerp(new Vector3(-40.0f, 10.0f, 0.0f), new Vector3(40.0f, 10.0f, 0.0f), blend);
            //Vector3 pos = Vector3.Lerp(new Vector3(0.0f, 2.0f, -4.0f), new Vector3(0.0f, 2.0f, 4.0f), blend);
            luces[0].Position = new Vector4(pos, 1.0f);

            //Actualizo los sistemas de particulas
            particles.Update();

            for (int i = 0; i < maxExplosiones; i++)
                if (timeSinceStartup > inicioExplosiones[i] && timeSinceStartup < inicioExplosiones[i] + 2)
                    explosiones[i].Update();

            //Invalidamos el glControl para que se redibuje.(llama al metodo Paint)
            glControl3.Invalidate();

            //Terminamos de procesar el frame, calculamos el FPS
            //UpdateFramesPerSecond(); //Si lo llamo aca no tiene en cuenta el tiempo que tarda Paint
        }

        //FramesPerSecond con Stopwatch
        private void UpdateFramesPerSecond()
        {
            frameCount++;
            stopWatch.Stop();
            //Tiempo que tomo procesar el frame
            deltaTime = stopWatch.Elapsed.TotalSeconds;//stopWatch.Elapsed.TotalMilliseconds;
            stopWatch.Reset();
            stopWatch.Start();
            //Acumulo el tiempo hasta que pase 1 segundo
            fps_timeInterval += deltaTime;
            if (fps_timeInterval >= 1)//1000)
            {
                fps = frameCount;
                frameCount = 0;
                fps_timeInterval = 0;//-= 1000;
            }
        }

        private void updateDebugInfo()
        {
            //Muestro informacion de Debugeo en el titulo de la ventana
            int vertCount = 0, faceCount = 0, normalCount = 0, objCount = objeto.Meshes.Count;
            foreach (FVLMesh m in objeto.Meshes)
            {
                vertCount += m.VertexCount;
                faceCount += m.FaceCount;
                normalCount += m.VertexNormalList.Count;
            }

            String title = "CGProy2016 [FPS:"+fps+"] [Verts:" + vertCount + " - Normals:" + normalCount + " - Faces:" + faceCount + " - Objects:" + objCount + " - Material:" + materialIndex +
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

            this.Text = title;
            
        }

        private void glControl3_Paint(object sender, PaintEventArgs e)
        {
            if(toggleWires)
              gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line); //De cada poligono solo dibujo las lineas de contorno (wireframe).
              else gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill); 

            Matrix4 modelMatrix = Matrix4.Identity; //Por ahora usamos la identidad.
            viewMatrix = myCamera.getViewMatrix();
            projMatrix = myCamera.getProjectionMatrix();
            Matrix4 mvMatrix = Matrix4.Mult(viewMatrix, modelMatrix);
            //Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(mvMatrix))); //En Espacio de OJO
            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(modelMatrix))); //En Espacio de MUNDO
            Matrix4 MVP = Matrix4.Mult(mvMatrix, projMatrix);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); //Borramos el contenido del glControl.
            gl.Viewport(viewport); //Especificamos en que parte del glControl queremos dibujar.
            
            //FIRST SHADER (Para dibujar objetos)
            sProgram.Activate(); //Activamos el programa de shaders

            //Configuracion de las transformaciones del objeto en espacio de mundo
            //mapa.transform.scale = new Vector3(0.25f, 0.25f, 0.25f);
            //objeto.transform.scale = new Vector3(0.25f, 0.25f, 0.25f);

            #region Configuracion de Uniforms
            /*
            /// BUMPED SCPECULAR PHONG
            //Configuracion de los valores uniformes del shader
            sProgram.SetUniformValue("projMatrix", projMatrix);
            sProgram.SetUniformValue("modelMatrix", modelMatrix);
            //sProgram.SetUniformValue("normalMatrix", normalMatrix);
            sProgram.SetUniformValue("viewMatrix", viewMatrix);
            //sProgram.SetUniformValue("cameraPosition", myCamera.getPosition());
            sProgram.SetUniformValue("A", 0.3f);
            sProgram.SetUniformValue("B", 0.007f);
            sProgram.SetUniformValue("C", 0.00008f);
            sProgram.SetUniformValue("material.Ka", material.Kambient);
            sProgram.SetUniformValue("material.Kd", material.Kdiffuse);
            //sProgram.SetUniformValue("material.Ks", material.Kspecular);
            sProgram.SetUniformValue("material.Shininess", material.Shininess);
            sProgram.SetUniformValue("ColorTex", 0);
            sProgram.SetUniformValue("NormalMapTex", 1);
            sProgram.SetUniformValue("SpecularMapTex", 2);
            
            sProgram.SetUniformValue("numLights", luces.Length);
            for (int i = 0; i < luces.Length; i++)
            {
                sProgram.SetUniformValue("allLights[" + i + "].position", luces[i].Position);
                sProgram.SetUniformValue("allLights[" + i + "].Ia", luces[i].Iambient);
                sProgram.SetUniformValue("allLights[" + i + "].Ip", luces[i].Ipuntual);
                sProgram.SetUniformValue("allLights[" + i + "].coneAngle", luces[i].ConeAngle);
                sProgram.SetUniformValue("allLights[" + i + "].coneDirection", luces[i].ConeDirection);
                sProgram.SetUniformValue("allLights[" + i + "].enabled", luces[i].Enabled);
                sProgram.SetUniformValue("allLights[" + i + "].direccional", luces[i].Direccional);
            }*/

            
            // MULTIPLES LUCES
            //Configuracion de los valores uniformes del shader
            sProgram.SetUniformValue("projMatrix", projMatrix);
            sProgram.SetUniformValue("modelMatrix", modelMatrix);
            sProgram.SetUniformValue("normalMatrix", normalMatrix);
            sProgram.SetUniformValue("viewMatrix", viewMatrix);
            sProgram.SetUniformValue("cameraPosition", myCamera.getPosition());
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
                //sProgram.SetUniformValue("allLights[" + i + "].direccional", luces[i].Direccional);
            }
            #endregion


            //Dibujamos el Objeto
            objeto.setModelsMatrix(fisica.tank.MotionState.WorldTransform);
            objeto.transform.getset = fisica.tank.MotionState.WorldTransform;
            objeto.Dibujar(sProgram, viewMatrix);
            if (toggleNormals) objeto.DibujarNormales(sProgram, viewMatrix);

            //Dibujamos el Mapa
            mapa.setModelsMatrix(fisica.map.MotionState.WorldTransform);
            mapa.Dibujar(sProgram, viewMatrix);
            if (toggleNormals) mapa.DibujarNormales(sProgram, viewMatrix);

            
            sProgram.Deactivate(); //Desactivamos el programa de shader.

            
            //SECOND SHADER (Para dibujar las particulas)
            sProgramParticles.Activate(); //Activamos el programa de shaders
            sProgramParticles.SetUniformValue("projMatrix", projMatrix);
            sProgramParticles.SetUniformValue("modelMatrix", modelMatrix);
            sProgramParticles.SetUniformValue("viewMatrix", viewMatrix);
            //Dibujamos el sistema de particulas

            for (int i = 0; i < maxExplosiones; i++)
                if (timeSinceStartup > inicioExplosiones[i] && timeSinceStartup < inicioExplosiones[i] + 2)
                    explosiones[i].Dibujar(sProgramParticles);

            if (toggleParticles)
                particles.Dibujar(sProgramParticles);
            sProgramParticles.Deactivate(); //Desactivamos el programa de shaders

            if (drawGizmos)
            {
                //THIRD SHADER (Para dibujar los gizmos)
                sProgramUnlit.Activate(); //Activamos el programa de shaders
                sProgramUnlit.SetUniformValue("projMatrix", projMatrix);
                sProgramUnlit.SetUniformValue("modelMatrix", modelMatrix);
                sProgramUnlit.SetUniformValue("viewMatrix", viewMatrix);
                //Dibujamos los ejes de referencia.
                ejes_globales.Dibujar(sProgramUnlit);
                ejes_locales.Dibujar(sProgramUnlit);
                //Dibujamos la representacion visual de la luz.
                for (int i = 0; i < luces.Length; i++)
                    luces[i].gizmo.Dibujar(sProgramUnlit);

                sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.CreateTranslation(sphereCenters));
                cubo.Dibujar(sProgramUnlit);

                sProgramUnlit.Deactivate(); //Desactivamos el programa de shaders
            }

            glControl3.SwapBuffers(); //Intercambiamos buffers frontal y trasero, para evitar flickering.

            //Actualizamos la informacion de debugeo
            updateDebugInfo();

            //Terminamos de procesar el frame, calculamos el FPS
            UpdateFramesPerSecond();
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

        private void glControl3_Resize(object sender, EventArgs e)
        {   //Actualizamos el viewport para que dibuje en el centro de la pantalla.
            Size size = glControl3.Size;
            if (size.Width < size.Height)
            {
                viewport.X = 0;
                viewport.Y = (size.Height - size.Width) / 2;
                viewport.Width = size.Width;
                viewport.Height = size.Width;
            }
            else
            {
                viewport.X = (size.Width - size.Height) / 2;
                viewport.Y = 0;
                viewport.Width = size.Height;
                viewport.Height = size.Height;
            }
            glControl3.Invalidate(); //Invalidamos el glControl para que se redibuje.(llama al metodo Paint)
        }

        #region Entrada por teclado

        private void glControl3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void glControl3_KeyPressed(object sender, KeyEventArgs e)
        {
            if (transformaciones == 1) transformaciones = 0;

            //De 48 a 57 estan 0 a 9. De 96 a 105 numerico.
            if (e.Control && (int)e.KeyCode >= 48 && (int)e.KeyCode <= 57)
            {
                ModificarCono(-2.0f, (int)e.KeyCode - 48);
            }
            else
                if (e.Shift && (int)e.KeyCode >= 48 && (int)e.KeyCode <= 57)
                {
                    ModificarCono(2.0f, (int)e.KeyCode - 48);
                }
                else
                     if ((int)e.KeyCode >= 48 && (int)e.KeyCode <= 57)
                    {   
                        ToggleLight((int)e.KeyCode - 48);
                    }
                    else
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Down:
                                fisica.tank.LinearVelocity+=(-objeto.transform.forward);
                                break;
                            case Keys.Up:
                                fisica.tank.LinearVelocity+=(objeto.transform.forward);
                                break;
                            case Keys.Right:
                                fisica.tank.ApplyTorqueImpulse(new Vector3(0, 1f, 0));
                                break;
                            case Keys.Left:
                                fisica.tank.ApplyTorqueImpulse(new Vector3(0, -1f, 0));
                                break;
                            case Keys.S:
                                myCamera.Abajo();
                                break;
                            case Keys.W:
                                myCamera.Arriba();
                                break;
                            case Keys.D:
                                myCamera.Derecha();
                                break;
                            case Keys.A:
                                myCamera.Izquierda();
                                break;
                            case Keys.Add:
                            case Keys.I:
                                myCamera.Acercar(0.5f);
                                break;
                            case Keys.Subtract:
                            case Keys.O:
                                myCamera.Alejar(0.5f);
                                break;
                            //Teclas para activar/desactivar funciones
                            case Keys.F3:
                                toggleWires = !toggleWires;
                                break;
                            case Keys.F2:
                                toggleNormals = !toggleNormals;
                                break;
                            case Keys.G:
                                drawGizmos = !drawGizmos;
                                break;
                            //CAMBIO DE MATERIAL
                            case Keys.Z:
                                materialIndex = (materiales.Length + materialIndex - 1) % materiales.Length;
                                material = materiales[materialIndex];
                                break;
                            case Keys.X:
                                materialIndex = (materialIndex + 1) % materiales.Length;
                                material = materiales[materialIndex];
                                break;
                            case Keys.P:
                                toggleParticles = !toggleParticles;
                                break;
                            case Keys.J:
                                myCamera= new QSphericalCamera(10, 270, 10, 0.1f, 250); 
                                break;
                }
                    }

            glControl3.Invalidate(); //Notar que renderizamos para CUALQUIER tecla que sea presionada.
            //Actualizar la info de debugeo
            //updateDebugInfo();
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
            if (luces.Length >= luz && luces.Length >= 1 && luces[luz].Enabled == 1 && luces[luz].Direccional != 1)
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

        #endregion Entrada por teclado

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



        private void glControl3_Click(object sender, EventArgs e)
        {
            MouseEventArgs mouse = (MouseEventArgs)e;
            int Xopengl = mouse.X;
            int Yopengl = glControl3.Height - mouse.Y; //change Y axis
            if (viewport.Contains(Xopengl, Yopengl))
            { //Inside viewport?
                int Xviewport = Xopengl - viewport.X;
                int Yviewport = Yopengl - viewport.Y;
                Vector3 ray_wor = getRayFromMouse(Xviewport, Yviewport);


                float rToSphere = rayToSphere(myCamera.getPosition(), ray_wor, sphereCenters, sphereRadius);
                if (rToSphere != -1.0f)
                {
                    Vector3 origenParticulas = proyeccion(myCamera.getPosition(), ray_wor * 10);

                    double tiempoAux = inicioExplosiones[0]; //se busca un lugar para la explosion en el arreglo o se remplaza el mas antiguo
                    int masAntigua = 0;
                    for (int i = 0; i < maxExplosiones; i++)
                        if (inicioExplosiones[i] < tiempoAux)
                        {
                            tiempoAux = inicioExplosiones[i];
                            masAntigua = i;
                        }
                    explosiones[masAntigua] = new ParticleEmitter(origenParticulas, Vector3.UnitY * 0.25f, 500);
                    explosiones[masAntigua].Build(sProgramParticles);
                    inicioExplosiones[masAntigua] = timeSinceStartup;
                }

            }

            glControl3.Invalidate(); //Ask for Redrawing.
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
            Vector4 ray_eye = Vector4.Transform(ray_clip, Matrix4.Invert(projMatrix)); //inverse(projMat) * ray_clip
            ray_eye = new Vector4(ray_eye.X, ray_eye.Y, -1.0f, 0.0f);
            // world space
            Vector4 aux = Vector4.Transform(ray_eye, Matrix4.Invert(viewMatrix)); //inverse(viewMat) * ray_eye
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


    

}
}
