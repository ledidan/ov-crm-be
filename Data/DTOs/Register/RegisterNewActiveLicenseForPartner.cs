using System.ComponentModel.DataAnnotations;

namespace Data.DTOs
{
    public class RegisterNewPartnerForActiveLicense
    {
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        [Required]
        public required string Email { get; set; }

        [Required]
        public required CreatePartner createPartner { get; set; }
   
    }
}