using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SharedClassLibrary;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net.Security;

namespace SimpleServer
{
    class SimpleServer
    {
        public BinaryReader _tcpReader;
        public BinaryWriter _tcpWriter;
        public TcpListener _tcpListener;
        public BinaryFormatter _formatter;
        public MemoryStream _ms;
        public string _username;
        public EndPoint _endPoint;
        Client _client;
        List<Client> _clients;
        
        public SimpleServer(string ipAddress, int port)
        {
            //Firstly for the constructor
            _clients = new List<Client>(); //Create a new list of clients
            System.Net.IPAddress parsedAddress = IPAddress.Parse(ipAddress); //Next, set the systems IP address
            _tcpListener = new TcpListener(parsedAddress, port); //set the TCPListener to the address and port number chosen
            Console.WriteLine("Connected"); //If successful, we will have connected
        }
        public void Start()
        {
            //Firstly call the TCP listener start function
            _tcpListener.Start();
            bool running = true;
            do
            {
                //Then while we are running, accept a socket, and add our client to the socket
                Socket tcpSocket = _tcpListener.AcceptSocket();
                _client = new Client(tcpSocket);
                _clients.Add(_client); //Then add our client to the clients list
                Thread t = new Thread(new ParameterizedThreadStart(ClientMethod)); //Create a new thread and start it for our client.
                t.Start(_client);
            }
            while (running);
        }
        public Packet GetReturnMessage(Packet packet)
        {
            switch (packet.type) //check the packet to see if it's a message or a nickname
            {
                case PacketType.CHATMESSAGE:
                    ChatMessagePacket pChat = (ChatMessagePacket)packet;
                    SendMessageAllClients(_username + " - " + pChat.message);
                    break;
                case PacketType.NICKNAME:
                    NickNamePacket pNick = (NickNamePacket)packet;
                    _username = pNick.nickName;
                    break;
                case PacketType.ENDPOINT:
                    LoginPacket pLoginPacket = (LoginPacket)packet;
                    _endPoint = pLoginPacket.endPoint;
                    break;
            }
            return packet;
        }
        public void Stop()
        {
            _tcpListener.Stop();
        }
        public void ClientMethod(object ClientObj)
        {
            //Create the client
            Client client = (Client)ClientObj;
            Packet packet;

            while (_tcpReader != null)
            {
                if ((packet = client.TCPRead()) != null)
                {
                    GetReturnMessage(packet);
                }
                if((packet = client.UdpRead())!= null)
                {
                    GetReturnMessage(packet);
                }
            }
            client.Close();
            _clients.Remove(_client);
        }
        public void CreateMessage(string msg, Client client)
        {
            //When creating a message create a new packet and set it to a chat message child object
            Packet p = new ChatMessagePacket(msg);
            client.tcpSend(p); //them send the message to the index
        }
        public void SendMessageAllClients(string msg) //function for sending to all clients
        {
            for(int i = 0; i < _clients.Count; i++)
            {
                CreateMessage(msg, _clients[i]); //send a message to every client
            }
        }
    };
    class Client
    {
        public Socket _tcpSocket;
        public Socket _UdpSocket;
        public NetworkStream _stream;
        public NetworkStream _UdpStream;
        public BinaryReader _tcpReader;
        public BinaryWriter _tcpWriter;
        public BinaryReader _UdpReader;
        public BinaryWriter _UdpWriter;
        public BinaryFormatter _formatter;
        public MemoryStream _ms;
        public string _username;

        public Client(Socket socket)
        {
            _tcpSocket = socket;
            _stream = new NetworkStream(_tcpSocket);
            _UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _formatter = new BinaryFormatter();
            _ms = new MemoryStream();
        }

        public void UDPConnect(EndPoint clientConnection, string ipAdd, int port)
        {
            _UdpSocket.Connect(clientConnection);
            IPAddress iPAddress = Dns.Resolve(ipAdd).AddressList[0];
            IPEndPoint ipLocalEnd = new IPEndPoint(iPAddress, port);
            _UdpStream = new NetworkStream(_UdpSocket);
            _UdpReader = new BinaryReader(_UdpStream);
            _UdpWriter = new BinaryWriter(_UdpStream);
            Packet sendPacket = new LoginPacket(ipLocalEnd);
           tcpSend(sendPacket);
        }
        public void tcpSend(Packet data)
        {
            _ms = new MemoryStream();
            //Serialise the data into the memory stream
            _formatter.Serialize(_ms, data);
            byte[] buffer = _ms.GetBuffer();
            _ms.Position = 0;
            //write the data to the buffer
            _tcpWriter.Write(buffer.Length);
            _tcpWriter.Write(buffer);
            _tcpWriter.Flush();
        }
        public void UDPSend(Packet packet)
        {
            _ms = new MemoryStream();
            //Serialise data
            _formatter.Serialize(_ms, packet);
            byte[] buffer = _ms.GetBuffer();
            _ms.Position = 0;
            _UdpWriter.Write(buffer.Length);
            _UdpWriter.Write(buffer);
            _UdpWriter.Flush();

        }
        public Packet TCPRead()
        {
            int noOfIncomingBytes;
            if((noOfIncomingBytes = _tcpReader.ReadInt32())!= 0)
            {
                _ms = new MemoryStream(noOfIncomingBytes);
                byte[] buffer = _tcpReader.ReadBytes(noOfIncomingBytes);
                _ms.Write(buffer, 0, noOfIncomingBytes);
                _ms.Position = 0;
                Packet packet = _formatter.Deserialize(_ms) as Packet;
                return packet;
            }
            return null;
        }

        public Packet UdpRead()
        {
            int noOfIncomingBytes;
            if ((noOfIncomingBytes = _UdpReader.ReadInt32()) != 0)
            {
                _ms = new MemoryStream(noOfIncomingBytes);
                byte[] buffer = _UdpReader.ReadBytes(noOfIncomingBytes);
                _ms.Write(buffer, 0, noOfIncomingBytes);
                _ms.Position = 0;
                Packet packet = _formatter.Deserialize(_ms) as Packet;
                return packet;
            }
            return null;
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
