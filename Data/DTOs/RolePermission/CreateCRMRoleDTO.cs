


namespace Data.DTOs
{
    public class CreateCRMRoleDto
    {
        public string Name { get; set; }

        public string? Description { get; set; }

        public int? SourceRoleId {get; set;}
    }
}