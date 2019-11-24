using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SharedClassLibrary;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleServer
{
    class SimpleServer
    {
        public TcpListener      TCPListener;
        public EndPoint         localEP;
        public IPAddress        serverIpAddress;
        public int              serverPort;
        private Client          _mClient;
        public List<Client>     clientList;
        public List<string>     nicknameList;
 
        public SimpleServer(string serverIpAddress, int port)
        {
            clientList = new List<Client>();
            IPAddress parsedAddress = IPAddress.Parse(serverIpAddress);
            this.serverIpAddress = parsedAddress;

            serverPort = port;
            TCPListener = new TcpListener(parsedAddress, port);
            nicknameList = new List<string>();

            nicknameList.Add("Server");
        }

        public void Start()
        {
            TCPListener.Start();
            bool connection = true;

            do
            {
                Socket tcpSocket = TCPListener.AcceptSocket();
                _mClient = new Client(tcpSocket);
                clientList.Add(_mClient);
                Thread t = new Thread(new ParameterizedThreadStart(TCPClientMethod));
                t.Start(_mClient);
            }

            while (connection);
        }
        public Packet HandlePacket(Packet packetReceived, Client mClient)
        {
            switch (packetReceived.type)
            {
                case PacketType.CHATMESSAGE:
                    ChatMessagePacket pMessage = (ChatMessagePacket)packetReceived;
                    if (pMessage.index == 0)
                        SendMessageAll(mClient.username + " - " + pMessage.message);
                    else
                        CreateMessage(mClient.username + " whispers to you: " +
                        pMessage.message, clientList[pMessage.index - 1]);
                    break;

                case PacketType.NICKNAME:
                    NickNamePacket pNick = (NickNamePacket)packetReceived;
                    mClient.username = pNick.nickName;
                    nicknameList.Add(mClient.username);
                    SendClientList(nicknameList);
                    break;

                case PacketType.ENDPOINT:
                    LoginPacket pLogin = (LoginPacket)packetReceived;
                    localEP = pLogin.endPoint;
                    mClient.UDPSendLocalEP(localEP);
                    Thread t = new Thread(new ParameterizedThreadStart(UDPClientMethod));
                    t.Start(mClient);
                    break;

                case PacketType.GAMEREQ:
                    GameRequestPacket pGameReqPacekt = (GameRequestPacket)packetReceived;
                    int packetState = (int)pGameReqPacekt.requestState;
                    switch(packetState)
                    {
                        case 0:
                             if(CheckUserExists(clientList,pGameReqPacekt.userName))
                                    RespondGameRequest(pGameReqPacekt.userListNum, pGameReqPacekt.requestState, pGameReqPacekt.userName);
                            break;
                        case 1:
                            if (CheckUserExists(clientList, pGameReqPacekt.userName))
                                RespondGameRequest(pGameReqPacekt.userListNum, pGameReqPacekt.requestState, pGameReqPacekt.userName);
                            break;
                        case 2:
                            if(CheckUserExists(clientList,pGameReqPacekt.userName))
                                    RespondGameRequest(pGameReqPacekt.userListNum, pGameReqPacekt.requestState, pGameReqPacekt.userName);
                            break;

                    }
                    break;
            }
            return packetReceived;
        }

        public void Stop()
        {
            TCPListener.Stop();
        }

        public void UDPClientMethod(object ClientObj)
        {
            Client clientUDP = (Client)ClientObj;

            while(clientUDP.UDPSocket.Connected)
                HandlePacket(clientUDP.UDPRead(),clientUDP);
        }

        public void TCPClientMethod(object ClientObj)
        {
            Client clientTCP = (Client)ClientObj;

            while(clientTCP.TCPSocket.Connected)
                HandlePacket(clientTCP.TCPRead(),clientTCP);

            clientTCP.Close();
            clientList.Remove(_mClient);
        }
        public void CreateMessage(string message, Client recipient)
        {
            Packet pSend = new ChatMessagePacket(message);
            recipient.TCPSend(pSend);
            //client.UDPSend(p);
        }
        public void SendMessageAll(string message)
        {
            for (int i = 0; i < clientList.Count; i++)
                CreateMessage(message, clientList[i]);
        }

        public void SendClientList(List<string> clientList)
        { 
            Packet p = new UserListPacket(clientList);

            for (int i = 0; i < this.clientList.Count; i++)
                this.clientList[i].TCPSend(p);
        }

        public void RespondGameRequest(int userListNumber,GameRequestPacket.RequestState state, string username)
        {
            Packet pResponse = new GameRequestPacket(userListNumber, state, username);
            for (int i = 0; i < this.clientList.Count; i++)
                if (clientList[i].username == username)
                    clientList[userListNumber-1].TCPSend(pResponse);
        }

        public bool CheckUserExists(List<Client> list, string check)
        {
            for (int i = 0; i < list.Count; i++)
                if (list[i].username == check)
                    return true;
            return false;
        }

    };
    class Client
    {
        public Socket           TCPSocket, UDPSocket;
        public NetworkStream    TCPStream;
        public BinaryReader     TCPReader;
        public BinaryWriter     TCPWriter;
        public BinaryFormatter  formatter;
        public MemoryStream     memStream;
        public string           username;

        public Client(Socket socket)
        {
            TCPSocket =    socket;
            TCPStream =    new NetworkStream(TCPSocket);
            TCPReader =    new BinaryReader(TCPStream);
            TCPWriter =    new BinaryWriter(TCPStream);
            UDPSocket =    new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            formatter =    new BinaryFormatter();
            memStream =    new MemoryStream();
        }

        public void UDPSendLocalEP(EndPoint clientEP)
        {
            UDPSocket.Connect(clientEP);
            Packet sendPacket = new LoginPacket(UDPSocket.LocalEndPoint);
            TCPSend(sendPacket);
        }

        public void TCPSend(Packet data)
        {
            memStream = new MemoryStream();
            formatter.Serialize(memStream, data);
            byte[] buffer = memStream.GetBuffer();

            memStream.Position = 0;
            TCPWriter.Write(buffer.Length);
            TCPWriter.Write(buffer);
            TCPWriter.Flush();
        }
        public void UDPSend(Packet data)
        {
            memStream = new MemoryStream();
            formatter.Serialize(memStream, data);
            byte[] buffer = memStream.GetBuffer();
            UDPSocket.Send(buffer);
        }
        public Packet TCPRead()
        {
            int noOfIncomingBytes;

            try
            {
                if ((noOfIncomingBytes = TCPReader.ReadInt32()) != 0)
                {
                    memStream = new MemoryStream(noOfIncomingBytes);
                    byte[] buffer = TCPReader.ReadBytes(noOfIncomingBytes);
                    memStream.Write(buffer, 0, noOfIncomingBytes);
                    memStream.Position = 0;
                    Packet tcpReadPacket = formatter.Deserialize(memStream) as Packet;
                    return tcpReadPacket;
                }
            }
            catch(SocketException e)
            {
                return new Packet();
            }
            return new Packet();
        }

        public Packet UDPRead()
        {
            byte[] bytes = new byte[1024 * 1024];

            try {
                int noOfIncomingBytes;

                if ((noOfIncomingBytes = UDPSocket.Receive(bytes)) != 0)
                {
                    memStream = new MemoryStream(bytes);
                    Packet udpReadPacket = formatter.Deserialize(memStream) as Packet;
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
            TCPStream = stream;
            return TCPStream;
        }
        public BinaryReader Reader { get; private set; }
        public BinaryWriter Writer { get; private set; }
        private Socket socket()
        {
            return TCPSocket;
        }
        public void Close()
        {
            TCPSocket.Close();
        }
    }
}
