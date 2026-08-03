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

    -- Core fields
    LogTimeUtc DATETIME2 NULL,
    LogLevel NVARCHAR(20) NULL,

    MessageTemplate NVARCHAR(MAX) NULL,
    ExceptionText NVARCHAR(MAX) NULL,
    ExceptionType NVARCHAR(500) NULL,

    -- Application context
    ApplicationName NVARCHAR(200) NULL,
    SourceContext NVARCHAR(1000) NULL,

    Context NVARCHAR(500) NULL,
    Detail NVARCHAR(MAX) NULL,

    -- Environment
    MachineName NVARCHAR(200) NULL,

    MemoryUsage BIGINT NULL,
    MemoryGb INT NULL,

    ProcessId INT NULL,
    ThreadId INT NULL,

    Version NVARCHAR(100) NULL,

    UserName NVARCHAR(500) NULL,
    EnvironmentUserName NVARCHAR(500) NULL,

    ClientGuid UNIQUEIDENTIFIER NULL,

    -- General SQL fields
    SqlErrorNumber INT NULL,

    ShortSql NVARCHAR(MAX) NULL,
    SqlText NVARCHAR(MAX) NULL,
    SqlResult INT NULL,
    SqlParameters NVARCHAR(MAX) NULL,
    ConnectionString NVARCHAR(MAX) NULL,

    -- Generic Serilog rendering values (@r)
    Renderings NVARCHAR(MAX) NULL,

    -- Interface / integration logs
    EndpointUrl NVARCHAR(2000) NULL,
    JobResult BIT NULL,

    -- Original log payload
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

CREATE INDEX IX_{tableName}_SqlResult
    ON dbo.[{tableName}] (SqlResult, LogTimeUtc DESC);

CREATE INDEX IX_{tableName}_Application
    ON dbo.[{tableName}] (ApplicationName, LogTimeUtc DESC);

CREATE INDEX IX_{tableName}_Process
    ON dbo.[{tableName}] (ProcessId, LogTimeUtc DESC);

END