namespace ArtworkService.DTOs;
public class GetArtworksFilterDTO
{
    public string? Title { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? CreationTime { get; set; }
}
