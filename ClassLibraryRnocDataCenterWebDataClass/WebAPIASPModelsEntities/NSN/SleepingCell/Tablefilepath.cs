using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

public partial class Tablefilepath
{
    public int Id { get; set; }

    public int SttFilePath { get; set; }

    public string Oss { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Host { get; set; } = null!;

    public int Port { get; set; }

    public string Filepath { get; set; } = null!;

    public string Protocol { get; set; } = null!;

    public bool? Active { get; set; }
}
