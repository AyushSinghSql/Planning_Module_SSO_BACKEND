using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace PlanningAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Role { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int RoleId { get; set; }
        [NotMapped]
        public string? ProjectName { get; set; }

        [NotMapped]
        public string? ProjecId { get; set; }

        //public ICollection<ApprovalRequest> ApprovalRequests { get; set; } = new List<ApprovalRequest>();
        public ICollection<UserProjectMap> UserProjects { get; set; }
                = new List<UserProjectMap>();
        public ICollection<OrgGroupUserMapping> OrgGroupMappings { get; set; }
                = new List<OrgGroupUserMapping>();

        //public ICollection<UserOrgMapping> UserOrgMapping { get; set; }
        //        = new List<UserOrgMapping>();

        public Role UserRole { get; set; } = null!;

        public ICollection<UserScreenPermission> ScreenOverrides { get; set; } = new List<UserScreenPermission>();
        public ICollection<UserFieldPermission> FieldOverrides { get; set; } = new List<UserFieldPermission>();

    }

    public class UserProjectMap
    {
        public int UserId { get; set; }
        public string ProjId { get; set; } = null!;
        public DateTime AssignedAt { get; set; }

        public User User { get; set; } = null!;
        public PlProject Project { get; set; } = null!;
    }

    public class UserGroupMap
    {
        public int UserId { get; set; }
        public string ProjId { get; set; } = null!;
        public DateTime AssignedAt { get; set; }

        public User User { get; set; } = null!;
        public OrgGroup OrgGroup { get; set; } = null!;
    }

    public class OrgGroupUserMapping
    {
        public int OrgGroupId { get; set; }
        public int UserId { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime? AssignedAt { get; set; }
        public string? AssignedBy { get; set; }

        public OrgGroup OrgGroup { get; set; }
        public User User { get; set; }
    }

    //[Table("user_org_mapping", Schema = "public")]
    //public class UserOrgMapping
    //{
    //    public string OrgId { get; set; }
    //    public int UserId { get; set; }
    //    public PlOrgnization Orgnization { get; set; }
    //    public User User { get; set; }
    //}



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class UserConfiguration
    {
        public int UserId { get; set; }
        public Visibility Visibility { get; set; }
        public string configType { get; set; }
    }

    public class Visibility
    {
        public bool projectHours { get; set; }

        [JsonProperty("projectHours.idType")]
        public bool projectHoursidType { get; set; }

        [JsonProperty("projectHours.emplId")]
        public bool projectHoursemplId { get; set; }

        [JsonProperty("projectHours.warning")]
        public bool projectHourswarning { get; set; }

        [JsonProperty("projectHours.name")]
        public bool projectHoursname { get; set; }

        [JsonProperty("projectHours.acctId")]
        public bool projectHoursacctId { get; set; }

        [JsonProperty("projectHours.acctName")]
        public bool projectHoursacctName { get; set; }

        [JsonProperty("projectHours.orgId")]
        public bool projectHoursorgId { get; set; }

        [JsonProperty("projectHours.glcPlc")]
        public bool projectHoursglcPlc { get; set; }

        [JsonProperty("projectHours.isRev")]
        public bool projectHoursisRev { get; set; }

        [JsonProperty("projectHours.isBrd")]
        public bool projectHoursisBrd { get; set; }

        [JsonProperty("projectHours.status")]
        public bool projectHoursstatus { get; set; }

        [JsonProperty("projectHours.perHourRate")]
        public bool projectHoursperHourRate { get; set; }

        [JsonProperty("projectHours.total")]
        public bool projectHourstotal { get; set; }
        public bool projectAmounts { get; set; }

        [JsonProperty("projectAmounts.idType")]
        public bool projectAmountsidType { get; set; }

        [JsonProperty("projectAmounts.emplId")]
        public bool projectAmountsemplId { get; set; }

        [JsonProperty("projectAmounts.name")]
        public bool projectAmountsname { get; set; }

        [JsonProperty("projectAmounts.acctId")]
        public bool projectAmountsacctId { get; set; }

        [JsonProperty("projectAmounts.acctName")]
        public bool projectAmountsacctName { get; set; }

        [JsonProperty("projectAmounts.orgId")]
        public bool projectAmountsorgId { get; set; }

        [JsonProperty("projectAmounts.isRev")]
        public bool projectAmountsisRev { get; set; }

        [JsonProperty("projectAmounts.isBrd")]
        public bool projectAmountsisBrd { get; set; }

        [JsonProperty("projectAmounts.status")]
        public bool projectAmountsstatus { get; set; }

        [JsonProperty("projectAmounts.total")]
        public bool projectAmountstotal { get; set; }
    }




    public class BulkProjectUserToggleRequest
    {
        public string ProjId { get; set; } = null!;
        public List<int> UserIds { get; set; } = new();
    }
    public class BulkUserGroupsToggleRequest
    {
        public int UserId { get; set; }
        public List<int> GroupIds { get; set; } = new();
    }

}
