namespace ArtworkService.DTOs;
public class UpdateArtworkResponseDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string CategoryName { get; set; }
    public DateTime CreationTime { get; set; }
    public string ImageAdress { get; set; }
}
