using AttendAI.Application.Academic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace AttendAI.Web.Areas.Admin.Controllers;

public sealed class ClassroomsController : AdminControllerBase
{
    private readonly IClassroomService _classroomService;

    public ClassroomsController(IClassroomService classroomService, IStringLocalizer<SharedResource> localizer)
        : base(localizer)
    {
        _classroomService = classroomService;
    }

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] ClassroomQuery query, CancellationToken cancellationToken)
        => View(await _classroomService.GetPagedAsync(query, cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var classroom = await _classroomService.GetByIdAsync(id, cancellationToken);
        return classroom is null ? NotFound() : View(classroom);
    }

    [HttpGet]
    public IActionResult Create()
        => View(new ClassroomCommand());

    [HttpPost]
    public async Task<IActionResult> Create(ClassroomCommand command, CancellationToken cancellationToken)
    {
        var result = await _classroomService.CreateAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("ClassroomCreated");
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var classroom = await _classroomService.GetByIdAsync(id, cancellationToken);
        if (classroom is null)
        {
            return NotFound();
        }

        return View(new ClassroomCommand
        {
            Code = classroom.Code,
            BuildingNameEnglish = classroom.BuildingNameEnglish,
            BuildingNameArabic = classroom.BuildingNameArabic,
            RoomNumber = classroom.RoomNumber,
            Capacity = classroom.Capacity,
            Latitude = classroom.Latitude,
            Longitude = classroom.Longitude,
            RowVersion = classroom.RowVersion
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, ClassroomCommand command, CancellationToken cancellationToken)
    {
        var result = await _classroomService.UpdateAsync(id, command, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return View(command);
        }

        FlashSuccess("ClassroomUpdated");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _classroomService.ActivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("ClassroomActivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _classroomService.DeactivateAsync(id, cancellationToken);
        if (result.Succeeded) FlashSuccess("ClassroomDeactivated"); else FlashFailure(result);
        return RedirectToAction(nameof(Index));
    }
}
