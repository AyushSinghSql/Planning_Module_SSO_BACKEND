using System;
using System.Collections.Generic;

namespace PlanningAPI.Models;

public partial class PlOrgnization
{
    public string OrgId { get; set; } = null!;

    public string OrgName { get; set; } = null!;

    public decimal IcTrckngLvlNo { get; set; }

    public decimal OrgLvlsNo { get; set; }

    public string OrgAbbrvCd { get; set; } = null!;

    public string ModifiedBy { get; set; } = null!;

    public DateTime TimeStamp { get; set; }

    public string CompanyId { get; set; } = null!;

    public virtual ICollection<PlEmployee> PlEmployees { get; set; } = new List<PlEmployee>();

    public virtual ICollection<PlProject> PlProjects { get; set; } = new List<PlProject>();
}
