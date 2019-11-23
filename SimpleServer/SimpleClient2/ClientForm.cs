using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SimpleClient2
{
    public partial class ClientForm : Form
    {
        delegate void UpdateChatWindowDelegate(string message);
        UpdateChatWindowDelegate _updateChatWindowDelegate;

        delegate void UpdateClientListBoxDelegate(List<string> clientListFromClient);
        UpdateClientListBoxDelegate _updateClientListBoxDeletage;

        public SimpleClient.SimpleClient _Client;

        public bool _canConnectToServer;
        public bool _canSetClientNickName;

        public string _clientNickName;

        public ClientForm(SimpleClient.SimpleClient client)
        {
            InitializeComponent();
            _updateChatWindowDelegate = new UpdateChatWindowDelegate(UpdateChatWindow);
            _updateClientListBoxDeletage = new UpdateClientListBoxDelegate(UpdateClientListBox);
            _Client = client;
            InputMessage.Select();
            _canConnectToServer = true;
            _canSetClientNickName = true;
        }

        public void UpdateClientListBox(List<string> clientListFromClient)
        {
            if(comboBox1.InvokeRequired)
            {
                Invoke(_updateClientListBoxDeletage, clientListFromClient);
            }
            else
            {
                comboBox1.Items.Clear();
                for (int i = 0; i < clientListFromClient.Count; i++)
                {
                    comboBox1.Items.Add(clientListFromClient[i]);
                }
            }
        }

        public void UpdateChatWindow(string messageRecieved)
        {
            try
            {
                if (chatBox.InvokeRequired)
                {
                    Invoke(_updateChatWindowDelegate, messageRecieved);
                }
                else
                {
                    chatBox.Text += messageRecieved += "\n";
                    chatBox.SelectionStart = chatBox.Text.Length;
                    chatBox.ScrollToCaret();
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
            if (_canConnectToServer == false)
            {
                _Client.CreateMessage(InputMessage.Text);
                InputMessage.Clear();
            }
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
            _clientNickName = InputMessage.Text;
        }
        private void InputMessage_TextChanged(object sender, EventArgs e)
        {

        }
        private void comboBox1_SelectedIndexChanged_2(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            _Client.CreateNickName(_clientNickName);
            _canConnectToServer = false;
            _canSetClientNickName = false;
            button2.Enabled = _canConnectToServer;
            button1.Enabled = _canSetClientNickName;
        }
    }
}
