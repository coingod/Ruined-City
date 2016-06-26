using System;
using System.Windows.Forms;
using System.Threading;

namespace cg2016
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MapTestScene());
            //Application.Run(new MainWindow());
            //Application.Run(new MainSupercube());
            Thread t = new Thread(new ThreadStart(SplashStart));
            t.Start();
            Thread.Sleep(5000);
            using (MainGameWindow mw = new MainGameWindow())
            {
                mw.Run();
            }


            t.Abort();


        }

        private static void SplashStart()
        {
            Application.Run(new Splash());

        }
    }
}
