using System;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using gl = OpenTK.Graphics.OpenGL.GL;
using GLBoolean = OpenTK.Graphics.OpenGL.Boolean;

namespace CGUNS.Shaders
{
    public class ShaderProgram
    {
        private int programHandle;
        public int ID
        {
            get
            {
                return programHandle;
            }
        }
        private List<Shader> shaders;
        //private Dictionary<String, int> programUniforms;
        private Dictionary<String, int> programAttributes;

        public ShaderProgram()
        {
            programHandle = gl.CreateProgram();
            if (programHandle == 0)
            {
                String message = "Cannot create OpenGL program object.";
                throw new Exception(message);
            }
            log("Successfully created Program with Handle: {0}", programHandle);
            shaders = new List<Shader>();
            //programUniforms = new Dictionary<string, int>();
            programAttributes = new Dictionary<string, int>();
        }

        public void AddShader(Shader shader)
        {
            if (shader != null)
            {
                shaders.Add(shader);
            }
        }

        #region GET/SET Vertex attribs
        /// <summary>
        /// Establece una determinada ubicación para un atributo de vértice (shader de vértices), antes de la llamada al método <c>Build</c>.
        /// No es obligatorio asignarlas. Si no se especifican las ubicaciones, OpenGl les asigna una arbitraria que se puede consultar (después de la llamada al método <c>Build</c>) con GetVertexAttribLocation.
        /// </summary>
        /// <param name="vertexAttribName">El nombre del atributo. (Como figura en el código del shader, p. ej: "vertexPos")</param>
        /// <param name="desiredLocation">La ubicación deseada.</param>
        public void SetDesiredVertexAttribLocation(String vertexAttribName, int desiredLocation)
        { //Los guardo en mi mapita para (antes de linkear) bindearselos a OpenGL
            //Si ya estaba, lo saco. (Si no estaba, no pasa nada)
            programAttributes.Remove(vertexAttribName);
            programAttributes[vertexAttribName] = desiredLocation;
            // Si ya hay un attrib con esa locacion? Ops!, no lo controlo.
        }

        public int GetVertexAttribLocation(String vertexAttribName)
        {
            int resul = -1;
            if (IsValidAttribute(vertexAttribName))
            {
                resul = programAttributes[vertexAttribName];
            }
            else
            {
                String message = "Shader program does not contain an attribute named: " + vertexAttribName;
                throw new ShaderProgramException(message);
            }
            return resul;
        }

        private bool IsValidAttribute(String attribName)
        {
            return programAttributes.ContainsKey(attribName);
        }
        #endregion GET/SET Vertex attribs

        #region Build the program

        public void Build()
        {
            if (programHandle == 0)
            {
                String message = "Invalid Program. (Was it successfully created?)";
                throw new Exception(message);
            }

            foreach (Shader shader in shaders)
            {
                //shader.Compile();
                gl.AttachShader(programHandle, shader.Id);
            }

            BindDesiredAttributeLocations();
            gl.LinkProgram(programHandle);
            TestProgramLinkage();
            //Creo mis tablitas de simbolos.
            //BuildUniformsMap();
            BuildAttributesMap();
            // Clean resources.
            foreach (Shader shader in shaders)
            {
                gl.DetachShader(programHandle, shader.Id);
                //shader.Delete();
            }
        }

        private void BindDesiredAttributeLocations()
        { // Si hay location particulares, las bindeo.
            foreach (String attribName in programAttributes.Keys)
            {
                int attribLocation = programAttributes[attribName];
                gl.BindAttribLocation(programHandle, attribLocation, attribName);
            }
            programAttributes.Clear(); //Después se reconstruye
        }

        private void TestProgramLinkage()
        {
            GLBoolean linkedOK;
            int resul;
            String linkerMessage;

            //gl.GetProgram(programHandle, ProgramParameter.LinkStatus, out resul);
            gl.GetProgram(programHandle, GetProgramParameterName.LinkStatus, out resul);

            linkerMessage = gl.GetProgramInfoLog(programHandle);
            linkedOK = (GLBoolean)resul;
            if (linkedOK == GLBoolean.False)
            {
                throw new ShaderProgramLinkageException(linkerMessage);
            }
            log(linkerMessage); //Should print success;
        }
        /*
        private void BuildUniformsMap()
        {
            int uniformCount;
            //gl.GetProgram(programHandle, ProgramParameter.ActiveUniforms, out uniformCount);
            gl.GetProgram(programHandle, GetProgramParameterName.ActiveUniforms, out uniformCount);

            for (int uniformIndex = 0; uniformIndex < uniformCount; uniformIndex++)
            {
                String uniformName;
                int uniformSize; //Usar esta info para validar el seteo de uniformes!!
                ActiveUniformType uniformType;
                uniformName = gl.GetActiveUniform(programHandle, uniformIndex, out uniformSize, out uniformType);

                //En la notebook, falla la busqueda de uniformes. (nombres vacios y repetidos)
                //Por eso la comparacion y el try ignore.
                if (!String.IsNullOrEmpty(uniformName))
                {
                    try
                    {
                        programUniforms.Add(uniformName, uniformIndex);
                    }
                    catch (Exception ignore) { }
                }

            }
        }*/

        private void BuildAttributesMap()
        {
            int attribCount;
            //gl.GetProgram(programHandle, ProgramParameter.ActiveAttributes, out attribCount);
            gl.GetProgram(programHandle, GetProgramParameterName.ActiveAttributes, out attribCount);
            String attribName;
            for (int attribIndex = 0; attribIndex < attribCount; attribIndex++)
            {
                int attribSize;
                ActiveAttribType attribType;
                attribName = gl.GetActiveAttrib(programHandle, attribIndex, out attribSize, out attribType);
                int index = gl.GetAttribLocation(programHandle, attribName);
                programAttributes.Add(attribName, index);
            }
        }
        #endregion Build the program

        public void Activate()
        {
            if (programHandle != 0)
            {
                gl.UseProgram(programHandle);
            }
        }

        public void Deactivate()
        {
            gl.UseProgram(0);
        }

        private void log(String format, params Object[] args)
        {
            System.Diagnostics.Debug.WriteLine(String.Format(format, args), "[CGUNS]");
        }


        #region SetUniform
        private bool ValidateUniform(String uniformName)
        {
            bool resul = true;
            int location = gl.GetUniformLocation(ID, uniformName);
            if (location == -1)
            {
                String message = String.Format("Shader program does not contain a uniform named: {0}", uniformName);
                throw new ShaderProgramException(message);
            }
            return resul;
        }

        public void SetUniformValue(String uniformName, int value)
        {
            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.Uniform1(location, value);
            }
        }

        public void SetUniformValue(String uniformName, float value)
        {
            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.Uniform1(location, value);
            }
        }

        public void SetUniformValue(String uniformName, Matrix4 value)
        {
            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.UniformMatrix4(location, false, ref value);
            }
        }

        public void SetUniformValue(String uniformName, Matrix3 value)
        {
            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.UniformMatrix3(location, false, ref value);
            }
        }

        public void SetUniformValue(String uniformName, Vector4 value)
        {
            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.Uniform4(location, value);
            }
        }

        public void SetUniformValue(String uniformName, Vector3 value)
        {

            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.Uniform3(location, value);
            }
        }

        public void SetUniformValue(String uniformName, Vector2 value)
        {
            if (ValidateUniform(uniformName))
            {
                int location = gl.GetUniformLocation(ID, uniformName);
                gl.Uniform2(location, value);
            }
        }

        #endregion SetUniform
    }
}
