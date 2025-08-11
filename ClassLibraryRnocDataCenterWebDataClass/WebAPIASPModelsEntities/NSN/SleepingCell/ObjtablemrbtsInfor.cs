using System;
using System.Collections.Generic;

namespace ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;

public partial class ObjtablemrbtsInfor
{
    public int Id { get; set; }

    public int? Stt { get; set; }

    public string? Mrbtsname { get; set; }

    public string? Oam { get; set; }

    public decimal? MrbtsId { get; set; }

    public string? Enodebname { get; set; }

    public string? Note { get; set; }

    public bool? Reset { get; set; }

    public bool? Blacklist { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Vendor { get; set; }

    public virtual ICollection<Objtable4gkpireportresultdetail> Objtable4gkpireportresultdetails { get; set; } = new List<Objtable4gkpireportresultdetail>();
}
