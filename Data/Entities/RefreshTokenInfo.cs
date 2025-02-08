namespace Data.Entities
{
    public class RefreshTokenInfo : BaseEntity
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public int UserId { get; set; }
    }
}
