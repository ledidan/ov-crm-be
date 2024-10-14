using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class UserRole
    {
        public int Id { get; set; }

        [Required]
        public virtual required SystemRole Role { get; set; }

        [Required]
        public virtual required ApplicationUser User { get; set; }
    }
}
