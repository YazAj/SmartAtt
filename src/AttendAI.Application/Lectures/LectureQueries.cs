using AttendAI.Application.Common.Models;
using AttendAI.Domain.Enums;

namespace AttendAI.Application.Lectures;

public sealed class LectureScheduleQuery : PagedQuery
{
    public Guid? SectionId { get; set; }

    public Guid? InstructorId { get; set; }

    public Guid? ClassroomId { get; set; }

    public bool? IsActive { get; set; }

    public DayOfWeek? DayOfWeek { get; set; }
}

public sealed class LectureSessionQuery : PagedQuery
{
    public Guid? SectionId { get; set; }

    public Guid? InstructorId { get; set; }

    public Guid? ClassroomId { get; set; }

    public LectureSessionStatus? Status { get; set; }
}
