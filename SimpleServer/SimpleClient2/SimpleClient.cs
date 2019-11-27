using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SimpleClient2;
using SharedClassLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;

namespace SimpleClient
{
    public class SimpleClient
    {
        private BinaryReader            _tcpReader;    
        private BinaryWriter            _tcpWriter;
        private NetworkStream           _stream;

        private Thread                  _tcpReaderThread, _udpReaderThread;
        public TcpClient                _tcpClient;
        public UdpClient                _udpClient;

        public BinaryFormatter          _formatter;
        public MemoryStream             _memoryStream;
        public ClientForm               _form;
        public NoughtsAndCrosses        noughtsAndCrosses;

        public string                   _serverIPAddress;
        public int                      _serverPort;
        public List<string>             nicknameList;
        public string                   gameRecipient;
        public string                   gameSender;
        public SimpleClient()
        {
            _form = new ClientForm(this);
            _tcpClient = new TcpClient();
            _udpClient = new UdpClient();
            noughtsAndCrosses = new NoughtsAndCrosses(this);
            noughtsAndCrosses.Hide();
            nicknameList = new List<string>();
            gameRecipient = "";
            gameSender = "";
        }

        public bool Connect(string serverIPAddress, int serverPort)
        {
            _serverIPAddress = serverIPAddress;
            _serverPort = serverPort;

            _tcpClient.Connect(_serverIPAddress,_serverPort);
            _stream = _tcpClient.GetStream();

            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _formatter = new BinaryFormatter();
            _memoryStream = new MemoryStream();

            _udpClient.Connect(_serverIPAddress, _serverPort);

            LoginPacket logInPacket = new LoginPacket(_udpClient.Client.LocalEndPoint);

            TCPSend(logInPacket);

            _tcpReaderThread = new Thread(new ThreadStart(TCPRead));
            Application.Run(_form);
            return true;
        }
        public void Run()
        {
            _tcpReaderThread.Start();
        }

        public void Stop()
        {
            _tcpReaderThread.Abort();
            _tcpClient.Close();
            _udpReaderThread.Abort();
            _udpClient.Close();
        }


        public void UDPRead()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] buffer = _udpClient.Receive(ref endPoint);
                _memoryStream = new MemoryStream(buffer);
                Packet packetFromServer = _formatter.Deserialize(_memoryStream) as Packet;

                HandlePacket(packetFromServer);
            }
        }

        public void TCPRead()
        {
            int noOfIncomingBytes;
            try
            {
                while ((noOfIncomingBytes = _tcpReader.ReadInt32()) != 0)
                {
                    _memoryStream = new MemoryStream(noOfIncomingBytes);
                    byte[] buffer = _tcpReader.ReadBytes(noOfIncomingBytes);

                    _memoryStream.Write(buffer, 0, noOfIncomingBytes);

                    _memoryStream.Position = 0;
                    Packet packetFromServer = _formatter.Deserialize(_memoryStream) as Packet;

                    HandlePacket(packetFromServer);
                }
            }
            catch (SocketException e)
            {
            }
        }
        private void HandlePacket(Packet packetFromServer)
        {
            switch (packetFromServer.type)
            {
                case PacketType.GAME:
                    GamePacket pGamePacket = (GamePacket)packetFromServer;
                    int selection = UpdateButtons(pGamePacket.x, pGamePacket.y);
                    switch(selection)
                    {
                        case 1:
                            noughtsAndCrosses.button1.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button1.Text = pGamePacket.text;
                                noughtsAndCrosses.button1.Enabled = false;
                            }));
                            break;
                        case 2:
                            noughtsAndCrosses.button2.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button2.Text = pGamePacket.text;
                                noughtsAndCrosses.button2.Enabled = false;
                            }));
                            break;
                        case 3:
                            noughtsAndCrosses.button3.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button3.Text = pGamePacket.text;
                                noughtsAndCrosses.button3.Enabled = false;
                            }));
                            break;
                        case 4:
                            noughtsAndCrosses.button4.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button4.Text = pGamePacket.text;
                                noughtsAndCrosses.button4.Enabled = false;
                            }));
                            break;
                        case 6:
                            noughtsAndCrosses.button6.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button6.Text = pGamePacket.text;
                                noughtsAndCrosses.button6.Enabled = false;
                            }));
                            break;
                        case 7:
                            noughtsAndCrosses.button7.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button7.Text = pGamePacket.text;
                                noughtsAndCrosses.button7.Enabled = false;
                            }));
                            break;
                        case 8:
                            noughtsAndCrosses.button8.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button8.Text = pGamePacket.text;
                                noughtsAndCrosses.button8.Enabled = false;
                            }));
                            break;
                        case 9:
                            noughtsAndCrosses.button9.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button9.Text = pGamePacket.text;
                                noughtsAndCrosses.button9.Enabled = false;
                            }));
                            break;
                        case 10:
                            noughtsAndCrosses.button10.Invoke(new MethodInvoker(delegate ()
                            {
                                noughtsAndCrosses.button10.Text = pGamePacket.text;
                                noughtsAndCrosses.button10.Enabled = false;
                            }));
                            break;

                    }
                    break;
                case PacketType.CHATMESSAGE:
                    ChatMessagePacket messagePacket = (ChatMessagePacket)packetFromServer;
                    _form.UpdateChatWindow(messagePacket.message);
                    break;

                case PacketType.NICKNAME:
                    NickNamePacket nicknamePacket = (NickNamePacket)packetFromServer;
                    _form.UpdateChatWindow(nicknamePacket.nickName);
                    break;

                case PacketType.ENDPOINT:
                    LoginPacket serverLogInDetails = (LoginPacket)packetFromServer;
                    _udpClient.Connect((IPEndPoint)serverLogInDetails.endPoint);

                    _udpReaderThread = new Thread(UDPRead);
                    _udpReaderThread.Start();
                    break;

                case PacketType.USERLIST:
                    UserListPacket userListPacket = (UserListPacket)packetFromServer;
                    nicknameList = userListPacket.userList;

                    _form.UpdateClientListBox(userListPacket.userList);
                    break;

                case PacketType.GAMEREQ:
                    GameRequestPacket gameReqPacket = (GameRequestPacket)packetFromServer;
                    int packetState = (int)gameReqPacket.requestState;
                    switch (packetState)
                    {
                        case 0:
                            _form.UpdateChatWindow("Game was declined!");
                            break;
                        case 1:
                            gameRecipient = gameReqPacket.recipient;
                            gameSender = gameReqPacket.sender;
                            _form.UpdateChatWindow("Game accepted!");
                            RequestGame(gameReqPacket.sender, 1, gameReqPacket.recipient);
                            noughtsAndCrosses.Text = _form.clientNickName;
                            noughtsAndCrosses.ShowDialog();
                            break;
                        case 2:
                            _form.CreateMessageBox(gameReqPacket.sender,gameReqPacket.recipient);
                            break;
                    }
                    break;
            }
        }
        public void TCPSend(Packet data)
        {
            _memoryStream = new MemoryStream();

            _formatter.Serialize(_memoryStream, data);

            byte[] buffer = _memoryStream.GetBuffer();
            _memoryStream.Position = 0;

            _tcpWriter.Write(buffer.Length);
            _tcpWriter.Write(buffer);
            _tcpWriter.Flush();
        }

        public void UDPSend(Packet data)
        {
            _memoryStream = new MemoryStream();
            _formatter.Serialize(_memoryStream, data);
            byte[] buffer = _memoryStream.GetBuffer();

            _udpClient.Send(buffer,buffer.Length);
        }
        public void CreateNickName(string nickName)
        {
            //udpSend(new NickNamePacket(nickName));
            TCPSend(new NickNamePacket(nickName)); 
        }
        public void CreateMessage(string message,string recipient)
        {
            //udpSend(new ChatMessagePacket(message));
            TCPSend(new ChatMessagePacket(message,recipient));
        }
        public void RequestGame(string recipient, int request, string sender)
        {
            GameRequestPacket.RequestState state = (GameRequestPacket.RequestState)request;
            TCPSend(new GameRequestPacket(recipient, state,sender));
        }

        public void AcceptDeclineGame(string recipient, int accept, string sender)
        {
            GameRequestPacket.RequestState state = (GameRequestPacket.RequestState)accept;
            TCPSend(new GameRequestPacket(recipient, state, sender));
        }

        public void UpdateMatrix(int positionx, int positiony, string user)
        {
            UDPSend(new GamePacket(positionx, positiony, gameSender, gameRecipient, 1,user));
        }

        public int UpdateButtons(int x, int y)
        {
            switch(x)
            {
                case 0:
                    switch (y)
                    {
                        case 0:
                            return 8;
                        case 1:
                            return 4;
                        case 2:
                            return 1;
                    }
                    break;
                case 1:
                    switch(y)
                    {
                        case 0:
                            return 9;
                        case 1:
                            return 6;
                        case 2:
                            return 2;
                    }
                    break;
                case 2:
                    switch(y)
                    {
                        case 0:
                            return 10;
                        case 1:
                            return 7;
                        case 2:
                            return 3;
                    }
                    break;
                    
            }
            return -1;
        }

    }
}
