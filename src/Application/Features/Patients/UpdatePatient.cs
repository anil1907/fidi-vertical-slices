using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Patients;

public class UpdatePatientController : ApiControllerBase
{
    [HttpPut("/api/patients/{id}")]
    public async Task<IActionResult> Update(Guid id, UpdatePatientCommand command)
    {
        if (id != command.Id)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Not matching ids");
        }

        var result = await Mediator.Send(command);

        return result.Match(_ => NoContent(), Problem);
    }
}

public record UpdatePatientCommand(Guid Id, string Name, int Age, string Phone, string Email, string? Notes, DateTime? LastVisit)
    : IRequest<ErrorOr<Success>>;

internal sealed class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(c => c.Phone)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();
    }
}

internal sealed class UpdatePatientCommandHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdatePatientCommand, ErrorOr<Success>>
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ErrorOr<Success>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId, cancellationToken);

        if (patient is null)
        {
            return Error.NotFound(description: "Patient not found.");
        }

        patient.Name = request.Name;
        patient.Age = request.Age;
        patient.Phone = request.Phone;
        patient.Email = request.Email;
        patient.Notes = request.Notes;
        patient.LastVisit = request.LastVisit;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}