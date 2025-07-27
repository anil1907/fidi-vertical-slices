using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Common.Security;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Patients;

[Authorize]
public class UpdatePatientController : ApiControllerBase
{
    [HttpPut("/api/patients/{id}")]
    public async Task<IActionResult> Update(Guid id, UpdatePatientCommand command)
    {
        return ApiResult(await Mediator.Send(command));
    }
}

public record UpdatePatientCommand(Guid Id, string Name, int Age, string Phone, string Email, string? Notes, DateTime? LastVisit)
    : IRequest<ApiResponse<bool>>;

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
    : IRequestHandler<UpdatePatientCommand, ApiResponse<bool>>
{
    private readonly ApplicationDbContext _context = context;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public async Task<ApiResponse<bool>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.UserId == userId, cancellationToken);

        if (patient is null)
        {
            return ApiResponse<bool>.Fail("Patient not found.");
        }

        patient.Name = request.Name;
        patient.Age = request.Age;
        patient.Phone = request.Phone;
        patient.Email = request.Email;
        patient.Notes = request.Notes;
        patient.LastVisit = request.LastVisit;

        await _context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true);
    }
}