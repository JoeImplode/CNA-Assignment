using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleClient2
{
    public partial class NoughtsAndCrosses : Form
    {

        public SimpleClient.SimpleClient Client;
        public NoughtsAndCrosses(SimpleClient.SimpleClient client)
        {
            InitializeComponent();
            Client = client;
            
        }
        public void button1_Click(object sender, EventArgs e) { Client.UpdateMatrix(0, 2,Client.gameSender); }

        public void button8_Click(object sender, EventArgs e) { Client.UpdateMatrix(0, 0, Client.gameSender); }

        public void Button9_Click(object sender, EventArgs e) { Client.UpdateMatrix(1, 0,Client.gameSender); }

        public void Button10_Click(object sender, EventArgs e){ Client.UpdateMatrix(2, 0, Client.gameSender); }

        public void Button4_Click(object sender, EventArgs e) { Client.UpdateMatrix(0, 1, Client.gameSender); }

        public void Button6_Click(object sender, EventArgs e) { Client.UpdateMatrix(1, 1, Client.gameSender); }

        public void Button7_Click(object sender, EventArgs e) { Client.UpdateMatrix(2, 1, Client.gameSender); }

        public void Button2_Click(object sender, EventArgs e) { Client.UpdateMatrix(1, 2, Client.gameSender); }

        public void Button3_Click(object sender, EventArgs e) { Client.UpdateMatrix(2, 2, Client.gameSender); }
    }
}
