using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("version_code")]
    public class VersionCode
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("version_code")]
        [StringLength(50)]
        public string VersionCodeValue { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
