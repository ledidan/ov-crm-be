



using Data.Entities;

namespace Data.DTOs
{
    public class CRMRole : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string? Description { get; set; }
        public int? PartnerId { get; set; }
        public Partner? Partner { get; set; }
        public ICollection<Employee> Employees { get; set; }
        public ICollection<CRMRolePermission> RolePermissions { get; set; }
    }
}