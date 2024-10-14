using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class UserRole
    {
        public int Id { get; set; }

        [ForeignKey("SystemRole")]
        public int RoleId { get; set; }

        [ForeignKey("ApplicationUser")]
        public int UserId { get; set; }
    }
}
