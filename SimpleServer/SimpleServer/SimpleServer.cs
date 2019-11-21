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
        public IPAddress _ipadd;
        public int _port;
        Client _client;
        List<Client> _clients;
        
        public SimpleServer(string ipAddress, int port)
        {
            
            //Firstly for the constructor
            _clients = new List<Client>(); //Create a new list of clients
            System.Net.IPAddress parsedAddress = IPAddress.Parse(ipAddress); //Next, set the systems IP address
            _ipadd = parsedAddress;
            _port = port;
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
                Thread t = new Thread(new ParameterizedThreadStart(TCPClientMethod)); //Create a new thread and start it for our client.
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
                   _client.UDPConnect(_endPoint); //if we have a log in packet, then connect the client with UDP
                    Thread t = new Thread(new ParameterizedThreadStart(UDPClientMethod));
                    t.Start(_client);
                    break;
            }
            return packet;
        }
        public void Stop()
        {
            _tcpListener.Stop();
        }

        public void UDPClientMethod(object ClientObj)
        {
            Client client = (Client)ClientObj;
            while(client._UdpSocket.Connected)
            {
                GetReturnMessage(client.UdpRead());
            }
        }

        public void TCPClientMethod(object ClientObj)
        {
            //Create the client
            Client client = (Client)ClientObj;
            while(client._tcpSocket.Connected)
            {
                GetReturnMessage(client.TCPRead());
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
        public BinaryReader _tcpReader;
        public BinaryWriter _tcpWriter;
        public BinaryFormatter _formatter;
        public MemoryStream _ms;
        public string _username;

        public Client(Socket socket)
        {
            _tcpSocket = socket;
            _stream = new NetworkStream(_tcpSocket);
            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _formatter = new BinaryFormatter();
            _ms = new MemoryStream();
        }

        public void UDPConnect(EndPoint clientConnection)
        {
            _UdpSocket.Connect(clientConnection);
            Packet sendPacket = new LoginPacket(_UdpSocket.LocalEndPoint);
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
            _formatter.Serialize(_ms, packet);
            byte[] buffer = _ms.GetBuffer();
            _UdpSocket.Send(buffer);
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
            byte[] bytes = new byte[1024 * 1024];
            try {
                int noOfIncomingBytes;
                if ((noOfIncomingBytes = _UdpSocket.Receive(bytes)) != 0)
                {
                    _ms = new MemoryStream(bytes);
                    Packet packet = _formatter.Deserialize(_ms) as Packet;
                    return packet;
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
