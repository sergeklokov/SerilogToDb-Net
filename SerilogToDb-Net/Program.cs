using Microsoft.Extensions.Configuration;
using SerilogToDb_Net;

if (args.Length == 0 || args.Length > 2)
{
    Console.WriteLine("Usage: SerilogToDb-Net <logfile> [tableName]");
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

var tableName = args.Length == 2
    ? args[1]
    : $"Serilog_{Path.GetFileNameWithoutExtension(filePath)}";

tableName = new string(
    tableName
        .Select(character => char.IsLetterOrDigit(character) || character == '_' ? character : '_')
        .ToArray());

if (string.IsNullOrWhiteSpace(tableName) || tableName.All(character => character == '_'))
{
    Console.WriteLine("Table name must contain at least one letter or digit.");
    return;
}

Console.WriteLine($"Table: {tableName}");

var parser = new LogParser();

var events = parser.Parse(filePath);

var importer = new SqlImporter(connectionString!);

await importer.CreateTableIfMissingAsync(tableName);

await importer.BulkInsertAsync(tableName, events);

Console.WriteLine($"Imported {events.Count} rows");