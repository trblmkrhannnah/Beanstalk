using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Beanstalk.Database.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                      ?? "Host=localhost;Database=beanstalk;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connStr);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}