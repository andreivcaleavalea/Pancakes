using System.ComponentModel.DataAnnotations;

namespace UserService.Models.Entities
{
    public class Friendship
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        [Required]
        public string SenderId { get; set; } = string.Empty; // User who sent the friend request
        
        [Required]
        public string ReceiverId { get; set; } = string.Empty; // User who received the friend request
        
        [Required]
        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
    }
}
