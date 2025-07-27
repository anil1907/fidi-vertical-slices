using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;

using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Features.Auth;

namespace VerticalSliceArchitecture.Application.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetService<ISender>()!;

    protected IActionResult ApiResult<T>(ApiResponse<T> response)
    {
        try
        {
            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return StatusCode(StatusCodes.Status400BadRequest, response);
        }
        catch (Exception exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<T>.Fail(exception.Message, 500));
        }
    }
}