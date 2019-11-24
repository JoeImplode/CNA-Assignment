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

        public string                   _serverIPAddress;
        public int                      _serverPort;
        public List<string>             clients;

        public SimpleClient()
        {
            _form = new ClientForm(this);
            _tcpClient = new TcpClient();
            _udpClient = new UdpClient();

            clients = new List<string>();
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
        private void HandlePacket(Packet packetFromServer)
        {
            switch (packetFromServer.type)
            {
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
                    clients = userListPacket.userList;

                    _form.UpdateClientListBox(userListPacket.userList);
                    break;

                case PacketType.GAMEREQ:
                    GameRequestPacket gameReqPacket = (GameRequestPacket)packetFromServer;
                    break;
            }
        }
        public void TCPSend(Packet data)
        {
            MemoryStream ms = new MemoryStream();
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
        public void CreateNickName(string nickName)
        {
            //udpSend(new NickNamePacket(nickName));
            TCPSend(new NickNamePacket(nickName)); 
        }
        public void CreateMessage(string message,int index = 0)
        {
            //udpSend(new ChatMessagePacket(message));
            int _index = index;
            TCPSend(new ChatMessagePacket(message,_index));
        }
        public void RequestGame(int _userListIndex, int request, string _userName)
        {
            GameRequestPacket.RequestState state = (GameRequestPacket.RequestState)request;
            TCPSend(new GameRequestPacket(_userListIndex, state,_userName));
        }
    }
}
