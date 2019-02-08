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
        StartMatchMaking,
        StopMatchMaking,

        ChangeServers,
        SpawnerStatus,
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
        public int MaxThreads;
    }

    public class RegisterGameMsg : MessageBase
    {
        public string UniqueID; //Guid
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
    }

    public class RequestSpawn : MessageBase
    {
        public string ProcessAlias;
        public string SceneName;

        //Only valid in the reply from the spawner. So it should be moved to another message
        public string UniqueID; //Guid
        public string NetworkAddress; 
    }

    public class StartMatchMaking : MessageBase
    {

    }

    public class StopMatchMaking : MessageBase
    {

    }

    //Used to tell a player to connect to a new game server
    public class ChangeServers : MessageBase
    {
        public string NetworkAddress;
        public ushort NetworkPort;
        public string SceneName;
    }

    //Updates the MasterSpawner with current status
    public class SpawnerStatus : MessageBase
    {
        public int CurrentThreads;
    }
}
