using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Patients;

public class DeletePatientController : ApiControllerBase
{
    [HttpDelete("/api/patients/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await Mediator.Send(new DeletePatientCommand(id));

        return result.Match(_ => NoContent(), Problem);
    }
}

public record DeletePatientCommand(int Id) : IRequest<ErrorOr<Success>>;

internal sealed class DeletePatientCommandHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeletePatientCommand, ErrorOr<Success>>
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ErrorOr<Success>> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId, cancellationToken);

        if (patient is null)
        {
            return Error.NotFound(description: "Patient not found.");
        }

        _context.Patients.Remove(patient);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}