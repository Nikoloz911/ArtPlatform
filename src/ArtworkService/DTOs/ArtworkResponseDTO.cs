namespace ArtworkService.DTOs;
public class ArtworkResponseDTO
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; }
    public string CategoryName { get; set; }
    public DateTime CreationTime { get; set; }
    public string ImageAdress { get; set; }
}
