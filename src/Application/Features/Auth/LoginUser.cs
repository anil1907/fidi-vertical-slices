using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VerticalSliceArchitecture.Application.Common;
using VerticalSliceArchitecture.Application.Common.Interfaces;
using VerticalSliceArchitecture.Application.Common.Models;
using VerticalSliceArchitecture.Application.Infrastructure.Persistence;

namespace VerticalSliceArchitecture.Application.Features.Auth;

public class LoginUserController : ApiControllerBase
{
    [HttpPost("/api/login")]
    public async Task<IActionResult> Login(LoginUserCommand command)
    {
        return ApiResult(await Mediator.Send(command));
    }
}

public record LoginUserCommand(string Email, string Password) : IRequest<ApiResponse<LoginUserResponse>>;

public record LoginUserResponse(string Token);

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(c => c.Password)
            .NotEmpty();
    }
}

internal sealed class LoginUserCommandHandler(
    ApplicationDbContext context,
    IJwtTokenGenerator tokenGenerator) : IRequestHandler<LoginUserCommand, ApiResponse<LoginUserResponse>>
{
    public async Task<ApiResponse<LoginUserResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            throw new Exception("test");
        }

        var valid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!valid)
        {
            return ApiResponse<LoginUserResponse>.Fail("Invalid credentials.");
        }

        var token = tokenGenerator.GenerateToken(user);
        return ApiResponse<LoginUserResponse>.Success(new LoginUserResponse(token));
    }
}
