using Mirror;

public class EmptyReply : MessageBase
{

}

public class RegisterServerConnectionMsg : MessageBase
{
    public static short MsgId = 1000;
    public string UniqueID;
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