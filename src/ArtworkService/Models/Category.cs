namespace ArtworkService.Models;
public class Category
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public DateTime CreationDate { get; set; }
    public string ImageURL { get; set; }
    public List<Artwork> Artwork { get; set; }
}
