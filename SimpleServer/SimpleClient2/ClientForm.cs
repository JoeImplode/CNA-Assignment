using System;
using System.Windows.Forms;

namespace SimpleClient2
{
    public partial class ClientForm : Form
    {
        delegate void UpdateChatWindowDelegate(string message);
        UpdateChatWindowDelegate _updateChatWindowDelegate;

        public SimpleClient.SimpleClient _Client;

        public ClientForm(SimpleClient.SimpleClient client)
        {
            InitializeComponent();
            _updateChatWindowDelegate = new UpdateChatWindowDelegate(UpdateChatWindow);
            _Client = client;
            InputMessage.Select();
        }

        public void UpdateChatWindow(string messageRecieved)
        {
            try
            {
                if (Chat.InvokeRequired)
                {
                    Invoke(_updateChatWindowDelegate, messageRecieved);
                }
                else
                {
                    Chat.Text += messageRecieved += "\n";
                    Chat.SelectionStart = Chat.Text.Length;
                    Chat.ScrollToCaret();
                }
            }
            catch (System.InvalidOperationException e)
            {
                Console.WriteLine (e.Message);
            }
        }
        private void RichTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void Button1_Click(object sender, EventArgs e)
        {
            _Client.CreateMessage(InputMessage.Text);
            InputMessage.Clear();
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            _Client.Run();
        }
        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _Client.Stop();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            _Client.CreateNickName(InputMessage.Text);
            InputMessage.Clear();
        }
        private void InputMessage_TextChanged(object sender, EventArgs e)
        {

        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
