using Insight;
using Mirror;
using System;
using System.Collections.Generic;

public class ZoneModule : InsightModule
{
    InsightServer insightServer;

    public List<ZoneContainer> ZoneList = new List<ZoneContainer>();

    public override void Initialize(InsightServer server)
    {
        insightServer = server;
    }

    public override void RegisterHandlers()
    {
        insightServer.RegisterHandler(RegisterServerConnectionMsg.MsgId, HandleRegisterServerConnectionMsg);

        insightServer.RegisterServerHandler(RegisterZoneMsg.MsgId, HandleRegisterZoneMsg);
        insightServer.RegisterServerHandler(UnregisterZoneMsg.MsgId, HandleUnregisterZoneMsg);

        insightServer.RegisterServerHandler(GetZonesMsg.MsgId, HandleGetZonesMsg);
    }

    private void HandleRegisterServerConnectionMsg(InsightNetworkMessage netMsg)
    {
        print("HandleRegisterServerConnectionMsg");

        RegisterServerConnectionMsg message = netMsg.ReadMessage<RegisterServerConnectionMsg>();

        print("Received UniqueID: " + message.UniqueID);

        //This needs to verify it should actually be adding the handlers

        insightServer.SetServerHandlers(netMsg.conn);

        netMsg.conn.Send(RegisterServerConnectionMsg.MsgId, new EmptyReply());
    }

    private void HandleRegisterZoneMsg(InsightNetworkMessage netMsg)
    {
        print("HandleRegisterZoneMsg");

        RegisterZoneMsg message = netMsg.ReadMessage<RegisterZoneMsg>();

        ZoneList.Add(new ZoneContainer()
        {
            UniqueID = message.UniqueID,
            ScenePath = message.ScenePath,
            NetworkAddress = message.NetworkAddress,
            NetworkPort = message.NetworkPort,
            MaxPlayers = message.MaxPlayers,
            CurentPlayers = message.CurentPlayers
        });
    }

    private void HandleUnregisterZoneMsg(InsightNetworkMessage netMsg)
    {
        print("HandleUnregisterZoneMsg");

        UnregisterZoneMsg message = netMsg.ReadMessage<UnregisterZoneMsg>();

        foreach(ZoneContainer zone in ZoneList)
        {
            if(zone.UniqueID.Equals(message.UniqueID))
            {
                ZoneList.Remove(zone);
            }
        }
    }

    private void HandleGetZonesMsg(InsightNetworkMessage netMsg)
    {
        //Request from Server for a list of all active Zones

        netMsg.conn.Send(GetZonesMsg.MsgId, new GetZonesMsg() { zonesList = ZoneList.ToArray() });
    }
}

[Serializable]
public struct ZoneContainer
{
    public string UniqueID;
    public string ScenePath;
    public string NetworkAddress;
    public int NetworkPort;
    public int MaxPlayers; //Distated by the current zone
    public int CurentPlayers;
}

public class RegisterZoneMsg : MessageBase
{
    public static short MsgId = 9090;
    public string UniqueID;
    public string ScenePath;
    public string NetworkAddress;
    public int NetworkPort;
    public int MaxPlayers;
    public int CurentPlayers;
}

public class UnregisterZoneMsg : MessageBase
{
    public static short MsgId = 9091;
    public string UniqueID;
}

public class GetZonesMsg : MessageBase
{
    public static short MsgId = 9092;
    public ZoneContainer[] zonesList;
}