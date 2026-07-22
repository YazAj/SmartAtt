namespace AttendAI.Web.Models;

public sealed record EmptyStateViewModel(string Title, string Message, string? ActionText = null, string? ActionUrl = null);
