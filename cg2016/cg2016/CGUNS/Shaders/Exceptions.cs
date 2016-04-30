using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CGUNS.Shaders
{
    /// <summary>
    /// Exception when Linking a ShaderProgram.
    /// </summary>
    public class ShaderProgramLinkageException : Exception
    {
        public ShaderProgramLinkageException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception when Compiling a Shader.
    /// </summary>
    public class ShaderCompilationException : Exception
    {
        public ShaderCompilationException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception on a ShaderProgram operation.
    /// </summary>
    public class ShaderProgramException : Exception
    {
        public ShaderProgramException(String message)
            : base(message)
        {
        }
    }
}
