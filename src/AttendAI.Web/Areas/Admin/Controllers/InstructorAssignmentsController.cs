using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class InstructorAssignmentsController : AdminControllerBase
{
    private readonly IInstructorAssignmentService _assignmentService;
    private readonly IAcademicLookupService _lookupService;

    public InstructorAssignmentsController(
        IInstructorAssignmentService assignmentService,
        IAcademicLookupService lookupService,
        IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _assignmentService = assignmentService;
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] InstructorAssignmentQuery query, CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(await _assignmentService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.GetByIdAsync(id, cancellationToken);
        return assignment is null ? NotFound() : View(assignment);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(new InstructorAssignmentCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(InstructorAssignmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("AssignmentCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> SetPrimary(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _assignmentService.GetByIdAsync(id, cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        return View(new InstructorAssignmentPrimaryCommand
        {
            IsPrimary = assignment.IsPrimary,
            RowVersion = assignment.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> SetPrimary(Guid id, InstructorAssignmentPrimaryCommand command, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.SetPrimaryAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("AssignmentPrimaryChanged");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("AssignmentDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(CancellationToken cancellationToken)
    {
        ViewBag.Instructors = await _lookupService.GetActiveInstructorsAsync(cancellationToken);
        ViewBag.Sections = await _lookupService.GetActiveSectionsAsync(cancellationToken);
        ViewBag.Courses = await _lookupService.GetActiveCoursesAsync(cancellationToken);
    }
}
