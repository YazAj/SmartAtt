using AttendAI.Application.Academic;
using AttendAI.Application.Lectures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class LectureSchedulesController : AdminControllerBase
{
    private readonly IAcademicLookupService _academicLookupService;
    private readonly ILectureLookupService _lectureLookupService;
    private readonly ILectureScheduleService _lectureScheduleService;

    public LectureSchedulesController(
        ILectureScheduleService lectureScheduleService,
        IAcademicLookupService academicLookupService,
        ILectureLookupService lectureLookupService,
        IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _lectureScheduleService = lectureScheduleService;
        _academicLookupService = academicLookupService;
        _lectureLookupService = lectureLookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] LectureScheduleQuery query, CancellationToken cancellationToken)
    {
        await PopulateFilterLookupsAsync(cancellationToken);
        return View(await _lectureScheduleService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _lectureScheduleService.GetByIdAsync(id, cancellationToken);
        return schedule is null ? NotFound() : View(schedule);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateEditLookupsAsync(cancellationToken);
        return View(new LectureScheduleCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(LectureScheduleCommand command, CancellationToken cancellationToken)
    {
        var result = await _lectureScheduleService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateEditLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("LectureScheduleCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var schedule = await _lectureScheduleService.GetByIdAsync(id, cancellationToken);
        if (schedule is null)
        {
            return NotFound();
        }

        await PopulateEditLookupsAsync(cancellationToken);
        return View(new LectureScheduleCommand
        {
            SectionId = schedule.SectionId,
            InstructorId = schedule.InstructorId,
            ClassroomId = schedule.ClassroomId,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            EffectiveFrom = schedule.EffectiveFrom,
            EffectiveTo = schedule.EffectiveTo,
            DefaultLateThresholdMinutes = schedule.DefaultLateThresholdMinutes,
            DefaultAllowedRadiusMeters = schedule.DefaultAllowedRadiusMeters,
            RowVersion = schedule.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, LectureScheduleCommand command, CancellationToken cancellationToken)
    {
        var result = await _lectureScheduleService.UpdateAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateEditLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("LectureScheduleUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _lectureScheduleService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("LectureScheduleActivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _lectureScheduleService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("LectureScheduleDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ClassroomTimetable(Guid classroomId, CancellationToken cancellationToken)
    {
        ViewBag.Classrooms = await _lectureLookupService.GetActiveClassroomsAsync(cancellationToken);
        ViewBag.ClassroomId = classroomId;
        return View(await _lectureScheduleService.GetClassroomTimetableAsync(classroomId, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> SectionTimetable(Guid sectionId, CancellationToken cancellationToken)
    {
        ViewBag.Sections = await _academicLookupService.GetActiveSectionsAsync(cancellationToken);
        ViewBag.SectionId = sectionId;
        return View(await _lectureScheduleService.GetSectionTimetableAsync(sectionId, cancellationToken));
    }

    private async Task PopulateFilterLookupsAsync(CancellationToken cancellationToken)
    {
        ViewBag.Sections = await _academicLookupService.GetActiveSectionsAsync(cancellationToken);
        ViewBag.Instructors = await _academicLookupService.GetActiveInstructorsAsync(cancellationToken);
        ViewBag.Classrooms = await _lectureLookupService.GetActiveClassroomsAsync(cancellationToken);
    }

    private Task PopulateEditLookupsAsync(CancellationToken cancellationToken)
        => PopulateFilterLookupsAsync(cancellationToken);
}
