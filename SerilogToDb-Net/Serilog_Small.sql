CREATE TABLE dbo.Serilog_Small
(
    LogId BIGINT IDENTITY(1,1) NOT NULL,

    -- Import metadata
    ImportDateUtc DATETIME2(3) NOT NULL
        CONSTRAINT DF_Serilog_Small_ImportDateUtc
        DEFAULT SYSUTCDATETIME(),
    LogSequenceNumber BIGINT NULL,

    -- Serilog fields
    LogTimeUtc DATETIME2(7) NOT NULL,
    LogLevel NVARCHAR(20) NULL,
    MessageTemplate NVARCHAR(MAX) NULL,
    RenderedMessage NVARCHAR(MAX) NULL,
    ExceptionText NVARCHAR(MAX) NULL,

    -- Context
    ApplicationName NVARCHAR(200) NULL,
    SourceContext NVARCHAR(1000) NULL,
    Context NVARCHAR(500) NULL,
    Detail NVARCHAR(MAX) NULL,

    -- Environment
    MachineName NVARCHAR(200) NULL,
    ProcessId INT NULL,
    ThreadId INT NULL,
    Version NVARCHAR(100) NULL,
    MemoryUsage BIGINT NULL,

    -- User/Session
    EnvironmentUserName NVARCHAR(500) NULL,
    UserName NVARCHAR(500) NULL,
    ClientGuid UNIQUEIDENTIFIER NULL,

    -- SQL Exception Details
    SqlErrorNumber INT NULL,
    SqlErrorState INT NULL,
    SqlErrorClass INT NULL,
    StoredProcedure NVARCHAR(500) NULL,
    ClientConnectionId UNIQUEIDENTIFIER NULL,

    -- Computed column
    HasException AS
    (
        CASE
            WHEN ExceptionText IS NULL THEN CONVERT(bit,0)
            ELSE CONVERT(bit,1)
        END
    ) PERSISTED,

    -- Original log event
    RawJson NVARCHAR(MAX) NOT NULL,

    CONSTRAINT PK_Serilog_Small
        PRIMARY KEY NONCLUSTERED (LogId)
);
GO

--------------------------------------------------------
-- Main clustered index for time-based searching
--------------------------------------------------------
CREATE CLUSTERED INDEX CIX_Serilog_Small_LogTimeUtc
ON dbo.Serilog_Small(LogTimeUtc, LogId);
GO

--------------------------------------------------------
-- Errors
--------------------------------------------------------
CREATE INDEX IX_Serilog_Small_Level_Time
ON dbo.Serilog_Small(LogLevel, LogTimeUtc DESC);
GO

CREATE INDEX IX_Serilog_Small_HasException_Time
ON dbo.Serilog_Small(HasException, LogTimeUtc DESC);
GO

--------------------------------------------------------
-- Source / Context
--------------------------------------------------------
CREATE INDEX IX_Serilog_Small_SourceContext_Time
ON dbo.Serilog_Small(SourceContext, LogTimeUtc DESC);
GO

CREATE INDEX IX_Serilog_Small_Context_Time
ON dbo.Serilog_Small(Context, LogTimeUtc DESC);
GO

--------------------------------------------------------
-- User analysis
--------------------------------------------------------
CREATE INDEX IX_Serilog_Small_User_Time
ON dbo.Serilog_Small(UserName, LogTimeUtc DESC);
GO

CREATE INDEX IX_Serilog_Small_ClientGuid_Time
ON dbo.Serilog_Small(ClientGuid, LogTimeUtc DESC);
GO

--------------------------------------------------------
-- Process analysis
--------------------------------------------------------
CREATE INDEX IX_Serilog_Small_ProcessId_Time
ON dbo.Serilog_Small(ProcessId, LogTimeUtc DESC);
GO

--------------------------------------------------------
-- Memory usage analysis
--------------------------------------------------------
CREATE INDEX IX_Serilog_Small_MemoryUsage_Time
ON dbo.Serilog_Small(MemoryUsage DESC, LogTimeUtc DESC);
GO