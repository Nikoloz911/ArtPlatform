namespace CategoryService.DTOs;
public class UpdateCategoryResponseDTO
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public DateTime CreationDate { get; set; }
    public string ImageURL { get; set; }
}
