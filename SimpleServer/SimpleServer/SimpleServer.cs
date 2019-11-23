using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SharedClassLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Security;


//-------------------------------------------------------
/* PROGRAM HELP:
 * The server holds a "mimick" client class which holds all operations for our actual clients that connect
 * Our get methods for the client and server call their respective handling functions
 * 1)Create a client and have it send a log in packet
 * 2)When our server constructs, it will handle the log in packet and store that endpoint info about that client
 */
//------------------------------------------------------
namespace SimpleServer
{
    class SimpleServer
    {
        public TcpListener _tcpListener;
        public EndPoint _mimickEndPoint;
        public IPAddress _serverIpAddress;
        public int _serverPort;
        Client _mimickClient;
        public List<Client> _clientList;
        public List<string> _listOfNicknames;
 
        public SimpleServer(string serverIpAddress, int port)
        {
            //Create a list of clients and pars and IP address
            _clientList = new List<Client>();
            
            System.Net.IPAddress parsedAddress = IPAddress.Parse(serverIpAddress);
            _serverIpAddress = parsedAddress;
            _serverPort = port;
            //Listen out on the parsed address and port for tcp connections
            _tcpListener = new TcpListener(parsedAddress, port);
            //Set our user list for all usernames
            _listOfNicknames = new List<string>();
            _listOfNicknames.Add("Server");
        }

        public void Start()
        {
            _tcpListener.Start();
            bool connection = true;
            do
            {
                //This loop checks if a new connection has been made to the server, if so, it then returns the tcp socket back to the client
                //Accept socket accepts a pending connection in the tcp listener - we set it to the tcpSocket
                Socket tcpSocket = _tcpListener.AcceptSocket();
                //Create our new client and pass the socket into the constructor, then add the client to the clients list
                _mimickClient = new Client(tcpSocket);
                _clientList.Add(_mimickClient);
                //Start a thread for our tcp client connection
                Thread t = new Thread(new ParameterizedThreadStart(TCPClientMethod));
                t.Start(_mimickClient);
            }
            while (connection);
        }
        public Packet HandlePacket(Packet packetFromClient, Client mimickClient)
        {
            switch (packetFromClient.type)
            {
                case PacketType.CHATMESSAGE:
                    //If we receive a chat message then send a message to al clients using the username and message
                    ChatMessagePacket msgFromClient = (ChatMessagePacket)packetFromClient;
                    SendMessageAllClients(mimickClient._username + " - " + msgFromClient.message);
                    break;
                case PacketType.NICKNAME:
                    //Set the username to the nick name packet passed in
                    NickNamePacket nickFromClient = (NickNamePacket)packetFromClient;
                    mimickClient._username = nickFromClient.nickName;
                    //Add the clients username to the user list and send it to the client
                    _listOfNicknames.Add(mimickClient._username);
                    SendClientList(_listOfNicknames);
                    break;
                case PacketType.ENDPOINT:
                    //We take our login packet from the client and set the mimick end point to the login.endpoint data
                    LoginPacket loginFromClient = (LoginPacket)packetFromClient;
                    _mimickEndPoint = loginFromClient.endPoint;
                    //We then connect our mimick client through UDP using this mimick end point
                    mimickClient.UDPSendLocalEP(_mimickEndPoint);
                    //Then start a thread with our mimicked client
                    Thread t = new Thread(new ParameterizedThreadStart(UDPClientMethod));
                    t.Start(mimickClient);
                    break;
            }
            return packetFromClient;
        }
        public void Stop()
        {
            _tcpListener.Stop();
        }

        public void UDPClientMethod(object ClientObj)
        {
            Client clientUDP = (Client)ClientObj;
            while(clientUDP._UdpSocket.Connected)
            {
                HandlePacket(clientUDP.UdpRead(),clientUDP);
            }
        }

        public void TCPClientMethod(object ClientObj)
        {
            Client clientTCP = (Client)ClientObj;
            while(clientTCP._tcpSocket.Connected)
            {
                HandlePacket(clientTCP.TCPRead(),clientTCP);
            }
            clientTCP.Close();
            _clientList.Remove(_mimickClient);
        }
        public void CreateMessage(string message, Client recipient)
        {
            Packet packetToSend = new ChatMessagePacket(message);
            recipient.tcpSend(packetToSend);
            //client.UDPSend(p);
        }
        public void SendMessageAllClients(string message)
        {
            for (int i = 0; i < _clientList.Count; i++)
            {
                CreateMessage(message, _clientList[i]);
            }
        }

        public void SendClientList(List<string> clientList)
        {
            Packet p = new UserListPacket(clientList);
            for (int i = 0; i < _clientList.Count; i++)
            {
                _clientList[i].tcpSend(p);
            }
        }
    };
    class Client
    {
        public Socket _tcpSocket, _UdpSocket;
        public NetworkStream _stream;
        public BinaryReader _tcpReader;
        public BinaryWriter _tcpWriter;
        public BinaryFormatter _formatter;
        public MemoryStream _memoryStream;
        public string _username;
        public Client(Socket socket)
        {
            _tcpSocket = socket;
            _stream = new NetworkStream(_tcpSocket);
            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _formatter = new BinaryFormatter();
            _memoryStream = new MemoryStream();
        }

        public void UDPSendLocalEP(EndPoint clientEndPoint)
        {
            //Connects our clientConnection to the socket
            _UdpSocket.Connect(clientEndPoint);
            //Then we send our login packet containing the local end point back to the UDPClient
            Packet sendPacket = new LoginPacket(_UdpSocket.LocalEndPoint);
            tcpSend(sendPacket);
        }

        public void tcpSend(Packet data)
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
            _UdpSocket.Send(buffer);
        }
        public Packet TCPRead()
        {
            int noOfIncomingBytes;
            try
            { 
                if ((noOfIncomingBytes = _tcpReader.ReadInt32()) != 0)
                {
                    _memoryStream = new MemoryStream(noOfIncomingBytes);
                    byte[] buffer = _tcpReader.ReadBytes(noOfIncomingBytes);
                    _memoryStream.Write(buffer, 0, noOfIncomingBytes);
                    _memoryStream.Position = 0;
                    Packet tcpReadPacket = _formatter.Deserialize(_memoryStream) as Packet;
                    return tcpReadPacket;
                }
            }
            catch(SocketException e)
            {
                return new Packet();
            }
            return new Packet();
        }

        public Packet UdpRead()
        {
            byte[] bytes = new byte[1024 * 1024];
            try {
                int noOfIncomingBytes;
                if ((noOfIncomingBytes = _UdpSocket.Receive(bytes)) != 0)
                {
                    _memoryStream = new MemoryStream(bytes);
                    Packet udpReadPacket = _formatter.Deserialize(_memoryStream) as Packet;
                    return udpReadPacket;
                }
            }
            catch(SocketException e)
            {
                return new Packet();
            }
            return new Packet();
        }

        public NetworkStream stream(NetworkStream stream)
        {
            _stream = stream;
            return _stream;
        }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }
        private Socket socket()
        {
            return _tcpSocket;
        }
        public void Close()
        {
            _tcpSocket.Close();
        }
    }
}
