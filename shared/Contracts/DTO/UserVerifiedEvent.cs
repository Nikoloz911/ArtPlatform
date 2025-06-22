namespace Contracts.DTO;
public class UserVerifiedEvent
{
    public int Id { get; set; }
    public bool IsVerified { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime? VerificationCodeExpirity { get; set; }
}
