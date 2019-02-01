using Mirror;

namespace Insight
{
    public enum MsgId : short
    {
        Error = -1,
        Empty,
        Status,

        Login,
        Chat,

        RegisterSpawner,
        RegisterGame,

        RequestSpawn,
        RequestGame,
        RequestMatch,
    }

    public class ErrorMsg : MessageBase
    {
        public string Text;
        public bool CauseDisconnect;
    }

    public class EmptyMsg : MessageBase { }

    public class StatusMsg : MessageBase
    {
        public string Text;
    }

    public class PropertiesMsg : MessageBase
    {
        public string SceneName;
        public string GameType;
        public int MaxPlayers;
        public bool IsPublic;
        public string GamePassword;
    }

    public class LoginMsg : MessageBase
    {
        public string AccountName;
        public string AccountPassword;
    }

    public class ChatMsg : MessageBase
    {
        public string Data;
        public string Origin;
        public string Target; //Used for private chat
    }

    public class RegisterSpawnerMsg : MessageBase
    {
        public string UniqueID; //Guid
    }

    public class RegisterGameMsg : MessageBase
    {
        public string UniqueID; //Guid
    }

    public class RequestSpawn : MessageBase
    {
        public string SpawnAlias;
        public string GameName; //or SceneName
        public string UniqueID;
        public string NetworkAddress;
    }

    public class RequestMatch : MessageBase
    {
        public string GameName;
        public int GameType;
        public int GameSlots;
    }
}
