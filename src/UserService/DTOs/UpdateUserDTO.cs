namespace UserService.DTOs;
public class UpdateUserDTO
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Biography { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
