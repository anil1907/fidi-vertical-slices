using ErrorOr;

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
public class DeletePatientController : ApiControllerBase
{
    [HttpDelete("/api/patients/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return ApiResult(await Mediator.Send(new DeletePatientCommand(id)));
    }
}

public record DeletePatientCommand(Guid Id) : IRequest<ApiResponse<bool>>;

internal sealed class DeletePatientCommandHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService)
    : IRequestHandler<DeletePatientCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await context.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.CreatedBy == currentUserService.UserId!, cancellationToken);

        if (patient is null)
        {
            return ApiResponse<bool>.Fail("Danışan bulunamadı.");
        }

        context.Patients.Remove(patient);
        await context.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true);
    }
}