namespace PlanningAPI.Models
{

    //public class UserAccess
    //{
    //    public int UserId { get; set; }
    //    public string Username { get; set; } = null!;
    //    public bool IsActive { get; set; } = true;

    //    public int RoleId { get; set; }
    //    public Role Role { get; set; } = null!;

    //    public ICollection<UserScreenPermission> ScreenOverrides { get; set; } = new List<UserScreenPermission>();
    //    public ICollection<UserFieldPermission> FieldOverrides { get; set; } = new List<UserFieldPermission>();
    //}

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<RoleScreenPermission> ScreenPermissions { get; set; } = new List<RoleScreenPermission>();
        public ICollection<RoleFieldPermission> FieldPermissions { get; set; } = new List<RoleFieldPermission>();
    }
    public class RoleScreenPermission
    {
        public int RoleId { get; set; }
        public string ScreenCode { get; set; } = null!;

        public bool CanView { get; set; }
        public bool CanEdit { get; set; }

        public Role Role { get; set; } = null!;
    }
    public class RoleFieldPermission
    {
        public int RoleId { get; set; }
        public string FieldCode { get; set; } = null!;

        public bool CanView { get; set; }
        public bool CanEdit { get; set; }

        public Role Role { get; set; } = null!;
    }

    public class UserScreenPermission
    {
        public int UserId { get; set; }
        public string ScreenCode { get; set; } = null!;

        public bool? CanView { get; set; }
        public bool? CanEdit { get; set; }

        //public UserAccess User { get; set; } = null!;
        public User User { get; set; } = null!;
    }

    public class UserFieldPermission
    {
        public int UserId { get; set; }
        public string FieldCode { get; set; } = null!;

        public bool? CanView { get; set; }
        public bool? CanEdit { get; set; }

        //public UserAccess User { get; set; } = null!;
        public User User { get; set; } = null!;
    }
    public class PermissionResponse
    {
        public Dictionary<string, PermissionAction> Screens { get; set; } = new();
        public Dictionary<string, PermissionAction> Fields { get; set; } = new();
    }

    public class PermissionAction
    {
        public bool View { get; set; }
        public bool Edit { get; set; }
    }
    public class UserSettingsRequest
    {
        public int UserId { get; set; }

        public Dictionary<string, PermissionAction>? Screens { get; set; }
        public Dictionary<string, PermissionAction>? Fields { get; set; }
    }
    public class RoleSettingsRequest
    {
        public int RoleId { get; set; }

        public Dictionary<string, PermissionAction>? Screens { get; set; }
        public Dictionary<string, PermissionAction>? Fields { get; set; }
    }
    public class RolePermissionsResponse
    {
        public int RoleId { get; set; }
        public Dictionary<string, PermissionAction> Screens { get; set; } = new();
        public Dictionary<string, PermissionAction> Fields { get; set; } = new();
    }
    public class BulkRolePermissionRequest
    {
        public int RoleId { get; set; }

        public List<BulkScreenPermission>? Screens { get; set; }
        public List<BulkFieldPermission>? Fields { get; set; }
    }

    public class BulkScreenPermission
    {
        public string ScreenCode { get; set; } = null!;
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
    }

    public class BulkFieldPermission
    {
        public string FieldCode { get; set; } = null!;
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
    }
    public class BulkUserPermissionRequest
    {
        public int UserId { get; set; }

        public List<BulkScreenPermission>? Screens { get; set; }
        public List<BulkFieldPermission>? Fields { get; set; }
    }

    public class RoleResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;
    }

}
