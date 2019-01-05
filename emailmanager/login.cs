using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace emailmanager
{
    public partial class login : Form
    {
        public login()
        {
            InitializeComponent();
        }

        public Form1 Main { get; internal set; }

        private void button1_Click(object sender, EventArgs e)
        {
            Main.SetRoot(textBox1.Text, textBox2.Text);
            Main.List();
            this.Hide();
            Main.Show();
        }
    }
}
