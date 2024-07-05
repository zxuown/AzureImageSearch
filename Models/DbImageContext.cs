using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.EntityFrameworkCore;

namespace AzureImageSearch.Models;

public class DbImageContext : DbContext
{
    public DbImageContext(DbContextOptions options) : base(options){ }

    public virtual DbSet<Image> Images { get; set; } = null!;

    public virtual DbSet<Tag> Tags { get; set; } = null!;
}
