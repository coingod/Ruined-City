using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using GLBoolean = OpenTK.Graphics.OpenGL.Boolean;

namespace CGUNS.Shaders
{
    /// <summary>
    /// Represents a Shader.
    /// A Shader is a small piece of code that runs on a programmable pipeline stage of the Graphics Card.
    /// Vertex and Fragment Shaders together conform a ShaderProgram.
    /// <remarks>Only Vertex and Fragment Shaders supported</remarks>
    /// </summary>
    public class Shader
    {
        private int shaderId;
        private ShaderType shaderType;
        private String shaderSource;

        internal int Id
        {
            get { return this.shaderId; }
        }

        public String Source
        {
            set { this.shaderSource = value; }
        }

        /// <summary>
        /// Creates a Shader of a given type.
        /// </summary>
        /// <param name="type">Shader type (Only Vertex or Fragment)</param>
        /// <exception cref="System.Exception">When other than ShaderType.VertexShader or ShaderType.FragmentShader is specified.</exception>
        /// <exception cref="System.Exception">If cannot create a OpenGL Shader Object.</exception>
        public Shader(ShaderType type)
        {
            if (type != ShaderType.VertexShader && type != ShaderType.FragmentShader)
            {
                throw new Exception("Shader type not supported!");
            }
            shaderType = type;
            shaderSource = null;
            //Tratamos de crear un objeto shader de OpenGL
            shaderId = gl.CreateShader(shaderType);
            if (shaderId == 0)
            { //Si no pudo, abortamos.
                //shaderId = -1;
                throw new Exception("Cannot create OpenGL Shader Object");
            }
            log("Sucessfully created [{0}] with ID: {1}", shaderType, shaderId);

        }

        /// <summary>
        /// Creates a Shader of a given type, and get the Source content from a file.
        /// </summary>
        /// <param name="type">Shader type (Only Vertex or Fragment)</param>
        /// <param name="fileName">File containing Shader source code.</param>
        /// <exception cref="System.Exception">When cannot access the file. See innerException for more info.</exception>
        /// <exception cref="System.Exception">If cannot create a OpenGL Shader Object.</exception>
        public Shader(ShaderType type, String fileName)
            : this(type)
        {
            try
            {
                //StringBuilder _shaderSource = new StringBuilder();
                ///String line;
                using (StreamReader stream = new StreamReader(fileName))
                {
                    shaderSource = stream.ReadToEnd();
                    /*
                    while (!stream.EndOfStream) {
                      line = stream.ReadLine();
                      if (!String.IsNullOrEmpty(line)) {

                        _shaderSource.AppendLine(line);
                      }
                    }
                     * */
                }
                //shaderSource = _shaderSource.ToString();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
        }

        /// <summary>
        /// Compiles a Shader
        /// </summary>
        /// <exception cref="CGUNS.Shaders.ShaderCompilationException">When Compilation fails.</exception>
        public void Compile()
        {
            if (String.IsNullOrEmpty(shaderSource))
            {
                String message = "Shader source code is null or empty.";
                throw new ShaderCompilationException(message);
            }
            if (shaderId == 0)
            {
                String message = "Invalid Shader (Was it successfully created?)";
                throw new ShaderCompilationException(message);
            }
            //Le seteamos el codigo fuente al objeto shader de OpenGL
            gl.ShaderSource(shaderId, shaderSource);
            gl.CompileShader(shaderId);
            TestShaderCompilation();
        }

        private void TestShaderCompilation()
        {
            GLBoolean compiledOK;
            int resul;
            String compilerMessage;

            gl.GetShader(shaderId, ShaderParameter.CompileStatus, out resul);

            compilerMessage = gl.GetShaderInfoLog(shaderId);
            compilerMessage = compilerMessage.TrimEnd('\r', '\n', ' '); //Le saco los enters del final.
            compiledOK = (GLBoolean)resul;

            if (compiledOK == GLBoolean.False)
            {
                throw new ShaderCompilationException(compilerMessage);
            }
            log(compilerMessage);
        }

        public void Delete()
        {
            if (shaderId != 0)
            {
                gl.DeleteShader(shaderId);
                shaderId = 0;
            }
            else
            {
                String message = "Invalid Shader. It may have been already deleted.";
                throw new Exception(message);
            }
        }

        private void log(String format, params Object[] args)
        {
            //Console.WriteLine("## [CGUNS]:" + format, args);
            System.Diagnostics.Debug.WriteLine(String.Format(format, args), "[CGUNS]");
        }
    }
}
