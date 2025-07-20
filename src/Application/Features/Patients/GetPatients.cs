using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;
using VerticalSliceArchitecture.Application.Common.Security;

namespace VerticalSliceArchitecture.Application.Features.Patients;

[Authorize]
public class GetPatientsController : ApiControllerBase
{
    [HttpGet("/api/patients")]
    public async Task<IActionResult> Get()
    {
        var result = await Mediator.Send(new GetPatientsQuery());

        return result.Match(Ok, Problem);
    }
}

public record PatientDto(Guid Id, string Name, int Age, string Phone, string Email, string? Notes, DateTime? LastVisit);

public record GetPatientsQuery : IRequest<ErrorOr<List<PatientDto>>>;

internal sealed class GetPatientsQueryHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetPatientsQuery, ErrorOr<List<PatientDto>>>
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ErrorOr<List<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);

        var patients = await _context.Patients
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Name)
            .Select(p => new PatientDto(p.Id, p.Name, p.Age, p.Phone, p.Email, p.Notes, p.LastVisit))
            .ToListAsync(cancellationToken);

        return patients;
    }
}