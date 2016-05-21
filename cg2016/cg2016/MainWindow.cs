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

namespace cg2016
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private System.Timers.Timer timer; // From System.Timers
        private float time = 0;

        private ShaderProgram sProgram; //Nuestro programa de shaders.
        private ShaderProgram sProgramUnlit; //Nuestro programa de shaders.

        private ObjetoGrafico objeto; //Nuestro objeto a dibujar.
        private Light[] luces;
        private Material[] materiales = new Material[] { Material.Default, Material.WhiteRubber, Material.Obsidian, Material.Bronze, Material.Gold, Material.Jade, Material.Brass };
        private Material material;
        private int materialIndex = 0;
        private QSphericalCamera myCamera;  //Camara
        private Rectangle viewport; //Viewport a utilizar (Porcion del glControl en donde voy a dibujar).
        private Ejes ejes_globales; // Ejes de referencia globales
        //private Ejes ejes_locales; // Ejes de referencia laocales al cubo

        private bool toggleNormals = false;
        private bool toggleWires = false;
        private bool drawGizmos = true;

        private int transformaciones = 0;
		private int tex1,tex2, tex3, tex4;
        private void glControl3_Load(object sender, EventArgs e)
        {            
            logContextInfo(); //Mostramos info de contexto.
            SetupShaders(); //Creamos los shaders y el programa de shader

            //Configuracion de Ejes
            ejes_globales = new Ejes();
            //ejes_locales = new Ejes(0.4f);
            ejes_globales.Build(sProgramUnlit);
            //ejes_locales.Build(sProgramUnlit);

            //Carga y configuracion de Objetos
            objeto = new ObjetoGrafico("CGUNS/ModelosOBJ/supercube.obj"); //Construimos los objetos que voy a dibujar.
            objeto.Build(sProgram); //Construyo los buffers OpenGL que voy a usar.
            GL.ActiveTexture(TextureUnit.Texture0);
			tex1 = CargarTextura("files/Texturas/BrickWallHD_d.png");
            //tex1 = CargarTextura("files/Texturas/chesterfield_d.png");
            GL.ActiveTexture(TextureUnit.Texture1);
            tex2 = CargarTextura("files/Texturas/BrickWallHD_n.png");
            //tex2 = CargarTextura("files/Texturas/chesterfield_n.png");
            GL.ActiveTexture(TextureUnit.Texture2);
            tex3 = CargarTextura("files/Texturas/BrickWallHD_s.png");
            //Configuracion de la Camara
            myCamera = new QSphericalCamera(); //Creo una camara.
            gl.ClearColor(Color.Black); //Configuro el Color de borrado.
            gl.Enable(EnableCap.DepthTest);
            //gl.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            //Configuracion de las Luces
            
            luces = new Light[4];

            luces[0] = new Light();
            luces[0].Position = new Vector4(0.0f, 0.0f, 2.0f, 1.0f);
            luces[0].Iambient = new Vector3(0.1f, 0.0f, 0.1f);
            luces[0].Idiffuse = new Vector3(1.0f, 0.0f, 0.0f);
            luces[0].Ispecular = new Vector3(1.0f, 1.0f, 1.0f);
            luces[0].ConeAngle = 180.0f;
            luces[0].ConeDirection = new Vector3(0.0f, -1.0f, 0.0f);
            luces[0].Enabled = 1;
            luces[0].Direccional = 0;
            luces[0].updateGizmo(sProgramUnlit);    //Representacion visual de la luz

            luces[1] = new Light();
            luces[1].Position = new Vector4(1.0f, 2.0f, 1.0f, 1.0f);
            luces[1].Iambient = new Vector3(0f, 0f, 0f);
            luces[1].Idiffuse = new Vector3(1f, 1f, 1f);
            luces[1].Ispecular = new Vector3(0.1f, 0.1f, 0.1f);
            luces[1].ConeAngle = 180.0f;
            luces[1].ConeDirection = new Vector3(0.0f, -1.0f, 0.0f);
            luces[1].Enabled = 0;
            luces[1].Direccional = 1;
            luces[1].updateGizmo(sProgramUnlit);    //Representacion visual de la luz

            luces[2] = new Light();
            luces[2].Position = new Vector4(0.0f, -3.0f, 0.0f, 1.0f); 
            luces[2].Iambient = new Vector3(0.1f, 0.1f, 0.0f);
            luces[2].Idiffuse = new Vector3(1f, 1f, 0.0f);
            luces[2].Ispecular = new Vector3(0.8f, 0.8f, 0.8f);
            luces[2].ConeAngle = 10.0f;
            luces[2].ConeDirection = new Vector3(0.0f, 1.0f, 0.0f);
            luces[2].Enabled = 0;
            luces[2].Direccional = 1;
            luces[2].updateGizmo(sProgramUnlit);    //Representacion visual de la luz

            luces[3] = new Light();
            luces[3].Position = new Vector4(0.0f, 0.0f, -3.0f, 1.0f);
            luces[3].Iambient = new Vector3(0.0f, 0.0f, 0.1f);
            luces[3].Idiffuse = new Vector3(0.0f, 0.0f, 0.5f);
            luces[3].Ispecular = new Vector3(0.8f, 0.8f, 0.8f);
            luces[3].ConeAngle = 20.0f;
            luces[3].ConeDirection = new Vector3(0.0f, 0.0f, -1.0f);
            luces[3].Enabled = 0;
            luces[3].Direccional = 1;
            luces[3].updateGizmo(sProgramUnlit);    //Representacion visual de la luz

            //Configuracion de Materiales
            material = materiales[materialIndex];
            
            updateDebugInfo();

            //Configuracion del Timer para redibujar la escena cada 10ms
            timer = new System.Timers.Timer(10);
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        //Se ejecuta cada tick del Timer y redibuja la escena. Sirve para animar
        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Console.WriteLine("Timer");
            time += 0.01f;
            float blend = ((float)Math.Sin(time) + 1)/2;
            Vector3 pos = Vector3.Lerp(new Vector3(-4.0f, 2.0f, 0.0f), new Vector3(4.0f, 2.0f, 0.0f), blend);
            luces[1].Position = new Vector4(pos, 1.0f);
            glControl3.Invalidate(); //Invalidamos el glControl para que se redibuje.(llama al metodo Paint)

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

            String title = "CGLabo2016 [Verts:" + vertCount + " - Normals:" + normalCount + " - Faces:" + faceCount + " - Objects:" + objCount + " - Material:" + materialIndex +
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
            Matrix4 viewMatrix = myCamera.getViewMatrix();
            Matrix4 projMatrix = myCamera.getProjectionMatrix();
            Matrix4 mvMatrix = Matrix4.Mult(viewMatrix, modelMatrix);
            Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(mvMatrix)));
            //Matrix3 normalMatrix = Matrix3.Transpose(Matrix3.Invert(new Matrix3(modelMatrix)));
            Matrix4 MVP = Matrix4.Mult(mvMatrix, projMatrix);

            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); //Borramos el contenido del glControl.
            gl.Viewport(viewport); //Especificamos en que parte del glControl queremos dibujar.

            if(drawGizmos)
            {
                //FIRST SHADER (Para dibujar los gizmos)
                sProgramUnlit.Activate(); //Activamos el programa de shaders
                sProgramUnlit.SetUniformValue("projMatrix", projMatrix);
                sProgramUnlit.SetUniformValue("modelMatrix", modelMatrix);
                sProgramUnlit.SetUniformValue("viewMatrix", viewMatrix);
                //Dibujamos los ejes de referencia.
                ejes_globales.Dibujar(sProgramUnlit);
                //Dibujamos la representacion visual de la luz.
                for (int i = 0; i < luces.Length; i++)
                    luces[i].gizmo.Dibujar(sProgramUnlit);
                sProgramUnlit.Deactivate(); //Desactivamos el programa de shaders
            }

            //SECOND SHADER (Para dibujar objetos)
            sProgram.Activate(); //Activamos el programa de shaders

            //Configuracion de los valores uniformes del shader
            
            sProgram.SetUniformValue("projMatrix", projMatrix);
            sProgram.SetUniformValue("modelMatrix", modelMatrix);
            sProgram.SetUniformValue("normalMatrix", normalMatrix);
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
                sProgram.SetUniformValue("allLights[" + i + "].Id", luces[i].Idiffuse);
                sProgram.SetUniformValue("allLights[" + i + "].Is", luces[i].Ispecular);
                sProgram.SetUniformValue("allLights[" + i + "].coneAngle", luces[i].ConeAngle);
                sProgram.SetUniformValue("allLights[" + i + "].coneDirection", luces[i].ConeDirection);
                sProgram.SetUniformValue("allLights[" + i + "].enabled", luces[i].Enabled);
                sProgram.SetUniformValue("allLights[" + i + "].direccional", luces[i].Direccional);
            }
            
            //Configuracion de las transformaciones del objeto a espacio de mundo
            //Transform transform = new Transform();
            //transform.Position = new Vector3(-1,-1,-1);
            //transform.Rotate(new Vector3(3.14f / 2f, 0f ,0f));
            //transform.Rotation = Quaternion.FromAxisAngle(Vector3.UnitX, 3.14f/2f);
            //foreach(Mesh m in objeto.Meshes)
            //m.Transform = transform;

            //Dibujamos el Objeto
            objeto.Dibujar(sProgram, mvMatrix);
            if (toggleNormals) objeto.DibujarNormales(sProgram, mvMatrix);

            //transform.Rotate(new Vector3(3.14f / 2f, 0f ,0f));
            //transform.Translate(new Vector3(1,1,1));
            //foreach(Mesh m in objeto.Meshes)
              //m.Transform = transform;

            //Dibujamos el objeto
            //objeto.Dibujar(sProgram, mvMatrix);
            //if (toggleNormals) objeto.DibujarNormales(sProgram, mvMatrix);
            
            sProgram.Deactivate(); //Desactivamos el programa de shader.

            //ejes_locales.Dibujar(sProgramUnlit);
            //Console.WriteLine("Camera Position: "+ myCamera.getPosition().ToString());

            glControl3.SwapBuffers(); //Intercambiamos buffers frontal y trasero, para evitar flickering.
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

        private void glControl3_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            e.IsInputKey = true;
        }

        private void glControl3_KeyPressed(object sender, KeyEventArgs e)
        {
            if (transformaciones == 1) transformaciones = 0;
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.S:
                    myCamera.Abajo();
                    break;

                case Keys.Up:
                case Keys.W:
                    myCamera.Arriba();
                    break;

                
                case Keys.Q:
                    //myCamera.Abajo2();
                    break;

                
                case Keys.E:
                    //myCamera.Arriba2();
                    break;

                case Keys.Right:
                case Keys.D:
                    myCamera.Derecha();
                    break;

                case Keys.Left:
                case Keys.A:
                    myCamera.Izquierda();
                    break;

                case Keys.Add:
                case Keys.I:
                    myCamera.Acercar(0.05f);
                    break;

                case Keys.Subtract:
                case Keys.O:
                    myCamera.Alejar(0.05f);
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

                //ON/OFF LUCES
                case Keys.D0:
                case Keys.NumPad0:
                    ToggleLight(0);
                    break;
                case Keys.NumPad1:
                case Keys.D1:
                    ToggleLight(1);
                    break;
                case Keys.NumPad2:
                case Keys.D2:
                    ToggleLight(2);
                    break;
                case Keys.NumPad3:
                case Keys.D3:
                    ToggleLight(3);
                    break;
                case Keys.D4:
                case Keys.NumPad4:
                    ToggleLight(4);
                    break;
                case Keys.D5:
                case Keys.NumPad5:
                    ToggleLight(5);
                    break;
                case Keys.D6:
                case Keys.NumPad6:
                    ToggleLight(6);
                    break;
                case Keys.D7:
                case Keys.NumPad7:
                    ToggleLight(7);
                    break;
                case Keys.D8:
                case Keys.NumPad8:
                    ToggleLight(8);
                    break;
                case Keys.D9:
                case Keys.NumPad9:
                    ToggleLight(9);
                    break;
                //TAMAÑO CONO LUZ_1
                case Keys.N:
                    ModificarCono(-2.0f);
                    break;
                case Keys.M:
                    ModificarCono(2.0f);
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
            }
            glControl3.Invalidate(); //Notar que renderizamos para CUALQUIER tecla que sea presionada.
            //Actualizar la info de debugeo
            updateDebugInfo();
        }

        private void ToggleLight(int i)
        {
            if (i < luces.Length)
            {
                luces[i].Toggle();
            }
        }
        private void ModificarCono(float deltaGrados)
        {
            if (luces.Length >= 1 && (luces[0].Enabled == 1))
            {
                float coneAngle = luces[0].ConeAngle;
                coneAngle = coneAngle + deltaGrados;
                if (coneAngle < 2.0f)
                {
                    coneAngle = 2.0f;
                }
                if (coneAngle > 45.0f)
                {
                    coneAngle = 45.0f;
                }
                luces[0].ConeAngle = coneAngle;
            }
        }

        private void SetupShaders()
        {
            //===== SHADER PARA LOS EJES =====
            //1. Creamos los shaders, a partir de archivos.
            String vShaderFile = "files/shaders/vunlit.glsl";
            String fShaderFile = "files/shaders/funlit.glsl";
            Shader vShader = new Shader(ShaderType.VertexShader, vShaderFile);
            Shader fShader = new Shader(ShaderType.FragmentShader, fShaderFile);
            //2. Los compilamos
            vShader.Compile();
            fShader.Compile();
            //3. Creamos el Programa de shader con ambos.
            sProgramUnlit = new ShaderProgram();
            sProgramUnlit.AddShader(vShader);
            sProgramUnlit.AddShader(fShader);
            //4. Construimos (linkeamos) el programa.
            sProgramUnlit.Build();
            //5. Ya podemos eliminar los shaders compilados. (Si no los vamos a usar en otro programa)
            vShader.Delete();
            fShader.Delete();

            //===== SHADER DE LUCES =====
            //1. Creamos los shaders, a partir de archivos.
            //vShaderFile = "files/shaders/vmultiplesluces.glsl";
            //fShaderFile = "files/shaders/fmultiplesluces.glsl";
            vShaderFile = "files/shaders/vbumpedspecularphong.glsl";
            fShaderFile = "files/shaders/fbumpedspecularphong.glsl";
            vShader = new Shader(ShaderType.VertexShader, vShaderFile);
            fShader = new Shader(ShaderType.FragmentShader, fShaderFile);
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


    }
}
