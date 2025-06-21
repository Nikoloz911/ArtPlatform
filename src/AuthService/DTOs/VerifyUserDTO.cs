namespace AuthService.DTOs;
public class VerifyUserDTO
{
    public string Email { get; set; }
    public string? VerificationCode { get; set; }
}
