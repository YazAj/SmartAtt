using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class EnrollmentsController : AdminControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly IAcademicLookupService _lookupService;

    public EnrollmentsController(IEnrollmentService enrollmentService, IAcademicLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _enrollmentService = enrollmentService;
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] EnrollmentQuery query, CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(await _enrollmentService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(id, cancellationToken);
        return enrollment is null ? NotFound() : View(enrollment);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(new EnrollmentCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(EnrollmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("EnrollmentCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> ChangeStatus(Guid id, CancellationToken cancellationToken)
    {
        var enrollment = await _enrollmentService.GetByIdAsync(id, cancellationToken);
        if (enrollment is null)
        {
            return NotFound();
        }

        return View(new EnrollmentStatusCommand
        {
            EnrollmentStatus = enrollment.EnrollmentStatus,
            RowVersion = enrollment.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> ChangeStatus(Guid id, EnrollmentStatusCommand command, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.ChangeStatusAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("EnrollmentStatusChanged");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("EnrollmentDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(CancellationToken cancellationToken)
    {
        ViewBag.Students = await _lookupService.GetActiveStudentsAsync(cancellationToken);
        ViewBag.Sections = await _lookupService.GetActiveSectionsAsync(cancellationToken);
        ViewBag.Courses = await _lookupService.GetActiveCoursesAsync(cancellationToken);
    }
}
