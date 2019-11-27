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
        public TcpListener          TCPListener;
        public EndPoint             localEP;
        public IPAddress            serverIpAddress;
        public int                  serverPort;
        private Client              _mClient;
        public List<Client>         clientList;
        public List<string>         nicknameList;
        public List<GameLogic>      Games;
        public SimpleServer(string serverIpAddress, int port)
        {
            clientList = new List<Client>();
            IPAddress parsedAddress = IPAddress.Parse(serverIpAddress);
            this.serverIpAddress = parsedAddress;
            Games = new List<GameLogic>();
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
                    if (pMessage.recipient == "server")
                        SendMessageAll(mClient.username + " - " + pMessage.message);
                    else
                    {
                        CreateMessage(mClient.username + " whispers to you: " +
                        pMessage.message,pMessage.recipient);
                    }
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
                    GameRequestPacket pGameReqPacket = (GameRequestPacket)packetReceived;
                    int packetState = (int)pGameReqPacket.requestState;
                    if (packetState == 0)
                        CheckUserExists(clientList, pGameReqPacket.sender, pGameReqPacket);
                    else if (packetState == 1)
                    {
                        if (CheckGameAccepted(Games, pGameReqPacket.sender) == false)
                        {
                            GameLogic game = new GameLogic(pGameReqPacket.recipient, pGameReqPacket.sender);
                            Games.Add(game);
                            CheckUserExists(clientList, pGameReqPacket.sender, pGameReqPacket);
                            CheckUserExists(clientList, pGameReqPacket.recipient, pGameReqPacket);
                        }
                        else
                        {
                            packetReceived = new Packet();
                        }
                    }
                    else if (packetState == 2)
                        CheckUserExists(clientList, pGameReqPacket.sender, pGameReqPacket);
                    break;
                case PacketType.GAME:
                    GamePacket pGamePacket = (GamePacket)packetReceived;
                    int gameNumber = CheckGameExists(Games, pGamePacket.sender);
                    if (gameNumber == -1)
                        break;
                    else
                        if (Games[gameNumber].CheckMove(pGamePacket.x, pGamePacket.y))
                        {
                        //Need to send a nought and a cross together
                        if (pGamePacket.text == Games[gameNumber].Player1)
                        {
                            SendGamePacket(pGamePacket.sender, pGamePacket.recipient, pGamePacket.x, pGamePacket.y, pGamePacket.value, "X");
                            SendGamePacket(pGamePacket.recipient, pGamePacket.sender, pGamePacket.x, pGamePacket.y, pGamePacket.value, "O");
                        }
                        else if(pGamePacket.text == Games[gameNumber].Player2)
                        {
                            SendGamePacket(pGamePacket.recipient, pGamePacket.sender, pGamePacket.x, pGamePacket.y, pGamePacket.value, "O");
                            SendGamePacket(pGamePacket.sender, pGamePacket.recipient, pGamePacket.x, pGamePacket.y, pGamePacket.value, "X");
                        }
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
        public void CreateMessage(string message, string recipient)
        {
            Packet pSend = new ChatMessagePacket(message, recipient);
            if (recipient == "server")
                for (int i = 0; i < clientList.Count; i++)
                    clientList[i].TCPSend(pSend);
            else
                CheckUserExists(clientList, recipient, pSend);
                //client.UDPSend(p);
        }
        public void SendMessageAll(string message)
        {
            CreateMessage(message, "server");
        }

        public void SendClientList(List<string> clientList)
        { 
            Packet p = new UserListPacket(clientList);
            for (int i = 0; i < this.clientList.Count; i++)
                this.clientList[i].TCPSend(p);
        }

        public bool CheckUserExists(List<Client> clientList, string checkString, Packet packet)
        {
            for (int i = 0; i < clientList.Count; i++)
                if (clientList[i].username == checkString)
                {
                    clientList[i].TCPSend(packet);
                    return true;
                }
            return false;
        }

        public int CheckGameExists(List<GameLogic> gameList,string checkString)
        {
            for (int i = 0; i < gameList.Count; i++)
                if ((gameList[i].Player1 == checkString) || (gameList[i].Player2 == checkString))
                    return i;
            return -1;
        }

        public bool CheckGameAccepted(List<GameLogic> gameList,string checkString)
        {
            for (int i = 0; i < gameList.Count; i++)
                if (gameList[i].Player1 == checkString || gameList[i].Player2 == checkString)
                    return true;
                return false;
        }

        public void SendGamePacket(string recipient, string sender, int x, int y, int value,string text)
        {
            Packet GamePacket = new GamePacket(x, y, recipient, sender, value,text);
            for (int i = 0; i < clientList.Count; i++)
                if (clientList[i].username == recipient)
                {
                    clientList[i].UDPSend(GamePacket);
                    break;
                }

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

    class GameLogic
    {
        public string Player1;
        public string Player2;
        public int[,] GameBoard = new int[3, 3];
        int outcome;

        public GameLogic(string p1, string p2)
        {
            Player1 = p1;
            Player2 = p2;
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    GameBoard[x, y] = 0;

        }

        public bool CheckMove(int x, int y)
        {
            if (GameBoard[x, y] == 0)
            {
                GameBoard[x, y] = 1;
                return true;
            }
            else
                return false;     
        }
    }
}
