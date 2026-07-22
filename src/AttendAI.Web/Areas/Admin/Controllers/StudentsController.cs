using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class StudentsController : AdminControllerBase
{
    private readonly IAcademicLookupService _lookupService;
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService, IAcademicLookupService lookupService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _studentService = studentService;
        _lookupService = lookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] StudentQuery query, CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(await _studentService.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, cancellationToken);
        return student is null ? NotFound() : View(student);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateLookupsAsync(cancellationToken);
        return View(new StudentAccountCommand());
    }

    [HttpPost]
    public async Task<IActionResult> Create(StudentAccountCommand command, CancellationToken cancellationToken)
    {
        var result = await _studentService.CreateAccountAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("StudentCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var student = await _studentService.GetByIdAsync(id, cancellationToken);
        if (student is null)
        {
            return NotFound();
        }

        await PopulateLookupsAsync(cancellationToken);
        return View(new StudentProfileCommand
        {
            StudentNumber = student.StudentNumber,
            NameEnglish = student.NameEnglish,
            NameArabic = student.NameArabic,
            DepartmentId = student.DepartmentId,
            EnrollmentYear = student.EnrollmentYear,
            AcademicLevel = student.AcademicLevel,
            RowVersion = student.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, StudentProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _studentService.UpdateProfileAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            await PopulateLookupsAsync(cancellationToken);
            return View(command);
        }

        FlashSuccess("StudentUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public IActionResult ResetPassword(Guid id)
        => View(new TemporaryPasswordCommand());

    [HttpPost]
    public async Task<IActionResult> ResetPassword(Guid id, TemporaryPasswordCommand command, CancellationToken cancellationToken)
    {
        var result = await _studentService.ResetTemporaryPasswordAsync(id, command, cancellationToken);
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
        var result = await _studentService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("StudentActivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _studentService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("StudentDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Enrollments(Guid id)
        => RedirectToAction("Index", "Enrollments", new { area = "Admin", StudentId = id });

    private async Task PopulateLookupsAsync(CancellationToken cancellationToken)
        => ViewBag.Departments = await _lookupService.GetActiveDepartmentsAsync(cancellationToken);
}
