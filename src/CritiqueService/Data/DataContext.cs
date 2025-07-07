using Microsoft.EntityFrameworkCore;
using CritiqueService.Models;
namespace CritiqueService.Data;
public class DataContext : DbContext
{
   public DbSet<Critique> Critiques { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);
    }
}
