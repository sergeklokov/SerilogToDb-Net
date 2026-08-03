using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SerilogToDb_Net
{
    using Microsoft.Data.SqlClient;
    using System.Data;
    using System.Text.RegularExpressions;

    public class SqlImporter
    {
        private readonly string _connectionString;

        public SqlImporter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task CreateTableIfMissingAsync(string tableName)
        {
            tableName = Regex.Replace(
                tableName,
                @"[^A-Za-z0-9_]",
                "_");

            var sql =
        $$"""
IF NOT EXISTS
(
    SELECT *
    FROM sys.tables
    WHERE name = '{{tableName}}'
)
BEGIN

CREATE TABLE dbo.[{{tableName}}]
(
    LogId BIGINT IDENTITY(1,1) PRIMARY KEY,
    LogTimeUtc DATETIME2 NULL,
    LogLevel NVARCHAR(20) NULL,
    MessageTemplate NVARCHAR(MAX) NULL,
    ExceptionText NVARCHAR(MAX) NULL,
    ExceptionType NVARCHAR(500) NULL,
    ApplicationName NVARCHAR(200) NULL,
    SourceContext NVARCHAR(1000) NULL,
    Context NVARCHAR(500) NULL,
    Detail NVARCHAR(MAX) NULL,
    MachineName NVARCHAR(200) NULL,

    MemoryUsage BIGINT NULL,
    MemoryGb INT NULL,

    ProcessId INT NULL,
    ThreadId INT NULL,

    Version NVARCHAR(100) NULL,

    UserName NVARCHAR(500) NULL,
    EnvironmentUserName NVARCHAR(500) NULL,

    ClientGuid UNIQUEIDENTIFIER NULL,

    SqlErrorNumber INT NULL,

    ShortSql NVARCHAR(MAX) NULL,
    SqlText NVARCHAR(MAX) NULL,
    SqlResult INT NULL,
    SqlParameters NVARCHAR(MAX) NULL,
    ConnectionString NVARCHAR(MAX) NULL,

    Renderings NVARCHAR(MAX) NULL,

    EndpointUrl NVARCHAR(2000) NULL,
    JobResult BIT NULL,

    RawJson NVARCHAR(MAX) NOT NULL
);

CREATE INDEX IX_{{tableName}}_Time
    ON dbo.[{{tableName}}] (LogTimeUtc DESC);

CREATE INDEX IX_{{tableName}}_LevelTime
    ON dbo.[{{tableName}}] (LogLevel, LogTimeUtc DESC);

CREATE INDEX IX_{{tableName}}_SourceContext
    ON dbo.[{{tableName}}] (SourceContext, LogTimeUtc DESC);

CREATE INDEX IX_{{tableName}}_Memory
    ON dbo.[{{tableName}}] (MemoryUsage DESC);

CREATE INDEX IX_{{tableName}}_User
    ON dbo.[{{tableName}}] (UserName, LogTimeUtc DESC);

CREATE INDEX IX_{{tableName}}_SqlResult
    ON dbo.[{{tableName}}] (SqlResult, LogTimeUtc DESC);

CREATE INDEX IX_{{tableName}}_Application
    ON dbo.[{{tableName}}] (ApplicationName, LogTimeUtc DESC);

CREATE INDEX IX_{{tableName}}_Process
    ON dbo.[{{tableName}}] (ProcessId, LogTimeUtc DESC);

END
""";

            await using var con = new SqlConnection(_connectionString);
            await con.OpenAsync();
            await new SqlCommand(sql, con).ExecuteNonQueryAsync();
        }

        public async Task BulkInsertAsync(string tableName, List<SerilogEvent> events)
        {
            var dt = new DataTable();

            dt.Columns.Add("LogTimeUtc", typeof(DateTime));
            dt.Columns.Add("LogLevel");
            dt.Columns.Add("MessageTemplate");
            dt.Columns.Add("ExceptionText");
            dt.Columns.Add("ExceptionType");
            dt.Columns.Add("ApplicationName");
            dt.Columns.Add("SourceContext");
            dt.Columns.Add("Context");
            dt.Columns.Add("Detail");
            dt.Columns.Add("MachineName");
            dt.Columns.Add("MemoryUsage", typeof(long));
            dt.Columns.Add("MemoryGb", typeof(int));
            dt.Columns.Add("ProcessId", typeof(int));
            dt.Columns.Add("ThreadId", typeof(int));
            dt.Columns.Add("Version");
            dt.Columns.Add("UserName");
            dt.Columns.Add("EnvironmentUserName");
            dt.Columns.Add("ClientGuid", typeof(Guid));
            dt.Columns.Add("SqlErrorNumber", typeof(int));
            dt.Columns.Add("ShortSql", typeof(string));
            dt.Columns.Add("SqlText", typeof(string));
            dt.Columns.Add("SqlResult", typeof(int));
            dt.Columns.Add("SqlParameters", typeof(string));
            dt.Columns.Add("ConnectionString", typeof(string));
            dt.Columns.Add("Renderings", typeof(string));
            dt.Columns.Add("EndpointUrl", typeof(string));
            dt.Columns.Add("JobResult", typeof(bool));

            dt.Columns.Add("RawJson");

            foreach (var e in events)
            {
                dt.Rows.Add(
                    e.LogTimeUtc,
                    e.LogLevel,
                    e.MessageTemplate,
                    e.ExceptionText,
                    e.ExceptionType,
                    e.ApplicationName,
                    e.SourceContext,
                    e.Context,
                    e.Detail,
                    e.MachineName,
                    e.MemoryUsage,
                    e.MemoryGb,
                    e.ProcessId,
                    e.ThreadId,
                    e.Version,
                    e.UserName,
                    e.EnvironmentUserName,
                    e.ClientGuid,
                    e.SqlErrorNumber,
                    e.ShortSql,
                    e.SqlText,
                    e.SqlResult,
                    e.SqlParameters,
                    e.ConnectionString,
                    e.Renderings,
                    e.EndpointUrl,
                    e.JobResult,
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
            bulk.ColumnMappings.Add("MemoryGb", "MemoryGb");
            bulk.ColumnMappings.Add("ProcessId", "ProcessId");
            bulk.ColumnMappings.Add("ThreadId", "ThreadId");
            bulk.ColumnMappings.Add("Version", "Version");
            bulk.ColumnMappings.Add("UserName", "UserName");
            bulk.ColumnMappings.Add("EnvironmentUserName", "EnvironmentUserName");
            bulk.ColumnMappings.Add("ClientGuid", "ClientGuid");
            bulk.ColumnMappings.Add("SqlErrorNumber", "SqlErrorNumber");

            bulk.ColumnMappings.Add("ExceptionType", "ExceptionType");
            bulk.ColumnMappings.Add("ShortSql", "ShortSql");
            bulk.ColumnMappings.Add("SqlText", "SqlText");
            bulk.ColumnMappings.Add("SqlResult", "SqlResult");
            bulk.ColumnMappings.Add("SqlParameters", "SqlParameters");
            bulk.ColumnMappings.Add("ConnectionString", "ConnectionString");
            bulk.ColumnMappings.Add("Renderings", "Renderings");
            bulk.ColumnMappings.Add("EndpointUrl", "EndpointUrl");
            bulk.ColumnMappings.Add("JobResult", "JobResult");
            bulk.ColumnMappings.Add("RawJson", "RawJson");

            await bulk.WriteToServerAsync(dt);
        }
    }
}
