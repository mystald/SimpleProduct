using Microsoft.EntityFrameworkCore;
using SimpleProduct.Server.Data;

namespace SimpleProduct.Tests.Helpers;

public static class TestDbContextFactory
{
    public static ApplicationDbContext CreateInMemoryDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new ApplicationDbContext(options);
    }
}
