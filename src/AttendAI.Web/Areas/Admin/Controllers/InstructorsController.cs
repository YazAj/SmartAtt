using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class InstructorsController : AdminControllerBase
{
    private readonly IInstructorService _instructorService;
    private readonly IAcademicLookupService _lookupService;

    public InstructorsController(IInstructorService instructorService, IAcademicLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _instructorService = instructorService;
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] InstructorQuery query, CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(await _instructorService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var instructor = await _instructorService.GetByIdAsync(id, cancellationToken);
        return instructor is null ? NotFound() : View(instructor);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(new InstructorAccountCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(InstructorAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await _instructorService.CreateAccountAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("InstructorCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var instructor = await _instructorService.GetByIdAsync(id, cancellationToken);
        if (instructor is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(cancellationToken);
        return View(new InstructorProfileCommand
        {
            EmployeeNumber = instructor.EmployeeNumber,
            NameEnglish = instructor.NameEnglish,
            NameArabic = instructor.NameArabic,
            DepartmentId = instructor.DepartmentId,
            AcademicTitle = instructor.AcademicTitle,
            RowVersion = instructor.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, InstructorProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _instructorService.UpdateProfileAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("InstructorUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult ResetPassword(Guid id)
        => View(new TemporaryPasswordCommand());

    [HttpPost]
    public async Task<IActionResult> ResetPassword(Guid id, TemporaryPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _instructorService.ResetTemporaryPasswordAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("TemporaryPasswordReset");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _instructorService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("InstructorActivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _instructorService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("InstructorDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Assignments(Guid id)
        => RedirectToAction("Index", "InstructorAssignments", new { area = "Admin", InstructorId = id });

    private async Task PopulateLookupsAsync(CancellationToken cancellationToken)
        => ViewBag.Departments = await _lookupService.GetActiveDepartmentsAsync(cancellationToken);
}
