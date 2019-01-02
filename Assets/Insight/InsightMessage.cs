using Mirror;

namespace Insight
{
    public class EmptyReply : MessageBase
    {

    }

    public class StatusMsg : MessageBase
    {
        public static short MsgId = -2;
        public string Text;
    }

    public class ErrorMsg : MessageBase
    {
        public static short MsgId = -1;
        public string Text;
        public bool CauseDisconnect;
    }

    //Test msg for ZoneModule
    public class RegisterZoneMsg : MessageBase
    {
        public static short MsgId = 1003;
        public string UniqueID;
        public string SceneName;
        public string NetworkAddress;
        public int NetworkPort;
        public int MaxPlayers;
        public int CurentPlayers;
    }

    //Test msg for ZoneModule
    public class UnregisterZoneMsg : MessageBase
    {
        public static short MsgId = 1004;
        public string UniqueID;
    }

    //Test msg for ZoneModule
    public class GetZonesMsg : MessageBase
    {
        public static short MsgId = 1005;
        public ZoneContainer[] zonesList;
    }

    //Test msg for ChatServer
    public class ChatMessage : MessageBase
    {
        public static short MsgId = 1006;
        public string Data;
        public string Origin;
        public string Target; //If this was a private chat
    }

    //Test msg for LoginServer
    public class LoginMsg : MessageBase
    {
        public static short MsgId = 1007;
        public string AccountName;
        public string AccountPassword;
        public string ClientVersion;
    }

    //Test msg for LoadTest
    public class ServerLoadTestMsg : MessageBase
    {
        public static short MsgId = 1008;
        public char[] Payload;
    }

    //Test msg for LoadTest
    public class ClientLoadTestMsg : MessageBase
    {
        public static short MsgId = 1009;
        public char[] Payload;
    }

    //Test msg for LoadTest
    public class ServerHealthMsg : MessageBase
    {
        public static short MsgId = 1010;
        public float CPULoadPercent;
        public float RAMLoadPercent;
        public float NETLoadPercent;
    }

    //Test msg for SpawnerExample
    public class SpawnDataMessage : MessageBase
    {
        public static short MsgId = 7003;
        public string GameName; //or SceneName
        public string UniqueID;
        public string NetworkAddress;
        public int NetworkPort;
    }
}