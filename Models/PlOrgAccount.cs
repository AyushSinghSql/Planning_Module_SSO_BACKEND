using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    public class OrgAccount
    {
        [Key, Column(Order = 0)]
        [MaxLength(20)]
        public string OrgId { get; set; }

        [Key, Column(Order = 1)]
        [MaxLength(20)]
        public string AcctId { get; set; }

        [MaxLength(30)]
        public string? AccType { get; set; }

        public bool? ActiveFl { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime TimeStamp { get; set; }

        [JsonIgnore]
        public ICollection<PlPoolRate>? PlPoolRates { get; set; }
        [JsonIgnore]
        public Account? Account { get; set; }
        [JsonIgnore]
        public Organization? Organization { get; set; }
    }

}
