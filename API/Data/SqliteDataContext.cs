using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class SqliteDataContext : DbContext
{
    public SqliteDataContext(DbContextOptions<SqliteDataContext> options)
            : base(options)
    {
    }

    public DbSet<ApiCallTracker> ApiCallTracker { get; set; }
}
