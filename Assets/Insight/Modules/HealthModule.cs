using UnityEngine;

using System.Diagnostics;
using System.Collections;

using Insight;

public class HealthModule : InsightModule
{
    InsightClient client;
    InsightServer server;

    ModuleManager manager;

    public float SendRateInSeconds = 60;

    public UnityEngine.UI.Text output;

    public override void Initialize(InsightClient client, ModuleManager manager)
    {
        this.client = client;
        this.manager = manager;

    }

    public override void Initialize(InsightServer server, ModuleManager manager)
    {
        this.server = server;
        this.manager = manager;
    }

    public override void RegisterHandlers()
    {
        if(server) server.RegisterHandler(ServerHealthMsg.MsgId, ServerHealthMsgHandler);
        if(client)
        {
            StartCoroutine(SendHealth());
        }
    }

    IEnumerator SendHealth()
    {
        var process = Process.GetCurrentProcess();
        var cpuCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);// new PerformanceCounter("Processor", "% Processor Time", "_Total");
        var ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        float cpuLoad = 0;
        float ramLoad = 0;

        while (true)
        {
            cpuLoad = cpuCounter.NextValue();
            ramLoad = ramCounter.NextValue();

            output.text = "cpuLoad: " + cpuLoad + " ramLoad: " + ramLoad;
            //UnityEngine.Debug.Log("cpuLoad: " + cpuLoad + " ramLoad: " + ramLoad, this);

            if(client && client.isConnected) client.Send(ServerHealthMsg.MsgId, new ServerHealthMsg() { CPULoadPercent = cpuLoad, RAMLoadPercent = ramLoad, NETLoadPercent = 0.3f }); //Test values
            yield return new WaitForSecondsRealtime(SendRateInSeconds);
        }
    }


    private void ServerHealthMsgHandler(InsightNetworkMessage netMsg)
    {
        var msg = netMsg.reader.ReadMessage<ServerHealthMsg>();
        UnityEngine.Debug.Log("Client Health: " + msg.CPULoadPercent + "//" + msg.RAMLoadPercent, this);
    }

}