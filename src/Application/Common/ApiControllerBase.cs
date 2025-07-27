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

    protected static ObjectResult OkResponse<T>(ApiResponse<T> response)
    {
        return new ObjectResult(response)
        {
            StatusCode = response.StatusCode,
        };
    }

    protected ActionResult<ApiResponse<T>> ApiResult<T>(ApiResponse<T> response)
    {
        if (response.IsSuccess)
        {
            return Ok(response);
        }

        return StatusCode(StatusCodes.Status400BadRequest, response);
    }

    protected ActionResult ApiResult<T>(ErrorOr<T> result)
    {
        var response = ApiResponse<T>.From(result);
        return StatusCode(response.StatusCode, response);
    }

    protected ActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return Problem(errors[0]);
    }

    private ObjectResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(statusCode: statusCode, title: error.Description);
    }

    private ActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        errors.ForEach(error => modelStateDictionary.AddModelError(error.Code, error.Description));

        return ValidationProblem(modelStateDictionary);
    }
}