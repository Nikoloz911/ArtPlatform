namespace AuthService.DTOs;
public class LoginResponseDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Biography { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string Role { get; set; }
    public bool IsVerified { get; set; }
    public string Token { get; set; }
}
