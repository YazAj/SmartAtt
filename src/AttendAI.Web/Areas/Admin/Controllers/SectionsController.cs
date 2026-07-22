using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class SectionsController : AdminControllerBase
{
    private readonly IAcademicLookupService _lookupService;
    private readonly ISectionService _sectionService;

    public SectionsController(ISectionService sectionService, IAcademicLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _sectionService = sectionService;
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] SectionQuery query, CancellationToken cancellationToken)
    {
        ViewBag.Courses = await _lookupService.GetActiveCoursesAsync(cancellationToken);
        return View(await _sectionService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var section = await _sectionService.GetByIdAsync(id, cancellationToken);
        return section is null ? NotFound() : View(section);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(new SectionCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(SectionCommand command, CancellationToken cancellationToken)
    {
        var result = await _sectionService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("SectionCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var section = await _sectionService.GetByIdAsync(id, cancellationToken);
        if (section is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(cancellationToken);
        return View(new SectionCommand
        {
            CourseId = section.CourseId,
            SectionNumber = section.SectionNumber,
            AcademicYear = section.AcademicYear,
            Semester = section.Semester,
            Capacity = section.Capacity,
            RowVersion = section.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, SectionCommand command, CancellationToken cancellationToken)
    {
        var result = await _sectionService.UpdateAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("SectionUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sectionService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("SectionActivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sectionService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("SectionDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult EnrolledStudents(Guid id)
        => RedirectToAction("Index", "Enrollments", new { area = "Admin", SectionId = id });

    [HttpGet]
    public IActionResult AssignedInstructors(Guid id)
        => RedirectToAction("Index", "InstructorAssignments", new { area = "Admin", SectionId = id });

    private async Task PopulateLookupsAsync(CancellationToken cancellationToken)
        => ViewBag.Courses = await _lookupService.GetActiveCoursesAsync(cancellationToken);
}
