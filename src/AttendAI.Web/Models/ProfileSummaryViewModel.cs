namespace AttendAI.Web.Models;

public sealed record ProfileSummaryViewModel(
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    bool IsDisabled);
