using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PartnerUser
    {
        public int Id { get; set; }
        public virtual required Partner Partner { get; set; }
        public virtual required ApplicationUser User { get; set; }
    }
}
