using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class PartnerUser
    {
        public int Id { get; set; }
        public int PartnerId { get; set; }

        [ForeignKey("ApplicationUser")]
        public int UserId { get; set; }
    }
}
