using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Common.Security;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Patients;

[Authorize]
public class GetPatientsController : ApiControllerBase
{
    [HttpGet("/api/patients")]
    public async Task<IActionResult> Get()
    {
        return ApiResult(await Mediator.Send(new GetPatientsQuery()));
    }
}

public record PatientDto(Guid Id, string Name, int Age, string Phone, string Email, string? Notes, DateTime? LastVisit);

public record GetPatientsQuery : IRequest<ApiResponse<List<PatientDto>>>;

internal sealed class GetPatientsQueryHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPatientsQuery, ApiResponse<List<PatientDto>>>
{
    public async Task<ApiResponse<List<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(currentUserService.UserId!);

        var patients = await context.Patients
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .Select(p => new PatientDto(p.Id, p.Name, p.Age, p.Phone, p.Email, p.Notes, p.LastVisit))
            .ToListAsync(cancellationToken);

        return ApiResponse<List<PatientDto>>.Success(patients);
    }
}