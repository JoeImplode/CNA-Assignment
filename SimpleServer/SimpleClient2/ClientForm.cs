using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SimpleClient2
{
    public partial class ClientForm : Form
    {
        delegate void                       UpdateChatWindowDelegate(string message);
        delegate void                       UpdateClientListBoxDelegate(List<string> clientListFromClient);

        UpdateChatWindowDelegate            updateChatWindowDelegate;
        UpdateClientListBoxDelegate         updateClientListBoxDeletage;

        public SimpleClient.SimpleClient    Client;

        public string                       clientNickName;
        public int                          clientListSelection;
        public bool                         isConnected;

        public ClientForm(SimpleClient.SimpleClient client)
        {
            InitializeComponent();
            updateChatWindowDelegate        = new UpdateChatWindowDelegate(UpdateChatWindow);
            updateClientListBoxDeletage     = new UpdateClientListBoxDelegate(UpdateClientListBox);
            Client                          = client;

            InputMessage.Select();

            button2.Enabled                 = false;
            clientListSelection             = 0;
            comboBox1.Enabled               = false;
            button3.Enabled                 = false;
            isConnected                     = false;
        }
        public void UpdateClientListBox(List<string> clientListFromClient)
        {
            if (comboBox1.InvokeRequired)
                Invoke(updateClientListBoxDeletage, clientListFromClient);

            else
            {
                comboBox1.Items.Clear();

                for (int i = 0; i < clientListFromClient.Count; i++)
                    comboBox1.Items.Add(clientListFromClient[i]);

                comboBox1.SelectedIndex = 0;
            }
        }
        public void UpdateChatWindow(string messageRecieved)
        {
            try
            {
                if (chatBox.InvokeRequired)
                    Invoke(updateChatWindowDelegate, messageRecieved);

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
            if (isConnected == true)
            {
                if (clientListSelection == 0)
                {
                    Client.CreateMessage(InputMessage.Text);
                    InputMessage.Clear();
                }
                else
                    Client.CreateMessage(InputMessage.Text, clientListSelection);
            }
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            Client.Run();
        }
        private void ClientForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Client.Stop();
        }

        private void NickNameButtonPressed(object sender, EventArgs e)
        {
            if (InputMessage.Text != string.Empty)
            {
                clientNickName = InputMessage.Text;
                button2.Enabled = true;
            }
        }
        private void InputMessageBoxChanged(object sender, EventArgs e)
        {

        }
        private void ComboBoxChanged(object sender, EventArgs e)
        {
            clientListSelection = comboBox1.SelectedIndex;
        }

        private void ConnectButtonPressed(object sender, EventArgs e)
        {
            Client.CreateNickName(clientNickName);
            button1.Enabled = false;
            button2.Enabled = false;
            isConnected = true;
            comboBox1.Enabled = true;
            button3.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Client.RequestGame(comboBox1.SelectedIndex, 2,clientNickName);
        }
    }
}
