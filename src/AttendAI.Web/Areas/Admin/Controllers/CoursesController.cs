using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class CoursesController : AdminControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IAcademicLookupService _lookupService;

    public CoursesController(ICourseService courseService, IAcademicLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _courseService = courseService;
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CourseQuery query, CancellationToken cancellationToken)
    {
        ViewBag.Departments = await _lookupService.GetActiveDepartmentsAsync(cancellationToken);
        return View(await _courseService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var course = await _courseService.GetByIdAsync(id, cancellationToken);
        return course is null ? NotFound() : View(course);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(new CourseCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CourseCommand command, CancellationToken cancellationToken)
    {
        var result = await _courseService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("CourseCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var course = await _courseService.GetByIdAsync(id, cancellationToken);
        if (course is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(cancellationToken);
        return View(new CourseCommand
        {
            Code = course.Code,
            NameEnglish = course.NameEnglish,
            NameArabic = course.NameArabic,
            DescriptionEnglish = course.DescriptionEnglish,
            DescriptionArabic = course.DescriptionArabic,
            CreditHours = course.CreditHours,
            DepartmentId = course.DepartmentId,
            RowVersion = course.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, CourseCommand command, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("CourseUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("CourseActivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _courseService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("CourseDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(CancellationToken cancellationToken)
        => ViewBag.Departments = await _lookupService.GetActiveDepartmentsAsync(cancellationToken);
}
