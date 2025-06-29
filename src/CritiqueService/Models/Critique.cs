namespace CritiqueService.Models;
public class Critique
{
    public int Id { get; set; }
    public double Rating { get; set; }
    public string Text { get; set; }
    public DateTime CreationDate { get; set; }
}
