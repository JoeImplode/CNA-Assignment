using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SimpleClient2;
using SharedClassLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Collections.Generic;

namespace SimpleClient
{
    public class SimpleClient
    {
        private BinaryReader _tcpReader;
        private BinaryWriter _tcpWriter;
        private NetworkStream _stream;
        private Thread _tcpReaderThread, _udpReaderThread;
        public TcpClient _tcpClient;
        public UdpClient _udpClient;
        public BinaryFormatter _formatter;
        public MemoryStream _memoryStream;
        public ClientForm _form;
        public string _serverIPAddress;
        public int _serverPort;

        public List<string> clients;

        public SimpleClient()
        {
            _form = new ClientForm(this);
            _tcpClient = new TcpClient();
            _udpClient = new UdpClient();
            clients = new List<string>();
        }

        public bool Connect(string serverIPAddress, int serverPort)
        {
            //connect the tcp client to the ip address and port number
            _serverIPAddress = serverIPAddress;
            _serverPort = serverPort;
            _tcpClient.Connect(_serverIPAddress,_serverPort);
            _stream = _tcpClient.GetStream();
            //Set the reader, writer, formatter and memory stream
            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _formatter = new BinaryFormatter();
            _memoryStream = new MemoryStream();
            //Connect our UDP client to the ip address and port
            _udpClient.Connect(_serverIPAddress, _serverPort);
            //Create a temporary login packet sent through TCP to set the details for UDP
            LoginPacket logInPacket = new LoginPacket(_udpClient.Client.LocalEndPoint);
            TCPSend(logInPacket);
            //Finally start the reader thread
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
                    //Update our window with our message
                    ChatMessagePacket messagePacket = (ChatMessagePacket)packetFromServer;
                    _form.UpdateChatWindow(messagePacket.message);
                    break;
                case PacketType.NICKNAME:
                    //Update our window with our nickname
                    NickNamePacket nicknamePacket = (NickNamePacket)packetFromServer;
                    _form.UpdateChatWindow(nicknamePacket.nickName);
                    break;
                case PacketType.ENDPOINT:
                    //Receive an end point and connect the udp client, start the udp reader thread
                    LoginPacket serverLogInDetails = (LoginPacket)packetFromServer;
                    _udpClient.Connect((IPEndPoint)serverLogInDetails.endPoint);
                    _udpReaderThread = new Thread(UDPRead);
                    _udpReaderThread.Start();
                    break;
                case PacketType.USERLIST:
                    //Set our client list to the user list passed from the server
                    UserListPacket userListPacket = (UserListPacket)packetFromServer;
                    clients = userListPacket.userList;
                    _form.UpdateClientListBox(userListPacket.userList);
                    break;
            }
        }
        public void TCPSend(Packet data)
        {
            //Create a new memory stream and serialise the data into the memory stream
            MemoryStream ms = new MemoryStream();
            _memoryStream = new MemoryStream();
            _formatter.Serialize(_memoryStream, data);
            //Get the buffer from the memory stream
            byte[] buffer = _memoryStream.GetBuffer();
            _memoryStream.Position = 0;
            //Write the buffer information along the tcp writer
            _tcpWriter.Write(buffer.Length);
            _tcpWriter.Write(buffer);
            _tcpWriter.Flush();
        }

        public void UDPSend(Packet data)
        {
            //Here simply serialise and send the informaiton over
            _memoryStream = new MemoryStream();
            _formatter.Serialize(_memoryStream, data);
            byte[] buffer = _memoryStream.GetBuffer();
            _udpClient.Send(buffer,buffer.Length);
        }

        public void UDPRead()
        {
            //Set a local var to any endpoint
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                //receive an end point from the udp client connection and set it to buffer
                byte[] buffer = _udpClient.Receive(ref endPoint);
                _memoryStream = new MemoryStream(buffer);
                //Deserialise the data and pass it to the client funciton
                Packet packetFromServer = _formatter.Deserialize(_memoryStream) as Packet;
                HandlePacket(packetFromServer);
            }
        }

        public void TCPRead()
        {
            int noOfIncomingBytes;
            try
            {
                //Read the bytes from the tcpReader and set them to noOfIncomingBytes;
                while ((noOfIncomingBytes = _tcpReader.ReadInt32()) != 0)
                {
                    //Set the memory stream and read the number of bytes we need into the buffer
                    _memoryStream = new MemoryStream(noOfIncomingBytes);
                    byte[] buffer = _tcpReader.ReadBytes(noOfIncomingBytes);
                    //Write the bytes and how many there are into the buffer
                    _memoryStream.Write(buffer, 0, noOfIncomingBytes);
                    _memoryStream.Position = 0;
                    //Deserialise the memory stream
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

        public void RequestDirectID()
        {

        }
    }
}
