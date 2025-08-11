using System;
using System.Collections.Generic;

namespace WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

public partial class Objtableaccountssh
{
    public int Id { get; set; }

    public int Sttaccountssh { get; set; }

    public string System { get; set; } = null!;

    public string Usename { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Port { get; set; }

    public bool? Active { get; set; }
}
