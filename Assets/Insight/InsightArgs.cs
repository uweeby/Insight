using System.Linq;

public class InsightArgs
{
    private readonly string[] _args;

    public InsightArgNames Names;

    public InsightArgs()
    {
        _args = System.Environment.GetCommandLineArgs();

        // Android fix
        if (_args == null)
            _args = new string[0];

        Names = new InsightArgNames();

        SceneName = IsProvided(Names.SceneName);
        MasterPort = ExtractValueInt(Names.MasterPort, 5000);
        MasterIp = ExtractValue(Names.MasterIp);
        //NetworkAddress = ExtractValue(Names.NetworkAddress);
        NetworkPort = ExtractValueInt(Names.NetworkPort);
    }

    public string UniqueID { get; private set; }
    public bool SceneName { get; private set; }
    public int MasterPort { get; private set; }
    public string MasterIp { get; private set; }
    public string NetworkAddress { get; private set; }
    public int NetworkPort { get; private set; }

    #region Helper methods
    public string ExtractValue(string argName, string defaultValue = null)
    {
        if (!_args.Contains(argName))
            return defaultValue;

        var index = _args.ToList().FindIndex(0, a => a.Equals(argName));
        return _args[index + 1];
    }

    public int ExtractValueInt(string argName, int defaultValue = -1)
    {
        var number = ExtractValue(argName, defaultValue.ToString());
        return System.Convert.ToInt32(number);
    }

    public bool IsProvided(string argName)
    {
        return _args.Contains(argName);
    }
    #endregion

    public class InsightArgNames
    {
        public string UniqueID { get { return "-UniqueID"; } }
        public string SceneName { get { return "-SceneName"; } }
        public string MasterPort { get { return "-MasterPort"; } }
        public string MasterIp { get { return "-MasterIp"; } }
        //public string NetworkAddress { get { return "-NetworkAddress"; } } //The server would never get to decide its own IP
        public string NetworkPort { get { return "-NetworkPort"; } }
    }
}