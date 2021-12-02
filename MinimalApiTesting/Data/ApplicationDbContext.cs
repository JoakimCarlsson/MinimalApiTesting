using Microsoft.EntityFrameworkCore;
using MinimalApiTesting.Models;

namespace MinimalApiTesting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)  : base(options) { }
        public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    }
}
