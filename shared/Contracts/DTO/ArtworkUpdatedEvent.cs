namespace Contracts.DTO;
public class ArtworkUpdatedEvent
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; }
    public DateTime CreationTime { get; set; }
    public string ImageAdress { get; set; }
}
