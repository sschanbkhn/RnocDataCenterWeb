using System;
using System.Collections.Generic;

namespace WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;

public partial class Outlook
{
    public int Id { get; set; }

    public int SttEmail { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool Active { get; set; }
}
