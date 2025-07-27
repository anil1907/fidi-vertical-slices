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
[Tags("Patients")]
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
            .WithMessage("İsim alanı boş bırakılamaz.")
            .MaximumLength(200)
            .WithMessage("İsim alanı en fazla 200 karakter olabilir.");

        RuleFor(c => c.Phone)
            .NotEmpty()
            .WithMessage("Telefon numarası boş bırakılamaz.")
            .MaximumLength(50)
            .WithMessage("Telefon numarası en fazla 50 karakter olabilir.");

        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("E-posta adresi boş bırakılamaz.")
            .EmailAddress()
            .WithMessage("Geçerli bir e-posta adresi giriniz.");
    }
}

internal sealed class UpdatePatientCommandHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdatePatientCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.CreatedBy == currentUserService.UserId!, cancellationToken);

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

        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true);
    }
}