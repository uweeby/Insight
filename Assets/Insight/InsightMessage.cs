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
    }

    public class ErrorMsg : MessageBase
    {
        public string Text;
        public bool CauseDisconnect;
    }

    public class EmptyMsg : MessageBase {}

    public class StatusMsg : MessageBase
    {
        public string Text;
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
}
