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
        public int _clientListSelection;

        public ClientForm(SimpleClient.SimpleClient client)
        {
            InitializeComponent();
            _updateChatWindowDelegate = new UpdateChatWindowDelegate(UpdateChatWindow);
            _updateClientListBoxDeletage = new UpdateClientListBoxDelegate(UpdateClientListBox);
            _Client = client;
            InputMessage.Select();
            _canConnectToServer = false;
            _canSetClientNickName = true;
            button2.Enabled = _canConnectToServer;
            _clientListSelection = 0;
            comboBox1.Enabled = false;
        }

        public void UpdateClientListBox(List<string> clientListFromClient)
        {
            if (comboBox1.InvokeRequired)
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
                comboBox1.SelectedIndex = 0;
                _canConnectToServer = true;
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
        private void OutputTextBoxChanged(object sender, EventArgs e)
        {

        }
        private void SendButtonPressed(object sender, EventArgs e)
        {
            if (_canConnectToServer == true)
            {
                if (_clientListSelection == 0)
                {
                    _Client.CreateMessage(InputMessage.Text);
                    InputMessage.Clear();
                }
                else
                {
                    _Client.CreateMessage(InputMessage.Text, _clientListSelection);
                }
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

        private void NickNameButtonPressed(object sender, EventArgs e)
        {
            _clientNickName = InputMessage.Text;
            _canConnectToServer = true;
            button2.Enabled = _canConnectToServer; 
        }
        private void InputMessageBoxChanged(object sender, EventArgs e)
        {

        }
        private void ComboBoxChanged(object sender, EventArgs e)
        {
            _clientListSelection = comboBox1.SelectedIndex;
        }

        private void ConnectButtonPressed(object sender, EventArgs e)
        {
            _Client.CreateNickName(_clientNickName);
            button1.Enabled = false;
            button2.Enabled = false;
            comboBox1.Enabled = true;
        }
    }
}
