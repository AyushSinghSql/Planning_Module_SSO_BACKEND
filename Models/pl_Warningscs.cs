namespace PlanningAPI.Models
{
    //public class PlWarning
    //{
    //    public int PlId { get; set; }
    //    public string ProjId { get; set; } = string.Empty;
    //    public string EmplId { get; set; } = string.Empty;
    //    public int Year { get; set; }
    //    public int Month { get; set; }
    //    public string Warning { get; set; } = string.Empty;
    //}

    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("pl_warnings", Schema = "public")]
    public class PlWarning
    {
        [Column("pl_id")]
        [Required]
        public int PlId { get; set; }

        [Column("proj_id")]
        [Required]
        [MaxLength(100)]
        public string ProjId { get; set; } = string.Empty;

        [Column("empl_id")]
        [Required]
        [MaxLength(100)]
        public string EmplId { get; set; } = string.Empty;

        [Column("year")]
        [Required]
        public int Year { get; set; }

        [Column("month")]
        [Required]
        public int Month { get; set; }

        [Column("warning")]
        [MaxLength(300)]
        public string? Warning { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Column("multiple_projects")]
        public bool MultipleProjects { get; set; } = false;
    }


}
