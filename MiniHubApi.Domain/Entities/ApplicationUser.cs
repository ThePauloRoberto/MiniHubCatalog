using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace MiniHubApi.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    public string FullName => $"{FirstName} {LastName}";
}
