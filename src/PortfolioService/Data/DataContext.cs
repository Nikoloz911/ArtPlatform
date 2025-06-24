using Microsoft.EntityFrameworkCore;
using PortfolioService.Models;
namespace PortfolioService.Data;
public class DataContext : DbContext
{
    public DbSet<Portfolio> Portfolios { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=MicroService3;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
    }
}
