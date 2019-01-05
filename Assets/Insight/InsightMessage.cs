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
}