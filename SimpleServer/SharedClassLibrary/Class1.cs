using System;
using System.Net;

namespace SharedClassLibrary {
    public enum PacketType
    {
        EMPTY,
        CHATMESSAGE,
        NICKNAME,
        USERLIST,
        ENDPOINT,
    }
    [Serializable]
    public class Packet
    {
        public PacketType type = PacketType.EMPTY;
    }

    [Serializable]
    public class ChatMessagePacket : Packet
    {
        public string message = string.Empty;
        public ChatMessagePacket (string message)
        {
            this.type = PacketType.CHATMESSAGE;
            this.message = message;
        }
    }
    [Serializable]
    public class NickNamePacket : Packet
    {
        public string nickName = string.Empty;

        public NickNamePacket(string nickName)
        {
            this.type = PacketType.NICKNAME;
            this.nickName = nickName;
        }
    }

    public class LoginPacket : Packet
    {
        public EndPoint endPoint;

        public LoginPacket(EndPoint _endPoint)
        {
            endPoint = _endPoint;
        }
    }

    public class Class1
    {
    }
}
