using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

public partial class Objtablescheduler
{
    public int Id { get; set; }

    public int Sttschedulertime { get; set; }

    public string Starttime { get; set; } = null!;

    public string Endtime { get; set; } = null!;

    public bool? Active { get; set; }
}
