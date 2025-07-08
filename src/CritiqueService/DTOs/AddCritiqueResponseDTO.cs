namespace CritiqueService.DTOs;
public class AddCritiqueResponseDTO
{
    public int Id { get; set; }
    public int ArtworkId { get; set; }
    public double Rating { get; set; }
    public string Text { get; set; }
    public DateTime CreationDate { get; set; }
}
