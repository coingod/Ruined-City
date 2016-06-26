using System;
using System.Windows.Forms;
using System.Threading;

namespace cg2016
{
    static class Program
    {
        static MainGameWindow mw;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mw = new MainGameWindow();

            Thread t = new Thread(new ThreadStart(SplashStart));
            t.Start();
            //Thread.Sleep(5000);
            mw.Run();
            //t.Abort();
        }

        private static void SplashStart()
        {
            Application.Run(new Splash(mw));

        }
    }
}
