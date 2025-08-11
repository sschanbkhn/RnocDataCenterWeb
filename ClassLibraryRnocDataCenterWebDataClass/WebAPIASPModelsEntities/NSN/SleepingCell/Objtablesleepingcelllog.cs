using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

public partial class Objtablesleepingcelllog
{
    public long Id { get; set; }

    public DateOnly Date { get; set; }

    public int? Todayanalysis { get; set; }

    public int? Sleepingcells { get; set; }

    public int? Processcells { get; set; }

    public int? Executioncells { get; set; }

    public int? Recheckcells { get; set; }

    public DateTime? Createdat { get; set; }
}
