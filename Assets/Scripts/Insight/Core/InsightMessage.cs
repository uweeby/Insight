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

    //Test msgs for Demo only
    public class ClientToZoneTestMsg : MessageBase
    {
        public static short MsgId = 1000;
        public string Source;
        public string Desintation;
        public string Data;
    }

    //Test msgs for Demo only
    public class ClientToMasterTestMsg : MessageBase
    {
        public static short MsgId = 1001;
        public string Source;
        public string Desintation;
        public string Data;
    }

    //Test msgs for Demo only
    public class ZoneToMasterTestMsg : MessageBase
    {
        public static short MsgId = 1002;
        public string Source;
        public string Desintation;
        public string Data;
    }

    //Test msg for ZoneModule
    public class RegisterZoneMsg : MessageBase
    {
        public static short MsgId = 9090;
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
        public static short MsgId = 9091;
        public string UniqueID;
    }

    //Test msg for ZoneModule
    public class GetZonesMsg : MessageBase
    {
        public static short MsgId = 9092;
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
        public static short MsgId = 1000;
        public string AccountName;
        public string AccountPassword;
        public string ClientVersion;
    }
}