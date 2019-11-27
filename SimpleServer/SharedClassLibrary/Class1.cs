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
        GAMEREQ,
        GAME
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
        public string recipient;
        public ChatMessagePacket(string message, string recipient)
        {
            this.type = PacketType.CHATMESSAGE;
            this.message = message;
            this.recipient = recipient;
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
    [Serializable]
    public class GameRequestPacket : Packet
    {
        public enum RequestState
        {
            DECLINED,
            ACCEPTED,
            REQUESTED,  
        }
        public string recipient;
        public RequestState requestState;
        public string sender;

        public GameRequestPacket(string recipient, RequestState state, string sender)
        {
            type = PacketType.GAMEREQ;
            this.recipient = recipient;
            requestState = state;
           this.sender = sender;
        }
    }

    [Serializable]
    public class GamePacket : Packet
    {
        public int x, y;
        public string recipient;
        public string sender;
        public int value;
        public string text;
        public GamePacket(int x, int y, string recipient, string sender, int value, string text)
        {
            this.type = PacketType.GAME;
            this.x = x;
            this.y = y;
            this.recipient = recipient;
            this.sender = sender;
            this.value = value;
            this.text = text;
        }
    }

    public class Class1
    {
    }
}
