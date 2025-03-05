

namespace Data.Entities
{
    public class EmailVerification : BaseEntity
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Token { get; set; } 
        public DateTime ExpiresAt { get; set; }
        public bool IsVerified { get; set; }
        public int UserId { get; set; } 
    }
}