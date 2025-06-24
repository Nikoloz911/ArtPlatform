namespace ArtworkService.DTOs;
public class ArtworkResponseDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Category { get; set; }
    public DateTime CreationTime { get; set; }
    public string ImageAdress { get; set; }
}
