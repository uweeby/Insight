using System;
using System.Linq;

public class InsightArgs
{
    private readonly string[] _args;

    public ArgNames Names;

    public InsightArgs()
    {
        _args = Environment.GetCommandLineArgs();

        Names = new ArgNames();

        StartMaster = IsProvided(Names.StartMaster);
        MasterPort = ExtractValueInt(Names.MasterPort, 5000);
        MasterIp = ExtractValue(Names.MasterIp);
        MachineIp = ExtractValue(Names.MachineIp);
        DestroyUi = IsProvided(Names.DestroyUi);

        AssignedPort = ExtractValueInt(Names.AssignedPort, -1);
        UniqueID = ExtractValue(Names.UniqueID);
        ExecutablePath = ExtractValue(Names.ExecutablePath);
        DontSpawnInBatchmode = IsProvided(Names.DontSpawnInBatchmode);
        MaxProcesses = ExtractValueInt(Names.MaxProcesses, 0);
        SceneName = ExtractValue(Names.SceneName);
    }

    #region Arguments
    public bool StartMaster { get; private set; }
    public int MasterPort { get; private set; }
    public string MasterIp { get; private set; }
    public string MachineIp { get; private set; }
    public bool DestroyUi { get; private set; }

    public int AssignedPort { get; private set; }
    public string UniqueID { get; private set; }
    public string ExecutablePath { get; private set; }
    public bool DontSpawnInBatchmode { get; private set; }
    public int MaxProcesses { get; private set; }
    public string SceneName { get; private set; }
    #endregion

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
        return Convert.ToInt32(number);
    }

    public bool IsProvided(string argName)
    {
        return _args.Contains(argName);
    }

    #endregion

    public class ArgNames
    {
        public string StartMaster { get { return "-StartMaster"; } }
        public string MasterPort { get { return "-MasterPort"; } }
        public string MasterIp { get { return "-MasterIp"; } }
        public string MachineIp { get { return "-MachineIp"; } }
        public string DestroyUi { get { return "-DestroyUi"; } }

        public string StartSpawner { get { return "-StartSpawner"; } }

        public string AssignedPort { get { return "-AssignedPort"; } }
        public string UniqueID { get { return "-UniqueID"; } }
        public string ExecutablePath { get { return "-Exe"; } }
        public string DontSpawnInBatchmode { get { return "-DontSpawnInBatchmode"; } }
        public string MaxProcesses { get { return "-MaxProcesses"; } }
        public string SceneName { get { return "-SceneName"; } }
    }
}
