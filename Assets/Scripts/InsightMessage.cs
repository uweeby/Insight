using Mirror;

public class ClientToZoneTestMsg : MessageBase
{
    public static short MsgId = 1000;
    public string Source;
    public string Desintation;
    public string Data;
}

public class ClientToMasterTestMsg : MessageBase
{
    public static short MsgId = 1001;
    public string Source;
    public string Desintation;
    public string Data;
}

public class ZoneToMasterTestMsg : MessageBase
{
    public static short MsgId = 1002;
    public string Source;
    public string Desintation;
    public string Data;
}
