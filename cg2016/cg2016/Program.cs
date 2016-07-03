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
            using (MainGameWindow mw = new MainGameWindow())
            {
                mw.Run();
            }
        }

    }
}
