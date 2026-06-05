using System;
using System.Collections.Generic;

namespace PlanningAPI.Models;

public partial class PlcCode
{
    public string LaborCategoryCode { get; set; } = null!;

    public string? Description { get; set; }

    public bool? Active { get; set; }

    public DateOnly? CreatedAt { get; set; }
}

public partial class PlcCodeDTO
{
    public string LaborCategoryCode { get; set; } = null!;

    public string? Description { get; set; }

}
