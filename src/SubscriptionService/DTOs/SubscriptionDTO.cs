namespace SubscriptionService.DTOs;
public class SubscriptionDTO
{
    public int Id { get; set; }
    public int ArtistId { get; set; } // Refers to the user being subscribed to
    public DateTime CreationDate { get; set; }
}
