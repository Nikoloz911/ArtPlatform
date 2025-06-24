namespace ArtworkService.DTOs;
public class GetArtworksFilterDTO
{
    public string? Title { get; set; }
    public string? Category { get; set; }
    public DateTime? CreationTime { get; set; }
}
