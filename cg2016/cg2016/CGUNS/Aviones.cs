using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IrrKlang;
using CGUNS.Meshes;
using CGUNS.Shaders;
using OpenTK;
using CGUNS.Primitives;
using CGUNS.Particles;

namespace CGUNS
{

    class Aviones
    {
        private static float pi = 3.14f;
        private static Matrix4 escala = Matrix4.CreateScale(1f) * Matrix4.CreateRotationY(pi / 2);

        public ObjetoGrafico[] objetos;
        private ISound[] sonidoAviones; //uno para cada uno por si se quiere usar distintos o en momentos diferentes

        //Variables utilizadas para los aviones con animacion en linea recta
        private static Vector3 origen = new Vector3(-75.0f, 5.0f, 0.0f); //de donde sale el avion y fin hasta donde llega
        private Vector3 fin = new Vector3(75.0f, 2.0f, 0.0f);
        private Vector3 posicion = origen;      //posicion del avion delantero
        private Vector3 desplazamiento1 = new Vector3(-2.0f, 0.0f, 4.0f) ; //utilizados para los otros dos aviones
        private Vector3 desplazamiento2 = new Vector3(-2.0f, 0.0f, -4.0f);

        //las variables que siguen son para simular un avion persiguiendo a otro
        private Vector3 centroPerseguido = new Vector3(0.0f, 140.0f, 0.0f)/10;
        private static float radio=12f;
        private Vector3 posPerseguido, posPerseguidor;
        private static float retraso = pi/ 6; //Se utiliza para posicionar a un avion tanto mas atras que otro
                
        private Boolean disparar = true; //rotar y disparar se ponen en true para habilitar los disparos y el rol en la persecucion
        private Boolean rotar = true;
        private float rotX = 0f; //usado en el avion perseguido para girar sobre si mismo
        private static int MAXDisparos = 30; //maxima cantidad de disparon que se van a dibujar
        
        //las siguientes listas se usan para guardar informacion sobre los disparos (posicion, tiempo y particulas involucradas)
        List<Vector3> posicionesDisparos = new List<Vector3>();
        List<Smoke> list = new List<Smoke>();
        List<double> tiempoInicios = new List<double>(); //Se guarda en que momento comienzan los disparos en cada posicion
        int texturaDisparos;

        Cube cubo = new Cube(0.04f, 0.04f, 0.04f);

        //Las siguientes variables se usan para determinar el inicio de las animaciones y cada cuanto tiempo se actualiza el valor de inicio
        private float inicioRectos = 0;
        private float inicioPersecucion = 0;
        private static float esperaRectos = 25;  //los aviones tardan aprox. 20 unidades de timeSinceStartup desde que salen hasta llegar a su punto final. Tiene que ser mayor a ese numero
        private static float esperaPersecucion = 30f; //tardan 12,56 en dar la vuelta entera. Tiene que ser mayor a ese numero

        //Booleanos usados para no realizar las cuentas ni dibujar los aviones en los momentos de espera
        private Boolean dibRectos;
        private Boolean dibPersecucion;


        /// <summary>
        /// Se crean los objetos e inicializan las posiciones y sonidos
        /// </summary>
        /// <returns></returns>
        public Aviones(ShaderProgram sProgram1, ShaderProgram sProgram2, ISoundEngine engine, ShaderProgram sProgramUnlit, int texturaDisparos, int cantAviones = 5)
        {
            int cantObj = 3;            
            objetos = new ObjetoGrafico[cantObj];
            sonidoAviones = new ISound[cantAviones];
            this.texturaDisparos = texturaDisparos;

            //Se calcula y setea la posicion inicial de todos los aviones
            posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(pi), radio * (float)Math.Sin(pi), 0);
            posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(pi - retraso), radio * (float)Math.Sin(pi - retraso), 0);


            //Se coloca sonido a cada uno de los aviones. 0-3 son los que se mueven en linea recta. 3ro es perseguido
            Vector3D origen3D = new Vector3D(origen.X, origen.Y, origen.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            String pathSonido = "files/audio/plane_engine.ogg";
            sonidoAviones[0] = engine.Play3D(pathSonido, origen3D.X, origen3D.Y, origen3D.Z, true);
            sonidoAviones[1] = engine.Play3D(pathSonido, desp1.X, desp1.Y, desp1.Z, true);
            sonidoAviones[2] = engine.Play3D(pathSonido, desp2.X, desp2.Y, desp2.Z, true);
            sonidoAviones[3] = engine.Play3D(pathSonido, posPerseguido.X, posPerseguido.Y, posPerseguido.Z, true);
            sonidoAviones[4] = engine.Play3D(pathSonido, posPerseguidor.X, posPerseguidor.Y, posPerseguidor.Z, true);

            for (int i=0; i<sonidoAviones.Length; i++)
                sonidoAviones[i].Volume = sonidoAviones[i].Volume;

            cubo.Build(sProgramUnlit);

             dibRectos = inicioRectos == 0;
             dibPersecucion = inicioPersecucion==0;

    }

        /// <summary>
        /// Se dibujan tanto los aviones en linea recta como los de la persecucion. Se dibujan solo si corresponde de acuerdo al tiempo actual de la animacion
        /// </summary>
        /// <returns></returns>
        public void Dibujar(ShaderProgram sProgram, ShaderProgram sProgramParticles, Double timeSinceStartup )
        {
            

            if (dibRectos)
            {   
                objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion);
                objetos[0].Dibujar(sProgram);

                objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento1);
                objetos[0].Dibujar(sProgram);

                objetos[0].transform.localToWorld = escala * Matrix4.CreateTranslation(posicion + desplazamiento2);
                objetos[0].Dibujar(sProgram);
            }

            //El 3ro es el perseguido, el 4to el que lo sigue
            if (dibPersecucion)
            {
                //al comenzar (en el tiempo igual a InicioRectos), el angulo tiene que ser pi
                float angulo = ((float)timeSinceStartup - inicioPersecucion) / 2;
                float aumento = 0.3f; //usado para que en determinado momento el de atras aumente la rotacion y apunte al otro avion

                objetos[1].transform.localToWorld = escala * Matrix4.CreateRotationX(rotX) * Matrix4.CreateRotationZ(pi + angulo) * Matrix4.CreateTranslation(posPerseguido);
                objetos[2].transform.localToWorld = escala * Matrix4.CreateRotationZ(pi + angulo - retraso + aumento) * Matrix4.CreateTranslation(posPerseguidor);

                for (int i = 1; i < 3; i++)
                {
                    objetos[i].Dibujar(sProgram);
                }
            }

        }




        public void DibujarCuadradosDisparos(ShaderProgram sProgramUnlit)
        {
            for (int i = 0; i < posicionesDisparos.Count; i++)
            {
                sProgramUnlit.SetUniformValue("modelMatrix", Matrix4.CreateTranslation(posicionesDisparos[i]));
                sProgramUnlit.SetUniformValue("figureColor", new Vector4(1f, 0, 0, 1));
                cubo.Dibujar(sProgramUnlit);
            }
        }

        /// <summary>
        ///Se dibujan los disparos si corresponde
        /// </summary>
        /// <returns></returns>
        public void DibujarDisparos(ShaderProgram sProgramParticles)
        {
            if (dibPersecucion)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].enabled)
                        list[i].Dibujar(sProgramParticles);

                }
            }
        }

        /// <summary>
        /// Se actualiza la posicion de los aviones y disparos si hace falta, además de los sonidos
        /// </summary>
        /// <returns></returns>
        public void Actualizar(Double timeSinceStartup, ShaderProgram sProgramParticles ) {

            //Se calcula la nueva posicion del 1ro que va en linea recta. En Dibujar se actualiza la posicion de los 3 aviones
            if (timeSinceStartup - inicioRectos > esperaRectos)
                 { //paso el tiempo de espera. Se comienza a dibujar los aviones desde el principio
                   inicioRectos = inicioRectos + esperaRectos;
                   dibRectos = true; }
            float blend = ((float)timeSinceStartup - inicioRectos) * 0.05f;// % 1;
            if (blend <= 1)
                posicion = Vector3.Lerp(origen, fin, blend);
            else dibRectos = false;


            //Actualizacion de la persecucion
            if (timeSinceStartup - inicioPersecucion > esperaPersecucion)
            {
                inicioPersecucion = inicioPersecucion + esperaPersecucion;
                dibPersecucion = true;
                rotar = true;
                disparar = true;
            }

            double avance = (timeSinceStartup - inicioPersecucion) / 2;
            if (avance > 2 * pi)
            {
                dibPersecucion = false; //significa que ya dio la vuelta entera. Se deja de dibujar hasta pasar el tiempo de espera
                list = new List<Smoke>();
                tiempoInicios = new List<double>();
                posicionesDisparos = new List<Vector3>(); //Se crean las listas nuevas. Es el equivalente a borrar los elementos que tenian todas. 
                                    // Quedan listas para los proximos disparos, en la posicion y tiempo que sea 
            }
            else {  //todavia no dio la vuelta. Se sigue calculando y dibujando
                double angulo = pi / 2 + avance;

                posPerseguido = centroPerseguido + new Vector3(radio * (float)Math.Cos(angulo), radio * (float)Math.Sin(angulo), 0);
                posPerseguidor = centroPerseguido + new Vector3(radio * (float)Math.Cos(angulo - retraso), radio * (float)Math.Sin(angulo - retraso), 0);


                //Solo dispara y rola cuando esta en la region del principio del semicirculo de abajo
                if (disparar)
                    if (angulo % (pi * 2) > 4.0f && angulo % (pi * 2) < 4.6f && posicionesDisparos.Count < MAXDisparos)
                    {
                        Disparar(sProgramParticles, timeSinceStartup - inicioPersecucion);
                    }

                if (rotar)
                {
                    if (angulo % (pi * 2) > 3.8f)
                        rotX += 0.1f;    //usada para rotar el avion de adelante
                    if (rotX > (pi * 2))
                    {
                        rotX = 0;      //vuelve a la posicion original despues de dar 2 vueltas
                        rotar = false;
                    }
                }

                //Se actualizan los disparos si es necesario            
                for (int i = 0; i < list.Count; i++)
                {
                    if (timeSinceStartup - inicioPersecucion - tiempoInicios[i] > 0.15)
                        list[i].enabled = false;
                    else list[i].Update();
                }

            }
            //Actualizacion de la posicion de los sonidos
            Vector3D origen3D = new Vector3D(posicion.X, posicion.Y, posicion.Z);
            Vector3D desp1 = origen3D + new Vector3D(desplazamiento1.X, desplazamiento1.Y, desplazamiento1.Z);
            Vector3D desp2 = origen3D + new Vector3D(desplazamiento2.X, desplazamiento2.Y, desplazamiento2.Z);

            sonidoAviones[0].Position = origen3D;
            sonidoAviones[1].Position = desp1;
            sonidoAviones[2].Position = desp2;
            sonidoAviones[3].Position = new Vector3D(posPerseguido.X, posPerseguido.Y, posPerseguido.Z);
            sonidoAviones[4].Position = new Vector3D(posPerseguidor.X, posPerseguidor.Y, posPerseguidor.Z);
            
        }


        /// <summary>
        /// Crea un nuevo sistema de particulas que representa un disparo de un avion a otro. Se utiliza la posicion de estos ultimos para calcular la ubicacion de los disparos 
        /// </summary>
        /// <returns></returns>
        private void Disparar(ShaderProgram sProgramParticles, double timeSinceStartup)
        {   //origen es perseguidor. Dir es perseguido
            Vector3 pendiente = posPerseguido - posPerseguidor;
            //recta se expresa como origen+ t*pendiente. Calculo para que valor de t va a dar y=0 (q punto esta dentro del plano)
            // 0 = rayOrigin.Y + t * pendiente.Y            

            float t = -posPerseguidor.Y / pendiente.Y;
            Vector3 posDisparo = posPerseguidor + t * pendiente; 
            //posDisparo.Y += 0.1f;

            if (posicionesDisparos.Count > 0)
                {//Se agrega un control para que haya cierta separacion entre disparos y no sean infinitos
                if (Math.Abs(posicionesDisparos[(posicionesDisparos.Count - 1)].X - posDisparo.X) > 0.1f)
                    posicionesDisparos.Add(posDisparo);
                }
            else posicionesDisparos.Add(posDisparo);

            Vector3 cor = new Vector3(-3, 0, 0);
            Smoke smokeParticles = new Smoke(posDisparo+cor,2);
            smokeParticles.Texture = texturaDisparos;
            smokeParticles.Build(sProgramParticles);
            list.Add(smokeParticles);
            tiempoInicios.Add(timeSinceStartup);
        }

        public ObjetoGrafico[] getAviones()
        {
            return objetos;
        }
    }
}
