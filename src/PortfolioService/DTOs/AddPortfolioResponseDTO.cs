namespace PortfolioService.DTOs;
public class AddPortfolioResponseDTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly CreationDate { get; set; }
}
