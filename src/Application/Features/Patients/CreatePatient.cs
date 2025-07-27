using ErrorOr;
using FluentValidation;
using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Domain.Patients;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Patients;

[Authorize]
public class CreatePatientController : ApiControllerBase
{
    [HttpPost("/api/patients")]
    public async Task<IActionResult> Create(CreatePatientCommand command)
    {
        return ApiResult(await Mediator.Send(command));
    }
}

public record CreatePatientCommand(string Name, int Age, string Phone, string Email, string? Notes, DateTime? LastVisit)
    : IRequest<ApiResponse<Guid>>;

internal sealed class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
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

internal sealed class CreatePatientCommandHandler(
    ApplicationDbContext context)
    : IRequestHandler<CreatePatientCommand, ApiResponse<Guid>>
{
    public async Task<ApiResponse<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = new Patient
        {
            Name = request.Name,
            Age = request.Age,
            Phone = request.Phone,
            Email = request.Email,
            Notes = request.Notes,
            LastVisit = request.LastVisit,
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.Success(patient.Id);
    }
}