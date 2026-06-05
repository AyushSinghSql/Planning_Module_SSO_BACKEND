using System;
using System.Collections.Generic;

namespace PlanningAPI.Models;

public partial class PlEmployeeProjectMapping
{
    public string EmplId { get; set; } = null!;

    public string ProjId { get; set; } = null!;

    public virtual PlEmployee Empl { get; set; } = null!;

    public virtual EmployeeMaster EmplMaster { get; set; } = null!;

    public virtual PlProject Proj { get; set; } = null!;
}
