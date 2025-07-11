using Contracts.Enums;

namespace SubscriptionService.Models;
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Biography { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public USER_ROLES Role { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpirity { get; set; }
    public List<Subscription>? Subscriptions { get; set; } 
}
