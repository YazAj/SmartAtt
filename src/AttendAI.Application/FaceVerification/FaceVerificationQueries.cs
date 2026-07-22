using AttendAI.Application.Common.Models;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.FaceVerification;

public sealed class AdminFaceVerificationAttemptQuery : PagedQuery
{
    public FaceVerificationOutcome? Outcome { get; set; }

    public FaceVerificationDecision? Decision { get; set; }

    public FaceVerificationPurpose? Purpose { get; set; }
}
