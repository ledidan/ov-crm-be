using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class JobTitleGroup
    {
        public int Id { get; set; }

        public required string JobTitleGroupCode { get; set; }

        public required string JobTitleGroupName { get; set; }

        public int? PartnerId { get; set; }
        public Partner Partner { get; set; }
    }


}
