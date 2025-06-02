namespace UserService.Models;
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Biography { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
