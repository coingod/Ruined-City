using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cg2016
{
    public partial class Splash : Form
    {
        private MainGameWindow gameWindow;

        public Splash(MainGameWindow mw)
        {
            InitializeComponent();
            gameWindow = mw;
            progressBar1.Maximum = 100;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //progressBar1.Increment(2);
            if (progressBar1.Value >= 100)
            {
                timer1.Stop();
                Dispose();
            }
            progressBar1.Value = gameWindow.UpdateLoadScreen();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
