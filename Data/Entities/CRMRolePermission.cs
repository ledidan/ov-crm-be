



using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class CRMRolePermission
    {
        public int RoleId { get; set; }
        public CRMRole Role { get; set; }

        public int PermissionId { get; set; }
        public CRMPermission Permission { get; set; }
    }
}