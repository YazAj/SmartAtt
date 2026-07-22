using AttendAI.Application.Common.Models;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Biometrics;

public sealed class AdminBiometricEnrollmentQuery : PagedQuery
{
    public FaceEnrollmentStatus? Status { get; set; }
}
