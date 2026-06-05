using System;
using System.Collections.Generic;

namespace PlanningAPI.Models;

public partial class Holidaycalender
{
    public int Id { get; set; }
    public int? Year { get; set; }

    public DateTime Date { get; set; }

    public string? Type { get; set; } = null!;

    public string? Name { get; set; } = null!;

    public bool Ispublicholiday { get; set; }

    public string? State { get; set; }

    public string? Remarks { get; set; }
}
