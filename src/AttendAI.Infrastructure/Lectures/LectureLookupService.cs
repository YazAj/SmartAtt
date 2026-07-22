using AttendAI.Application.Common.Models;
using AttendAI.Application.Lectures;
using AttendAI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AttendAI.Infrastructure.Lectures;

public sealed class LectureLookupService : ILectureLookupService
{
    private readonly ApplicationDbContext _dbContext;

    public LectureLookupService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LookupItem>> GetActiveClassroomsAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Classrooms
            .AsNoTracking()
            .Where(classroom => classroom.IsActive)
            .OrderBy(classroom => classroom.Code)
            .Select(classroom => new LookupItem(
                classroom.Id.ToString(),
                classroom.Code + " - " + classroom.BuildingNameEnglish + " / " + classroom.BuildingNameArabic + " " + classroom.RoomNumber))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LookupItem>> GetLectureSchedulesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.LectureSchedules
            .AsNoTracking()
            .Include(schedule => schedule.Section)
                .ThenInclude(section => section!.Course)
            .Include(schedule => schedule.Instructor)
            .Include(schedule => schedule.Classroom)
            .Where(schedule => schedule.IsActive)
            .OrderBy(schedule => schedule.Section!.Course!.Code)
            .ThenBy(schedule => schedule.Section!.SectionNumber)
            .ThenBy(schedule => schedule.DayOfWeek)
            .ThenBy(schedule => schedule.StartTime)
            .Select(schedule => new LookupItem(
                schedule.Id.ToString(),
                schedule.Section!.Course!.Code + "-" + schedule.Section.SectionNumber + " | " +
                schedule.Instructor!.EmployeeNumber + " | " +
                schedule.Classroom!.Code + " | " +
                schedule.DayOfWeek + " " + schedule.StartTime.ToString("HH:mm", CultureInfo.InvariantCulture)))
            .ToListAsync(cancellationToken);
}
