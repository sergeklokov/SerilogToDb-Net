using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SerilogToDb_Net
{
    public class LogParser
    {
        //public List<SerilogEvent> Parse(string filePath)
        //{
        //    var firstLine = File.ReadLines(filePath).First();
        //    Console.WriteLine(firstLine);
        //    return new List<SerilogEvent>();
        //}


    public List<SerilogEvent> Parse(string filePath)
    {
        var result = new List<SerilogEvent>();

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                using var doc = JsonDocument.Parse(line);

                var root = doc.RootElement;

                var evt = new SerilogEvent
                {
                    RawJson = line,

                    LogTimeUtc = root.TryGetProperty("@t", out var t)
                        ? t.GetDateTime()
                        : null,

                    LogLevel = root.TryGetProperty("@l", out var l)
                        ? l.ToString()
                        : null,

                    MessageTemplate = root.TryGetProperty("@mt", out var mt)
                        ? mt.ToString()
                        : null,

                    ExceptionText = root.TryGetProperty("@x", out var ex)
                        ? ex.ToString()
                        : null,

                    ApplicationName = root.TryGetProperty("ApplicationName", out var app)
                        ? app.ToString()
                        : null,

                    SourceContext = root.TryGetProperty("SourceContext", out var src)
                        ? src.ToString()
                        : null,

                    Context = root.TryGetProperty("Context", out var ctx)
                        ? ctx.ToString()
                        : null,

                    Detail = root.TryGetProperty("Detail", out var det)
                        ? det.ToString()
                        : null,

                    MachineName = root.TryGetProperty("MachineName", out var machine)
                        ? machine.ToString()
                        : null,

                    MemoryUsage = root.TryGetProperty("MemoryUsage", out var mem)
                        ? mem.GetInt64()
                        : null,

                    ProcessId = root.TryGetProperty("ProcessId", out var pid)
                        ? pid.GetInt32()
                        : null,

                    ThreadId = root.TryGetProperty("ThreadId", out var tid)
                        ? tid.GetInt32()
                        : null,

                    Version = root.TryGetProperty("Version", out var ver)
                        ? ver.ToString()
                        : null,

                    UserName = root.TryGetProperty("UserName", out var user)
                        ? user.ToString()
                        : null,

                    EnvironmentUserName = root.TryGetProperty("EnvironmentUserName", out var envUser)
                        ? envUser.ToString()
                        : null
                };

                if (root.TryGetProperty("ClientGuid", out var cg))
                {
                    if (Guid.TryParse(cg.ToString(), out var guid))
                    {
                        evt.ClientGuid = guid;
                    }
                }

                if (root.TryGetProperty("SqlErrorNumber", out var sqlErr))
                {
                    if (sqlErr.ValueKind == JsonValueKind.Number)
                        evt.SqlErrorNumber = sqlErr.GetInt32();
                }

                result.Add(evt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse line: {ex.Message}");
            }
        }

        Console.WriteLine($"Parsed {result.Count:N0} log entries.");

        return result;
    }


    private static string? ExtractString(string text, string pattern)
        {
            var m = Regex.Match(
                text,
                pattern,
                RegexOptions.Singleline);

            return m.Success
                ? m.Groups[1].Value.Trim()
                : null;
        }

        private static int? ExtractInt(string text, string pattern)
        {
            var s = ExtractString(text, pattern);

            return int.TryParse(s, out var v)
                ? v
                : null;
        }

        private static long? ExtractLong(string text, string pattern)
        {
            var s = ExtractString(text, pattern);

            return long.TryParse(s, out var v)
                ? v
                : null;
        }

        private static Guid? ExtractGuid(string text, string pattern)
        {
            var s = ExtractString(text, pattern);

            return Guid.TryParse(s, out var v)
                ? v
                : null;
        }

        private static DateTime? ExtractDate(string text, string pattern)
        {
            var s = ExtractString(text, pattern);

            return DateTime.TryParse(s, out var d)
                ? d
                : null;
        }
    }
}
