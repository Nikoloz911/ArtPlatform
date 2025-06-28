using Microsoft.EntityFrameworkCore;
using PortfolioService.Models;
namespace PortfolioService.Data;
public class DataContext : DbContext
{
    public DbSet<Portfolio> Portfolios { get; set; }
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
