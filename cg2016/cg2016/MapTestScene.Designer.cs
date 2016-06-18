namespace cg2016
{
    partial class MapTestScene
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControl3 = new CGUNS.GLControl3();
            this.SuspendLayout();
            // 
            // glControl3
            // 
            this.glControl3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.glControl3.BackColor = System.Drawing.Color.Black;
            this.glControl3.Location = new System.Drawing.Point(16, 15);
            this.glControl3.Margin = new System.Windows.Forms.Padding(5);
            this.glControl3.Name = "glControl3";
            this.glControl3.Size = new System.Drawing.Size(347, 293);
            this.glControl3.TabIndex = 0;
            this.glControl3.VSync = false;
            this.glControl3.Load += new System.EventHandler(this.glControl3_Load);
            this.glControl3.Paint += new System.Windows.Forms.PaintEventHandler(this.glControl3_Paint);
            this.glControl3.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl3_KeyPressed);
            this.glControl3.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.glControl3_PreviewKeyDown);
            this.glControl3.Resize += new System.EventHandler(this.glControl3_Resize);
            this.glControl3.Click += new System.EventHandler(this.glControl3_Click);
            // 
            // MapTestScene
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 322);
            this.Controls.Add(this.glControl3);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MapTestScene";
            this.Text = "MapTestScene";
            this.ResumeLayout(false);

        }

        #endregion

        private CGUNS.GLControl3 glControl3;
    }
}

