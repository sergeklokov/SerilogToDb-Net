using Microsoft.Extensions.Configuration;
using SerilogToDb_Net;

if (args.Length == 0)
{
    Console.WriteLine("Usage: SerilogToDb-Net <logfile>");
    return;
}

var filePath = args[0];

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

var configuration =
    new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

var connectionString =
    configuration.GetConnectionString("LogsDb");

var tableName =
    $"Serilog_{Path.GetFileNameWithoutExtension(filePath)}";

tableName = tableName
    .Replace(" ", "_")
    .Replace("-", "_");

Console.WriteLine($"Table: {tableName}");

var parser = new LogParser();

var events = parser.Parse(filePath);

var importer = new SqlImporter(connectionString!);

await importer.CreateTableIfMissingAsync(tableName);

await importer.BulkInsertAsync(tableName, events);

Console.WriteLine($"Imported {events.Count} rows");