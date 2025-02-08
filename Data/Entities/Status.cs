


using System.ComponentModel.DataAnnotations;

namespace Data.Entities
{
    public class Status : BaseEntity
    {
        public int Id { get; set; } 

        public string Name { get; set; } = string.Empty; 

        public string? Description { get; set; } 

    }
}