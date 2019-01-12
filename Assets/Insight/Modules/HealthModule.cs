using Insight;

public class HealthModule : InsightModule
{
    InsightClient insight;

    public float SendRateInSeconds = 60;

    public override void Initialize(InsightClient insight, ModuleManager manager)
    {
        this.insight = insight;

        InvokeRepeating("SendServerData", SendRateInSeconds, SendRateInSeconds);
    }


    private void SendHealth()
    {
        insight.Send(ServerHealthMsg.MsgId, new ServerHealthMsg() { CPULoadPercent = 0.5f, RAMLoadPercent = 0.4f, NETLoadPercent = 0.3f }); //Test values
    }
}