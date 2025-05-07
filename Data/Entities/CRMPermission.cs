


namespace Data.DTOs
{
    public class CRMPermission
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public string Resource { get; set; }

        public ICollection<CRMRolePermission> RolePermissions { get; set; }
    }
}