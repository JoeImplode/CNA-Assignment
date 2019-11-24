using System;
using System.Collections.Generic;
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
        public int index;
        public ChatMessagePacket(string message, int index = 0)
        {
            this.type = PacketType.CHATMESSAGE;
            this.message = message;
            this.index = index;
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
    [Serializable]
    public class LoginPacket : Packet
    {
        public EndPoint endPoint;
        public LoginPacket(EndPoint _endPoint)
        {
            this.type = PacketType.ENDPOINT;
            endPoint = _endPoint;
        }
    }
    [Serializable]
    public class UserListPacket : Packet
    {
        public List<string> userList;
        public UserListPacket(List<string> _userList)
        {
            this.type = PacketType.USERLIST;
            userList = _userList;
        }
    }
    public class Class1
    {
    }
}
