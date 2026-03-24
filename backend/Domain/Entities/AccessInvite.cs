namespace Domain.Entities
{
    public class AccessInvite : BaseEntity
    {
        public Guid BuildingId { get; set; }
        public Building Building { get; set; } = null!;
        public Guid CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;
        public AccessRole Role { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public Guid? UsedByUserId { get; set; }
        public User? UsedByUser { get; set; }
    }
}
