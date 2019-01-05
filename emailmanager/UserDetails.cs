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
    public partial class UserDetails : Form
    {
        public UserDetails()
        {
            InitializeComponent();
        }

        public Form1 Main { get; internal set; }
        string _usr;
        public string User { get
            {
                return _usr;
            }
            set {
                textBox1.Text = value;
                textBox1.ReadOnly = true;
                _usr = value; } }

        private void button4_Click(object sender, EventArgs e)
        {
            if(_usr!=null)  //edit
            {
                Main.SetPwd(textBox1.Text, textBox3.Text);
            }
            else //add
            {
                Main.Add(textBox1.Text, textBox3.Text);
            }
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
