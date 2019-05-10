using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JsonTools
{
    public partial class Form2 : Form
    {
        public string s = "";
        public int width = 0;
        public int height = 0;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Opacity = 0;
            TopMost = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Opacity = 1;
            timer2.Enabled = true;
        }
        int count = 0;
        private void timer2_Tick(object sender, EventArgs e)
        {
            count++;
            if (count >= 80)
            {

                this.Opacity -= 0.01;
                if (Opacity <= 0)
                {
                    //this.Close();
                    count = 0;
                    timer2.Enabled = false;
                }
            }
        }
        public void SetText(string s,Size sz,Point p)
        {
            if (Opacity != 0)
            {
                count = 0;
                timer2.Enabled = false;
                Opacity = 0;
            }
            label1.Text = s;
            this.Width = label1.Width + 40;
            this.Height = label1.Height + 40;
            label1.Left = this.Width / 2 - label1.Width / 2;
            label1.Top = this.Height / 2 - label1.Height / 2;
            width = this.Width;
            height = this.Height;

            this.Left = p.X + sz.Width / 2 - this.width / 2;
            this.Top = p.Y + sz.Height / 2 - this.height / 2;

            timer1.Enabled = true;
        }
    }
}
