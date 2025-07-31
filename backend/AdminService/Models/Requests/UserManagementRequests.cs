using System.ComponentModel.DataAnnotations;

namespace AdminService.Models.Requests
{
    public class UserSearchRequest
    {
        public string? SearchTerm { get; set; }
        public string? Email { get; set; }
        public string? Provider { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsBanned { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "desc";
    }

    public class BanUserRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
        
        public DateTime? ExpiresAt { get; set; } // Null for permanent ban
        
        public bool BanEmail { get; set; } = false;
        
        public bool DeleteContent { get; set; } = false;
    }

    public class UnbanUserRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
    }

    public class UpdateUserRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        public string Bio { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class ForcePasswordResetRequest
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
        
        public bool SendEmail { get; set; } = true;
    }

    public class MergeUsersRequest
    {
        [Required]
        public string PrimaryUserId { get; set; } = string.Empty;
        
        [Required]
        public string SecondaryUserId { get; set; } = string.Empty;
        
        [Required]
        [MinLength(10)]
        public string Reason { get; set; } = string.Empty;
    }
}