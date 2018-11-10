using System.Linq;

public class InsightArgs
{
    private readonly string[] _args;

    public SpectrumArgNames Names;

    public InsightArgs()
    {
        _args = System.Environment.GetCommandLineArgs();

        // Android fix
        if (_args == null)
            _args = new string[0];

        Names = new SpectrumArgNames();

        ScenePath = IsProvided(Names.ScenePath);
        //StartMaster = IsProvided(Names.StartMaster);
        //StartSpawner = IsProvided(Names.StartSpawner);
        //StartGameServer = IsProvided(Names.StartGameServer);
        //MasterPort = ExtractValueInt(Names.MasterPort, 5000);
        //MasterIp = ExtractValue(Names.MasterIp);
        //MachineIp = ExtractValue(Names.MachineIp);
        //AssignedPort = ExtractValueInt(Names.AssignedPort);
    }

    #region Arguments
    public bool ScenePath { get; private set; }

    /// <summary>
    /// If true, master server should be started
    /// </summary>
    public bool StartMaster { get; private set; }

    /// <summary>
    /// If true, master server should be started
    /// </summary>
    public bool StartSpawner { get; private set; }

    public bool StartGameServer { get; private set; }

    /// <summary>
    /// Port, which will be open on the master server
    /// </summary>
    public int MasterPort { get; private set; }

    /// <summary>
    /// Ip address to the master server
    /// </summary>
    public string MasterIp { get; private set; }

    /// <summary>
    /// Public ip of the machine, on which the process is running
    /// </summary>
    public string MachineIp { get; private set; }

    /// <summary>
    /// Port, assigned to the spawned process (most likely a game server)
    /// </summary>
    public int AssignedPort { get; private set; }

    #endregion

    #region Helper methods

    /// <summary>
    ///     Extracts a value for command line arguments provided
    /// </summary>
    /// <param name="argName"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
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

    public class SpectrumArgNames
    {
        public string ScenePath { get { return "-ScenePath"; } }
        //public string StartMaster { get { return "-SpectrumMaster"; } }
        //public string StartSpawner { get { return "-SpectrumSpawner"; } }
        //public string StartGameServer { get { return "-SpectrumGameServer"; } }
        //public string MasterPort { get { return "-SpectrumMasterPort"; } }
        //public string MasterIp { get { return "-SpectrumMasterIp"; } }
        //public string MachineIp { get { return "-SpectrumMachineIp"; } }
        //public string AssignedPort { get { return "-SpectrumAssignedPort"; } }

    }
}