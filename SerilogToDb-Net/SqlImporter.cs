using System;
using System.Collections.Generic;
using System.Text;

namespace SerilogToDb_Net
{
    using Microsoft.Data.SqlClient;
    using System.Data;

    public class SqlImporter
    {
        private readonly string _connectionString;

        public SqlImporter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateTableIfMissingAsync(string tableName)
        {
            var sql =
    $"""
IF NOT EXISTS
(
    SELECT *
    FROM sys.tables
    WHERE name = '{tableName}'
)
BEGIN

CREATE TABLE dbo.[{tableName}]
(
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,

    LogTimeUtc DATETIME2 NULL,
    LogLevel NVARCHAR(20) NULL,

    MessageTemplate NVARCHAR(MAX) NULL,
    ExceptionText NVARCHAR(MAX) NULL,

    ApplicationName NVARCHAR(200) NULL,
    SourceContext NVARCHAR(1000) NULL,

    Context NVARCHAR(500) NULL,
    Detail NVARCHAR(MAX) NULL,

    MachineName NVARCHAR(200) NULL,

    MemoryUsage BIGINT NULL,

    ProcessId INT NULL,
    ThreadId INT NULL,

    Version NVARCHAR(100) NULL,

    UserName NVARCHAR(500) NULL,
    EnvironmentUserName NVARCHAR(500) NULL,

    ClientGuid UNIQUEIDENTIFIER NULL,

    SqlErrorNumber INT NULL,

    RawJson NVARCHAR(MAX) NOT NULL
);

CREATE INDEX IX_{tableName}_Time
    ON dbo.[{tableName}] (LogTimeUtc DESC);

CREATE INDEX IX_{tableName}_LevelTime
    ON dbo.[{tableName}] (LogLevel, LogTimeUtc DESC);

CREATE INDEX IX_{tableName}_SourceContext
    ON dbo.[{tableName}] (SourceContext, LogTimeUtc DESC);

CREATE INDEX IX_{tableName}_Memory
    ON dbo.[{tableName}] (MemoryUsage DESC);

CREATE INDEX IX_{tableName}_User
    ON dbo.[{tableName}] (UserName, LogTimeUtc DESC);

END
""";

            await using var con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            await new SqlCommand(sql, con)
                .ExecuteNonQueryAsync();
        }

        public async Task BulkInsertAsync(string tableName, List<SerilogEvent> events)
        {
            var dt = new DataTable();

            dt.Columns.Add("LogTimeUtc", typeof(DateTime));
            dt.Columns.Add("LogLevel");
            dt.Columns.Add("MessageTemplate");
            dt.Columns.Add("ExceptionText");
            dt.Columns.Add("ApplicationName");
            dt.Columns.Add("SourceContext");
            dt.Columns.Add("Context");
            dt.Columns.Add("Detail");
            dt.Columns.Add("MachineName");
            dt.Columns.Add("MemoryUsage", typeof(long));
            dt.Columns.Add("ProcessId", typeof(int));
            dt.Columns.Add("ThreadId", typeof(int));
            dt.Columns.Add("Version");
            dt.Columns.Add("UserName");
            dt.Columns.Add("EnvironmentUserName");
            dt.Columns.Add("ClientGuid", typeof(Guid));
            dt.Columns.Add("SqlErrorNumber", typeof(int));
            dt.Columns.Add("RawJson");

            foreach (var e in events)
            {
                dt.Rows.Add(
                    e.LogTimeUtc,
                    e.LogLevel,
                    e.MessageTemplate,
                    e.ExceptionText,
                    e.ApplicationName,
                    e.SourceContext,
                    e.Context,
                    e.Detail,
                    e.MachineName,
                    e.MemoryUsage,
                    e.ProcessId,
                    e.ThreadId,
                    e.Version,
                    e.UserName,
                    e.EnvironmentUserName,
                    e.ClientGuid,
                    e.SqlErrorNumber,
                    e.RawJson);
            }

            await using var con = new SqlConnection(_connectionString);

            await con.OpenAsync();

            using var bulk = new SqlBulkCopy(con);

            bulk.DestinationTableName = $"dbo.[{tableName}]";

            // Explicit mappings
            bulk.ColumnMappings.Add("LogTimeUtc", "LogTimeUtc");
            bulk.ColumnMappings.Add("LogLevel", "LogLevel");
            bulk.ColumnMappings.Add("MessageTemplate", "MessageTemplate");
            bulk.ColumnMappings.Add("ExceptionText", "ExceptionText");
            bulk.ColumnMappings.Add("ApplicationName", "ApplicationName");
            bulk.ColumnMappings.Add("SourceContext", "SourceContext");
            bulk.ColumnMappings.Add("Context", "Context");
            bulk.ColumnMappings.Add("Detail", "Detail");
            bulk.ColumnMappings.Add("MachineName", "MachineName");
            bulk.ColumnMappings.Add("MemoryUsage", "MemoryUsage");
            bulk.ColumnMappings.Add("ProcessId", "ProcessId");
            bulk.ColumnMappings.Add("ThreadId", "ThreadId");
            bulk.ColumnMappings.Add("Version", "Version");
            bulk.ColumnMappings.Add("UserName", "UserName");
            bulk.ColumnMappings.Add("EnvironmentUserName", "EnvironmentUserName");
            bulk.ColumnMappings.Add("ClientGuid", "ClientGuid");
            bulk.ColumnMappings.Add("SqlErrorNumber", "SqlErrorNumber");
            bulk.ColumnMappings.Add("RawJson", "RawJson");

            await bulk.WriteToServerAsync(dt);
        }
    }
}
