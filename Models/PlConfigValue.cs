using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models;

public class PlConfigValue
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? ProjId { get; set; }

    //Optional: navigation property if you have a Project model
    //[JsonIgnore]
    //public virtual PlProject Project { get; set; }
}
