using System;
using System.Collections.Generic;
using System.Text;

namespace SerilogToDb_Net
{
    public class SerilogEvent
    {
        public DateTime? LogTimeUtc { get; set; }
        public string? LogLevel { get; set; }
        public string? MessageTemplate { get; set; }
        public string? ExceptionText { get; set; }

        public string? ApplicationName { get; set; }
        public string? SourceContext { get; set; }
        public string? Context { get; set; }
        public string? Detail { get; set; }

        public string? MachineName { get; set; }

        public long? MemoryUsage { get; set; }

        public int? ProcessId { get; set; }
        public int? ThreadId { get; set; }

        public string? Version { get; set; }

        public string? UserName { get; set; }
        public string? EnvironmentUserName { get; set; }

        public Guid? ClientGuid { get; set; }

        public int? SqlErrorNumber { get; set; }

        public string RawJson { get; set; } = "";
    }
}
