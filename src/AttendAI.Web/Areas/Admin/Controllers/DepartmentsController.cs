using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class DepartmentsController : AdminControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] DepartmentQuery query, CancellationToken cancellationToken)
        => View(await _departmentService.GetPagedAsync(query, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetByIdAsync(id, cancellationToken);
        return department is null ? NotFound() : View(department);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new DepartmentCommand());

    [HttpPost]
    public async Task<IActionResult> Create(DepartmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _departmentService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("DepartmentCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetByIdAsync(id, cancellationToken);
        if (department is null)
        {
            return NotFound();
        }

        return View(new DepartmentCommand
        {
            Code = department.Code,
            NameEnglish = department.NameEnglish,
            NameArabic = department.NameArabic,
            DescriptionEnglish = department.DescriptionEnglish,
            DescriptionArabic = department.DescriptionArabic,
            RowVersion = department.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, DepartmentCommand command, CancellationToken cancellationToken)
    {
        var result = await _departmentService.UpdateAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("DepartmentUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            FlashSuccess("DepartmentActivated");
        }
        else
        {
            FlashFailure(result);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            FlashSuccess("DepartmentDeactivated");
        }
        else
        {
            FlashFailure(result);
        }

        return RedirectToAction(nameof(Index));
    }
}
