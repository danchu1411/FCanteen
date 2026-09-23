using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FCanteen.Data;

public class FCanteenDesignTimeFactory
    : IDesignTimeDbContextFactory<FCanteenContext>
{
    public FCanteenContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        if (!File.Exists(
                Path.Combine(basePath, "appsettings.json")))
        {
            basePath =
                Path.Combine(basePath, "FCanteen.Data");
        }

        var configuration =
            new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile(
                    "appsettings.json",
                    optional: false)
                .Build();

        var connectionString =
            configuration.GetConnectionString("FCanteen")
            ?? throw new InvalidOperationException(
                "Connection string 'FCanteen' not found.");

        var optionsBuilder =
            new DbContextOptionsBuilder<FCanteenContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new FCanteenContext(
            optionsBuilder.Options);
    }
}
