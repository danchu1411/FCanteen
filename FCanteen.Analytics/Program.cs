using FCanteen.Data;
using FCanteen.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var configuration =
    new ConfigurationBuilder()
        .SetBasePath(
            AppContext.BaseDirectory)
        .AddJsonFile(
            "appsettings.json",
            optional: false)
        .Build();

var connectionString =
    configuration
        .GetConnectionString("FCanteen")
    ?? throw new InvalidOperationException(
        "Connection string 'FCanteen' was not found.");

var options =
    new DbContextOptionsBuilder
        <FCanteenContext>()
        .UseSqlServer(
            connectionString)
        .Options;

await using var db =
    new FCanteenContext(options);

Console.WriteLine(
    "FCanteen Lab02 - Large Data Seeder");

Console.WriteLine();

await Lab02DataSeeder.SeedAsync(db);

Console.WriteLine();

Console.WriteLine(
    "Press Enter to exit.");

Console.ReadLine();