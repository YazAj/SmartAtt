using System.Globalization;
using System.Text;

namespace AttendAI.Application.Reporting;

public static class SafeCsvReportWriter
{
    public const string ContentType = "text/csv; charset=utf-8";

    public static byte[] BuildAttendanceCsv(IEnumerable<AttendanceReportRowDto> rows)
    {
        var builder = new StringBuilder();
        AppendRow(
            builder,
            "Session Date",
            "Scheduled Start",
            "Scheduled End",
            "Course Code",
            "Course Name",
            "Section",
            "Student Number",
            "Student Name",
            "Instructor",
            "Classroom",
            "Status",
            "Checked In At");

        foreach (var row in rows)
        {
            AppendRow(
                builder,
                row.SessionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                row.ScheduledStartLocal.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture),
                row.ScheduledEndLocal.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture),
                row.CourseCode,
                row.CourseNameEnglish,
                row.SectionNumber,
                row.StudentNumber,
                row.StudentNameEnglish,
                row.InstructorNameEnglish,
                row.ClassroomCode,
                row.ReportStatus.ToString(),
                row.CheckedInAtLocal?.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture) ?? string.Empty);
        }

        return ToUtf8WithBom(builder.ToString());
    }

    public static byte[] BuildAuditCsv(IEnumerable<AttendanceAuditEventDto> events)
    {
        var builder = new StringBuilder();
        AppendRow(
            builder,
            "Occurred At",
            "Category",
            "Event Type",
            "Actor",
            "Subject",
            "Outcome",
            "Description");

        foreach (var item in events)
        {
            AppendRow(
                builder,
                item.OccurredAtLocal.ToString("yyyy-MM-dd HH:mm zzz", CultureInfo.InvariantCulture),
                item.Category.ToString(),
                item.EventType,
                item.Actor,
                item.Subject,
                item.Outcome,
                item.SafeDescription);
        }

        return ToUtf8WithBom(builder.ToString());
    }

    private static void AppendRow(StringBuilder builder, params string?[] fields)
    {
        for (var index = 0; index < fields.Length; index++)
        {
            if (index > 0)
            {
                builder.Append(',');
            }

            builder.Append(EscapeField(fields[index] ?? string.Empty));
        }

        builder.AppendLine();
    }

    private static string EscapeField(string value)
    {
        var safeValue = NeutralizeFormulaValue(value);
        if (safeValue.Contains('"') || safeValue.Contains(',') || safeValue.Contains('\n') || safeValue.Contains('\r'))
        {
            return $"\"{safeValue.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return safeValue;
    }

    private static string NeutralizeFormulaValue(string value)
    {
        if (value.Length == 0)
        {
            return value;
        }

        return value[0] is '=' or '+' or '-' or '@' or '\t' or '\r'
            ? $"'{value}"
            : value;
    }

    private static byte[] ToUtf8WithBom(string value)
    {
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var preamble = encoding.GetPreamble();
        var content = encoding.GetBytes(value);
        var result = new byte[preamble.Length + content.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(content, 0, result, preamble.Length, content.Length);
        return result;
    }
}
