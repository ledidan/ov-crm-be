using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PartnerUser : BaseEntity
    {
        public int Id { get; set; }
        public virtual required Partner Partner { get; set; }
        public virtual required ApplicationUser User { get; set; }

        public int? EmployeeId { get; set; }
    }
}
