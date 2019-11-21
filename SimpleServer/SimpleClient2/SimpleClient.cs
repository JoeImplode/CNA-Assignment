using System;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using SimpleClient2;
using SharedClassLibrary;
using System.Runtime.Serialization.Formatters.Binary;

namespace SimpleClient
{
    public class SimpleClient
    {
        public TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryReader _tcpReader;
        private BinaryWriter _tcpWriter;
        private BinaryReader _UdpReader;
        private BinaryWriter _UdpWriter;
        public UdpClient _UdpClient;
        public BinaryFormatter _formatter;
        public MemoryStream _ms;
        private Thread _readerThread;
        public ClientForm _messageForm;
        public string ipAddress;
        public int port;
        public SimpleClient()
        {
            _messageForm = new ClientForm(this);
            _tcpClient = new TcpClient();
            _UdpClient = new UdpClient();
        }

        public bool Connect(string address, int portNum)
        {
            ipAddress = address;
            //Connect the client to the ip address and port number
            port = portNum;
            _tcpClient.Connect(ipAddress,port);
            _stream = _tcpClient.GetStream();
            _UdpClient.Connect(ipAddress,port);
            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _UdpReader = new BinaryReader(_stream);
            _UdpWriter = new BinaryWriter(_stream);

            EndPoint localEndPoint = _UdpClient.Client.LocalEndPoint;

            LoginPacket tempLogin = new LoginPacket(localEndPoint);

            _formatter = new BinaryFormatter();

            tcpSend(tempLogin);

            
            _readerThread = new Thread(new ThreadStart(ProcessServerResponse));
            Application.Run(_messageForm);
            return true;
        }

        public void Run()
        {
            _readerThread.Start();
        }

        public void Stop()
        {
            _readerThread.Abort();
            _tcpClient.Close();
        }

        private void ClientLogic(Packet packet)
        {
            switch (packet.type)
            {
                case PacketType.CHATMESSAGE:
                    ChatMessagePacket pChat = (ChatMessagePacket)packet;
                    _messageForm.UpdateChatWindow(pChat.message);
                    break;
                case PacketType.NICKNAME:
                    NickNamePacket pNick = (NickNamePacket)packet;
                    _messageForm.UpdateChatWindow(pNick.nickName);
                    break;
                case PacketType.ENDPOINT:
                    LoginPacket pLogIn = (LoginPacket)packet;
                    break;
            }
        }

        public void ProcessServerResponse()
        {
            
            IPAddress iPAddress = Dns.Resolve(ipAddress).AddressList[0];
            IPEndPoint ipLocalEnd = new IPEndPoint(iPAddress, port);
            Packet sendPacket = new LoginPacket(ipLocalEnd);
            tcpSend(sendPacket);

            Packet packet;

            while (_tcpReader != null)
            {
                if ((packet = TCPRead()) != null)
                {
                    ClientLogic(packet);
                }
                if ((packet = UdpRead()) != null)
                {
                    ClientLogic(packet);
                }
            }
        }

        public void tcpSend(Packet data)
        {
            MemoryStream ms = new MemoryStream();

            _ms = new MemoryStream();
            _formatter.Serialize(_ms, data); //Serialise the data into the memory stream
            byte[] buffer = _ms.GetBuffer();
            _ms.Position = 0;

            _tcpWriter.Write(buffer.Length);
            _tcpWriter.Write(buffer);
            _tcpWriter.Flush();
        }

        public void udpSend(Packet data)
        {
            _ms = new MemoryStream();
            _formatter.Serialize(_ms, data); //Serialise the data into the memory stream
            byte[] buffer = _ms.GetBuffer();
            _ms.Position = 0;

            _UdpWriter.Write(buffer.Length);
            _UdpWriter.Write(buffer);
            _UdpWriter.Flush();
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

        public Packet TCPRead()
        {
            int noOfIncomingBytes;
            if ((noOfIncomingBytes = _tcpReader.ReadInt32()) != 0)
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

        public void CreateMessage(string message)
        {
            tcpSend(new ChatMessagePacket(message));
        }
    }
}
