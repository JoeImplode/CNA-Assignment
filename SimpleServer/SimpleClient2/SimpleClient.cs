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

namespace SimpleClient
{
    public class SimpleClient
    {
        public TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryReader _tcpReader;
        private BinaryWriter _tcpWriter;

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
            port = portNum;

            _tcpClient.Connect(ipAddress,port);
            _stream = _tcpClient.GetStream();
           
            _tcpReader = new BinaryReader(_stream);
            _tcpWriter = new BinaryWriter(_stream);
            _formatter = new BinaryFormatter();

            _UdpClient.Connect(ipAddress, port);
            LoginPacket tempLogin = new LoginPacket(_UdpClient.Client.LocalEndPoint);

            tcpSend(tempLogin);

            _readerThread = new Thread(new ThreadStart(TCPRead));
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
                    _UdpClient.Connect((IPEndPoint)pLogIn.endPoint);
                    Thread t = new Thread(UdpRead);
                    t.Start();
                    break;
            }
        }
        /*
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
        */
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
            _formatter.Serialize(_ms, data);
            byte[] buffer = _ms.GetBuffer();
            _UdpClient.Send(buffer,buffer.Length);
        }

        public void UdpRead()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] buffer = _UdpClient.Receive(ref endPoint);
                _ms = new MemoryStream(buffer);
                Packet packet = _formatter.Deserialize(_ms) as Packet;
                ClientLogic(packet);
            }
        }

        public void TCPRead()
        {
            int noOfIncomingBytes;
            while ((noOfIncomingBytes = _tcpReader.ReadInt32()) != 0)
            {
                _ms = new MemoryStream(noOfIncomingBytes);
                byte[] buffer = _tcpReader.ReadBytes(noOfIncomingBytes);
                _ms.Write(buffer, 0, noOfIncomingBytes);
                _ms.Position = 0;
                Packet packet = _formatter.Deserialize(_ms) as Packet;
                ClientLogic(packet);
            }
           
        }

        public void CreateMessage(string message)
        {
            tcpSend(new ChatMessagePacket(message));
            udpSend(new ChatMessagePacket(message));
        }
    }
}
