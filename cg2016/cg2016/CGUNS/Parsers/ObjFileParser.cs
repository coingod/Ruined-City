using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using CGUNS.Meshes.FaceVertexList;
using OpenTK;

namespace CGUNS.Parsers
{
    public class ObjFileParser
    {
        private const String VERTEX = "v";
        private const String FACE = "f";
        private const String NORMAL = "vn";
        private const String TEXCORD = "vt";
        private const Char COMMENT = '#';
        private const String OBJETO = "o";
        private const String GRUPO = "g";
        private static Char[] SEPARATORS = { ' ', '/' };
        private static bool logEnabled = false;


        public static List<FVLMesh> parseFile(String fileName)
        {
            FVLMesh mesh = new FVLMesh();//mesh actual
            int iMesh = 0;
            List<FVLMesh> objetos = new List<FVLMesh>();
            int vertexOffset = 0;
            int normalOffset = 0;
            int textureOffset = 0;

            objetos.Add(mesh);

            String line;
            String[] lineSplit;
            String sender = "ObjFileParser.parseFile: ";
            info(sender, "Opening file: {0}", fileName);
            StreamReader file = new StreamReader(fileName);
            info(sender, "OK. Reading all the lines of the file...");


            line = file.ReadLine();
            while (line != null)
            {
                line = line.Trim(); //Saco espacios en blanco.
                if ((line.Length != 0) && (!line[0].Equals(COMMENT))) //Si no es comentario
                {
                    lineSplit = line.Split(SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
                    if (lineSplit[0].Equals(VERTEX))
                    {
                        parseVertex(objetos[iMesh], lineSplit);
                    }
                    else if (lineSplit[0].Equals(NORMAL))
                    {
                        parseNormal(objetos[iMesh], lineSplit);
                    }
                    else if (lineSplit[0].Equals(FACE))
                    {
                        parseFace(objetos[iMesh], line, vertexOffset, normalOffset, textureOffset);//HERE!!
                    }
                    else if (lineSplit[0].Equals(TEXCORD))
                    {
                        parseTexCord(objetos[iMesh], lineSplit);
                    }
                    else if (lineSplit[0].Equals(OBJETO) || lineSplit[0].Equals(GRUPO))
                    {
                        if (objetos[0].Name == "Mesh")
                            objetos[0].Name = lineSplit[1];
                        if (objetos[0].FaceCount > 0) //Esto para poder importar archivos con o sin declaracion de Objects
                        {
                            vertexOffset += mesh.VertexCount;
                            normalOffset += mesh.VertexNormalList.Count;
                            textureOffset += mesh.TexCordList.Count;

                            mesh = new FVLMesh(lineSplit[1]);
                            objetos.Add(mesh);
                            iMesh++;
                        }
                    }
                    else
                    {
                        log(sender, "Not supported instruction: {0}", lineSplit[0]);
                    }
                }
                line = file.ReadLine();
            }
            file.Close();
            info(sender, "FINISHED!");
            /*
            System.Windows.Forms.MessageBox.Show("El archivo '" + fileName + "' ha sido correctamente cargado."
            + "\n-------------------- "
            + "\nObject Count: " + objetos.Count
            +  "\nVertex Count: " + (vertexOffset != 0 ? vertexOffset : objetos[0].VertexCount)
            + "\nTexture Count: " + (textureOffset != 0 ? textureOffset : objetos[0].VertexNormalList.Count)
            + "\nNormal Count: " + (normalOffset != 0 ? normalOffset : objetos[0].TexCordList.Count)
            + "\n-------------------- "
            + "\nInput: F2 Toggle Normals - F3 Toggle Wireframe"
            , "ObjFileParser");
            */
            //List<FVLMesh> toRet = new List<FVLMesh>();
            //toRet.Add(objetos[0]);
            //toRet = objetos;
            return objetos;
        }

        public static void parseVertex(FVLMesh mesh, String[] args)
        {
            String sender = "ObjFileParser.parseVertex: ";
            Vector3 vertex = new Vector3();
            switch (args.Length)
            {
                case 2:
                    log(sender, "Vertex definition must be (x,y,z [,w]). Found only x.");
                    vertex.X = float.Parse(args[1], System.Globalization.NumberStyles.Number);
                    vertex.Y = 0.0f;
                    vertex.Z = 0.0f;
                    //vertex.W = 1.0f;
                    break;
                case 3:
                    log(sender, "Vertex definition must be (x,y,z [,w]). Found only x, y.");
                    vertex.X = float.Parse(args[1], CultureInfo.InvariantCulture);
                    vertex.Y = float.Parse(args[2], CultureInfo.InvariantCulture);
                    vertex.Z = 0.0f;
                    //vertex.W = 1.0f;
                    break;
                case 4:
                    vertex.X = float.Parse(args[1], CultureInfo.InvariantCulture);
                    vertex.Y = float.Parse(args[2], CultureInfo.InvariantCulture);
                    vertex.Z = float.Parse(args[3], CultureInfo.InvariantCulture);
                    //vertex.W = 1.0f;
                    break;
                case 5:
                    log(sender, "Found (x, y, z, w). Discarding w component.");
                    vertex.X = float.Parse(args[1], CultureInfo.InvariantCulture);
                    vertex.Y = float.Parse(args[2], CultureInfo.InvariantCulture);
                    vertex.Z = float.Parse(args[3], CultureInfo.InvariantCulture);
                    //vertex.W = float.Parse(args[4], System.Globalization.NumberStyles.Number);
                    break;
                default:
                    break;
            }
            mesh.AddVertex(vertex);
            //vertexCount++;
        }

        public static void parseNormal(FVLMesh mesh, String[] args)
        {
            Vector3 vertex = new Vector3();
            vertex.X = float.Parse(args[1], System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            vertex.Y = float.Parse(args[2], System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            vertex.Z = float.Parse(args[3], System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            mesh.AddVertexNormal(vertex);
        }

        public static void parseTexCord(FVLMesh mesh, String[] args)
        {
            Vector2 texCord = new Vector2();
            texCord.X = float.Parse(args[1], System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            texCord.Y = float.Parse(args[2], System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
            mesh.AddTexCord(texCord);
        }


        public static void parseFace(FVLMesh mesh, string line, int vertexOffset, int normalOffset, int textureOffset)
        {
            FVLFace face = new FVLFace();

            int i = 2; // componente 1 = f , comp 2 = ' '
            String vertex;
            String texCord;
            String normal;

            while (i < line.Length)
            {
                vertex = "";
                texCord = "";
                normal = "";
                while (i < line.Length && line[i] != ' ' && line[i] != '/')
                {
                    vertex = vertex + line[i];
                    i++;
                }

                if (i < line.Length && line[i] != ' ')
                {
                    i++;
                    if (line[i] != '/')
                    {
                        while (i < line.Length && line[i] != ' ' && line[i] != '/')
                        {
                            texCord = texCord + line[i];
                            i++;
                        }
                    }
                    i++;

                    if (i < line.Length && line[i] != '/')
                    {
                        while (i < line.Length && line[i] != ' ' && line[i] != '/')
                        {
                            normal = normal + line[i];
                            i++;
                        }
                    }
                }

                i++;
                face.AddVertex(Int32.Parse(vertex, NumberStyles.Integer) - 1 - vertexOffset);
                if (!normal.Equals(""))
                {
                    face.AddNormal(Int32.Parse(normal, NumberStyles.Integer) - 1 - normalOffset);
                }
                if (!texCord.Equals(""))
                {
                    face.AddTexCord(Int32.Parse(texCord, NumberStyles.Integer) - 1 - textureOffset);
                }
            }

            mesh.AddFace(face);
        }


        private static void log(String sender, String format, params Object[] args)
        {
            if (logEnabled)
            {
                System.Console.WriteLine(sender + format, args);
            }
        }
        private static void info(String sender, String format, params Object[] args)
        {
            System.Console.WriteLine(sender + format, args);
        }

    }
}
