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
        public bool                         isConnected;

        public ClientForm(SimpleClient.SimpleClient client)
        {
            InitializeComponent();
            updateChatWindowDelegate        = new UpdateChatWindowDelegate(UpdateChatWindow);
            updateClientListBoxDeletage     = new UpdateClientListBoxDelegate(UpdateClientListBox);
            Client                          = client;

            InputMessage.Select();

            button2.Enabled                 = false;
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
                if (comboBox1.SelectedIndex == 0)
                {
                    Client.CreateMessage(InputMessage.Text,"server");
                    InputMessage.Clear();
                }
                else if ((string)comboBox1.Items[comboBox1.SelectedIndex] != clientNickName)
                {
                    Client.CreateMessage(InputMessage.Text,(string)comboBox1.Items[comboBox1.SelectedIndex]);
                    InputMessage.Clear();
                }
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
            if((string)comboBox1.Items[comboBox1.SelectedIndex]
                != clientNickName && comboBox1.SelectedIndex != 0)
                Client.RequestGame((string)comboBox1.Items[comboBox1.SelectedIndex], 2,clientNickName);
        }
        public void CreateMessageBox(string username,string recipient)
        {
            DialogResult dialogResult = MessageBox.Show(username + " would like to play a game with you! ","Game Request",MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Client.AcceptDeclineGame(recipient, 1, clientNickName);
            }
            else
                Client.AcceptDeclineGame(recipient, 0, clientNickName);
        }
    }
}
