using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace emailmanager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            m_cmds = new List<string>();
            recv = new Thread(new ThreadStart(Work));
            work = new Thread(new ThreadStart(receiver));
            recv.Start();
            work.Start();
        }

        internal void List()
        {
            lock (this)
            {
                m_cmds.Add("listusers\n");
            }
        }

        string root;
        string rootpwd;
        internal void SetRoot(string text1, string text2)
        {
            root = text1;
            rootpwd = text2;
            if (comstatus != 0)
            {
                clientSocket.Close();
                lock(this)
                {
                    m_cmds.Clear();
                }
            }
        }

        Thread recv;
        Thread work;
        class LocalAccount
        {
            public string Account { set; get; }
            public string Name { set; get; }
            public override string ToString()
            {
                return string.Format("{0}({1})", Account, Name);
            }
        }
        List<LocalAccount> localaccounts;

        List<LocalAccount> accounts;

        AutoResetEvent myResetEvent;

        internal void SetPwd(string text1, string text2)
        {
            lock (this)
            {
                m_cmds.Add(string.Format( "setpassword {0} {1}\n", text1, text2));
            }
        }

        internal void Add(string text1, string text2)
        {
            lock (this)
            {
                m_cmds.Add(string.Format("adduser {0} {1}\n", text1, text2));
            }
        }
        bool firstlist = false;

        int comstatus = 0;
        Socket clientSocket;
        void receiver()
        {
            int port = 4555;


            IPAddress[] ip = Dns.GetHostAddresses("mail.puresprite.com");
            IPEndPoint ipe = new IPEndPoint(ip[0], port);

            while(true)
            {
                if(IsDisposed)
                {
                    break;
                }

                if (!needlogin)
                {
                    Thread.Sleep(200);
                    continue;
                }

                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    comstatus = 1;
                    clientSocket.Connect(ipe);
                    comstatus = 2;
                }
                catch
                {
                    comstatus = 0;
                    continue;
                }

                //send message
               

                //receive message
                string recStr = "";
                byte[] recBytes = new byte[4096];
                int bytes = 0;
                while (true)
                {
                    if (IsDisposed)
                        break;
                    try
                    {
                        bytes = clientSocket.Receive(recBytes, recBytes.Length, 0);
                    }
                    catch {
                        bytes = 0;
                    }

                    if (bytes == 0)
                    {
                        comstatus = 0;
                        break;
                    }
                    string rcv = Encoding.ASCII.GetString(recBytes, 0, bytes);

                    if (rcv.Contains("Password:"))
                    {
                        comstatus = 4;
                    } else if (rcv.Contains("Welcome"))
                    {
                        comstatus = 6;
                        bisready = true;
                    } else if (rcv.Contains("added"))
                    {
                        string uname = "";
                        string[] sts = rcv.Split(' ');
                        if (sts.Length > 2) uname = sts[1];
                        MessageBox.Show("用户添加成功!"+ uname);

                    } else if (rcv.Contains("already"))
                    {
                        string uname = "";
                        string[] sts = rcv.Split(' ');
                        if (sts.Length > 2) uname = sts[1];


                        MessageBox.Show("用户添加失败!"+ uname);
                    }else if (rcv.Contains("Password for"))
                    {
                        MessageBox.Show("密码修改成功!");
                    }else if(rcv.Contains("Existing accounts")){
                       

                        string[] usrs = rcv.Split(':');
                        List<string> usrsl = new List<string>();
                        for(int i=1;i< usrs.Length; ++i)
                        {
                            if(i== usrs.Length-1)
                            {
                                usrsl.Add(usrs[i].Trim());
                            }
                            else
                            {
                                if(usrs[i].Length>4&& usrs[i].Substring(usrs[i].Length-4,4)=="user")
                                {
                                    usrsl.Add(usrs[i].Substring(0, usrs[i].Length-4).Trim());
                                }
                            }
                        }

                       this.Invoke(new EventHandler((object o, EventArgs ea) => {
                           this.listBox1.DataSource = usrsl;
                           if (!firstlist)
                           {
                               firstlist = true;
                               button1.Enabled = true;
                               button2.Enabled = true;
                           }

                       }));


                    }
                    switch (comstatus)
                    {
                        case 2:
                            clientSocket.Send(Encoding.ASCII.GetBytes(root+"\n"));
                            comstatus = 3;
                            break;
                        case 4:
                            clientSocket.Send(Encoding.ASCII.GetBytes(rootpwd+"\n"));
                            comstatus = 5;
                            break;
                        default:
                            break;
                    }

                    this.Invoke(new EventHandler((object o,EventArgs ea) => {
                        this.richTextBox1.Text = Encoding.UTF8.GetString(recBytes, 0, bytes) + this.richTextBox1.Text;
                    }));
                }
                
            }
            
        }
        bool bisready = false;

        bool ready
        {
            get
            {
                return bisready;
            }
        }
        bool needlogin;

        void dologin()
        {
            needlogin = true;
        }
        List<string> m_cmds;
        void Work()
        {
            myResetEvent = new AutoResetEvent(false);
            while (true)
            {
                List<string> cmds = null;

                if(this.IsDisposed)
                {
                    break;
                }
                
                lock (this)
                {
                    if(m_cmds.Count>0)
                    {
                        if (ready)
                        {
                            cmds = m_cmds;
                            m_cmds = new List<string>();
                        }
                        else
                        {
                            dologin();
                            Thread.Sleep(100);
                            continue;
                        }
                    }else
                    {
                        needlogin = false;
                    }
                }
                if (cmds == null)
                {
                    Thread.Sleep(200);
                    continue;
                }
                
                foreach(var i in cmds)
                {
                    clientSocket.Send(Encoding.ASCII.GetBytes(i));
                    Thread.Sleep(500);
                }

            }

            if(comstatus!=0)
            {
                clientSocket.Close();
            }
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(listBox1.SelectedItem!=null)
            {
                var u = new UserDetails();
                u.Main = this;
                u.User = listBox1.SelectedItem.ToString();
                u.ShowDialog();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var u = new UserDetails();
            u.Main = this;
            u.ShowDialog();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lock(this)
            {
                m_cmds.Add("listusers\n");
            }
        }
        login lg;
        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            lg = new login();
            lg.Main = this;
            this.Hide();
            lg.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.lg.ShowDialog();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
